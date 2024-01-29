using System.Text;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

using static PalDecompresses.Debug;
using static PalDecompresses.Global;
using static PalDecompresses.FileUtil;
using static PalDecompresses.Message;

namespace PalDecompresses
{
    public unsafe class PalUtil
    {
        public static bool
        PAL_IsWINVersion(
            bool* pfIsWIN95
        )
        {
            FileStream[] fps = { UTIL_OpenRequiredFile(strResFilePath + strResFileName[8]), UTIL_OpenRequiredFile(strResFilePath + strResFileName[9]), gpGlobals.f.fsF, gpGlobals.f.fsFBP, gpGlobals.f.fsFIRE, gpGlobals.f.fsMGO };
            uint data_size = 0, dos_score = 0, win_score = 0;
            bool result = FALSE;

            for (int i = 0; i < fps.Length; i++)
            {
                //
                // Find the first non-empty sub-file
                // 查找第一个非空子文件
                uint count = PAL_MKFGetChunkCount(fps[i]), j = 0, size;
                while (j < count && (size = PAL_MKFGetChunkSize(j, fps[i])) < 4) j++;
                if (j >= count) goto PAL_IsWINVersion_Exit;

                //
                // Read the content and check the compression signature
                // Note that this check is not 100% correct, however in incorrect situations,
                // the sub-file will be over 784MB if uncompressed, which is highly unlikely.
                // 读取内容并检查压缩签名
                // 注意该检查不是100%正确的，但是在不正确的情况下，
                // 如果未压缩，子文件将超过784MB，这是极不可能的。
                //
                data_size = PAL_MKFGetChunkSize(j, fps[i]);
                byte[] data = new byte[data_size];

                PAL_MKFReadChunk(&data, data_size, j, fps[i]);

                if (data[0] == 'Y' && data[1] == 'J' && data[2] == '_' && data[3] == '1')
                {
                    if (win_score > 0) goto PAL_IsWINVersion_Exit;
                    else dos_score++;
                }
                else
                {
                    if (dos_score > 0) goto PAL_IsWINVersion_Exit;
                    else win_score++;
                }

            }

            //
            // Finally check the size of object definition
            // 最后检查对象定义的大小
            data_size = PAL_MKFGetChunkSize(2, gpGlobals.f.fsSSS);
            if (data_size % sizeof(OBJECT_OTHER) == 0 && data_size % sizeof(OBJECT_OTHER_DOS) != 0 && dos_score > 0) goto PAL_IsWINVersion_Exit;
            if (data_size % sizeof(OBJECT_OTHER_DOS) == 0 && data_size % sizeof(OBJECT_OTHER) != 0 && win_score > 0) goto PAL_IsWINVersion_Exit;

            if (pfIsWIN95 != null) *pfIsWIN95 = (win_score == fps.Length) ? TRUE : FALSE;

            result = TRUE;

        PAL_IsWINVersion_Exit:
            fps[1].Dispose();
            fps[0].Dispose();

            return result;
        }

        public static void
        PAL_InitGlobals()
        /*++
          Purpose:

            Initialize global data.
            初始化全局数据。

          Parameters:

            None.
            无。

          Return value:

            0 = success, -1 = error.
            0 = 成功，-1 = 失败。

        --*/
        {
            //
            // 读取游戏资源数据
            //
            fixed (FILES* fsFiles = &gpGlobals.f)
            {
                //
                // Open files
                // 检查所需资源文件是否存在，抛出文件不存在的异常，提示用户文件不存在
                //
                fsFiles->fsFBP = UTIL_OpenRequiredFile(strResFilePath + strResFileName[0]);
                fsFiles->fsMGO = UTIL_OpenRequiredFile(strResFilePath + strResFileName[1]);
                fsFiles->fsBALL = UTIL_OpenRequiredFile(strResFilePath + strResFileName[2]);
                fsFiles->fsDATA = UTIL_OpenRequiredFile(strResFilePath + strResFileName[3]);
                fsFiles->fsF = UTIL_OpenRequiredFile(strResFilePath + strResFileName[4]);
                fsFiles->fsFIRE = UTIL_OpenRequiredFile(strResFilePath + strResFileName[5]);
                fsFiles->fsRGM = UTIL_OpenRequiredFile(strResFilePath + strResFileName[6]);
                fsFiles->fsSSS = UTIL_OpenRequiredFile(strResFilePath + strResFileName[7]);

                fixed (bool* pfIsWIN95 = &fIsWIN95)
                {

                    //
                    // Retrieve game resource version
                    // 检验游戏资源版本
                    //if (!PAL_IsWINVersion(pfIsWIN95)) return -1;
                    PAL_Failed(!PAL_IsWINVersion(pfIsWIN95), "Could not initialize global data, Failed to verify game resource version.", "PAL_InitGlobals");
                }

                //
                // 读取全局数据
                //
                fixed (GAMEDATA* lpGameData = &gpGlobals.g)
                {
                    // MKF 指定块长度
                    uint uiChunkIndex;
                    uint uiChunkLen;

                    //
                    // 读取事件
                    //
                    uiChunkIndex = 0;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsSSS);

                        // 获取块中结构体的组数
                        lpGameData->nEventObject = (uint)(uiChunkLen / sizeof(EVENTOBJECT));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binEventObject;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsSSS);

                        // 使事件指针指向其对应的字节数组
                        lpGameData->lprgEventObject = EVENTOBJECT.ToBase(lpChunkBin);

                        //if (fLogDebug) PAL_Debug("EventObject");
                    }

                    //
                    // 读取场景
                    //
                    uiChunkIndex = 1;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsSSS);

                        // 获取块中结构体的组数
                        lpGameData->nScene = (uint)(uiChunkLen / sizeof(SCENE));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binScene;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, (uint)uiChunkLen, uiChunkIndex, fsFiles->fsSSS);

                        // 使场景指针指向其对应的字节数组
                        lpGameData->lprgScene = SCENE.ToBase(lpChunkBin);

                        //if (fLogDebug) PAL_Debug("Scene");
                    }

                    //
                    // 读取对象
                    //
                    uiChunkIndex = 2;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsSSS);

                        // 获取块中结构体的组数
                        lpGameData->nObject = (uint)(fIsWIN95 ? (uiChunkLen / sizeof(OBJECT_OTHER)) : (uiChunkLen / sizeof(OBJECT_OTHER_DOS)));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binObject;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsSSS);

                        // 使对象指针指向其对应的字节数
                        if (fIsWIN95)
                        {
                            lpGameData->rgObject.other = OBJECT_OTHER.ToBase(lpChunkBin);
                            lpGameData->rgObject.player = OBJECT_PLAYER.ToBase(lpChunkBin);
                            lpGameData->rgObject.item = OBJECT_ITEM.ToBase(lpChunkBin);
                            lpGameData->rgObject.magic = OBJECT_MAGIC.ToBase(lpChunkBin);
                            lpGameData->rgObject.enemy = OBJECT_ENEMY.ToBase(lpChunkBin);
                            lpGameData->rgObject.poison = OBJECT_POISON.ToBase(lpChunkBin);
                        }
                        else
                        {
                            lpGameData->rgObject_DOS.other = OBJECT_OTHER_DOS.ToBase(lpChunkBin);
                            lpGameData->rgObject_DOS.player = OBJECT_PLAYER_DOS.ToBase(lpChunkBin);
                            lpGameData->rgObject_DOS.item = OBJECT_ITEM_DOS.ToBase(lpChunkBin);
                            lpGameData->rgObject_DOS.magic = OBJECT_MAGIC_DOS.ToBase(lpChunkBin);
                            lpGameData->rgObject_DOS.enemy = OBJECT_ENEMY_DOS.ToBase(lpChunkBin);
                            lpGameData->rgObject_DOS.poison = OBJECT_POISON_DOS.ToBase(lpChunkBin);
                        }

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("Object");
                    }

                    //
                    // 读取对话索引
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsSSS);

                        // 获取块中结构体的组数
                        lpGameData->nMsgIndex = uiChunkLen / sizeof(uint);

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binMsgIndex;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsSSS);

                        // 使对话索引指针指向其对应的字节数组
                        lpGameData->lprgMsgIndex = (uint*)lpChunkBin;

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("MsgIndex");
                    }

                    //
                    // 读取脚本
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsSSS);

                        // 获取块中结构体的组数
                        lpGameData->nScriptEntry = (uint)(uiChunkLen / sizeof(SCRIPTENTRY));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binScriptEntry;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsSSS);

                        // 使脚本指针指向其对应的字节数组
                        //lpGameData->lprgScriptEntry = SCRIPTENTRY.ToBase(lpChunkBin);
                    }

                    //
                    // 读取店铺
                    //
                    uiChunkIndex = 0;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nStore = (uint)(uiChunkLen / sizeof(STORE));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binStore;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使脚本指针指向其对应的字节数组
                        lpGameData->lprgStore = STORE.ToBase(lpChunkBin);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("Store");
                    }

                    //
                    // 读取敌方属性
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nEnemy = (uint)(uiChunkLen / sizeof(ENEMY));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binEnemy;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使脚本指针指向其对应的字节数组
                        lpGameData->lprgEnemy = ENEMY.ToBase(lpChunkBin);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("Enemy");
                    }

                    //
                    // 读取敌方队列
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nEnemyTeam = (uint)(uiChunkLen / sizeof(ENEMYTEAM));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binEnemyTeam;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使脚本指针指向其对应的字节数组
                        lpGameData->lprgEnemyTeam = ENEMYTEAM.ToBase(lpChunkBin);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("EnemyTeam");
                    }

                    //
                    // 读取我方属性
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binPlayerRoles;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        //
                        // 初始化每个队员的结构
                        //
                        // 获取我方属性结构
                        PLAYERROLES* lpPR = &lpGameData->rgPlayerRoles;

                        // 使脚本指针指向其对应的字节数组
                        ushort[]* lpLiXiaoYao = &lpPR->binLiXiaoYao;
                        ushort[]* lpZhaoLingEr = &lpPR->binZhaoLingEr;
                        ushort[]* lpLiuYueRu = &lpPR->binLinYueRu;
                        ushort[]* lpWuHou = &lpPR->binWuHou;
                        ushort[]* lpANu = &lpPR->binANu;
                        ushort[]* lpGaiLuoJiao = &lpPR->binGaiLuoJiao;

                        // 初始化字节二进制数据
                        long lSize = uiChunkLen / MAX_PLAYER_ROLES / sizeof(ushort);
                        *lpLiXiaoYao = new ushort[lSize];
                        *lpZhaoLingEr = new ushort[lSize];
                        *lpLiuYueRu = new ushort[lSize];
                        *lpWuHou = new ushort[lSize];
                        *lpANu = new ushort[lSize];
                        *lpGaiLuoJiao = new ushort[lSize];

                        ushort[]* uipChunkBin = (ushort[]*)lpChunkBin;

                        for (int i = 0; i < uiChunkLen / sizeof(ushort);)
                        {
                            for (int j = 0; j < lSize; j++)
                            {
                                (*lpLiXiaoYao)[j] = (*uipChunkBin)[i++];
                                (*lpZhaoLingEr)[j] = (*uipChunkBin)[i++];
                                (*lpLiuYueRu)[j] = (*uipChunkBin)[i++];
                                (*lpWuHou)[j] = (*uipChunkBin)[i++];
                                (*lpANu)[j] = (*uipChunkBin)[i++];
                                (*lpGaiLuoJiao)[j] = (*uipChunkBin)[i++];
                            }
                        }

                        lpPR->LiXiaoYao = PLAYER.ToBase(lpLiXiaoYao);
                        lpPR->ZhaoLingEr = PLAYER.ToBase(lpZhaoLingEr);
                        lpPR->LinYueRu = PLAYER.ToBase(lpLiuYueRu);
                        lpPR->WuHou = PLAYER.ToBase(lpWuHou);
                        lpPR->ANu = PLAYER.ToBase(lpANu);
                        lpPR->GaiLuoJiao = PLAYER.ToBase(lpGaiLuoJiao);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("Player");
                    }

                    //
                    // 读取敌方队列属性
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nMagic = (uint)(uiChunkLen / sizeof(MAGIC));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binMagic;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使脚本指针指向其对应的字节数组
                        lpGameData->lprgMagic = MAGIC.ToBase(lpChunkBin);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("Magic");
                    }

                    //
                    // 读取战场卦性
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nBattleField = (uint)(uiChunkLen / sizeof(BATTLEFIELD));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binBattleField;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使脚本指针指向其对应的字节数组
                        lpGameData->lprgBattleField = BATTLEFIELD.ToBase(lpChunkBin);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("BattleField");
                    }

                    //
                    // 读取仙术所需修行
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binLevelUpMagic;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        //
                        // 初始化每个队员的结构
                        //
                        // 获取我方属性结构
                        LEVELUPMAGIC_ALL* lpPR = &lpGameData->rgLevelUpMagic;

                        // 使脚本指针指向其对应的字节数组
                        uint[]* lpLiXiaoYao = &lpPR->binLiXiaoYao;
                        uint[]* lpZhaoLingEr = &lpPR->binZhaoLingEr;
                        uint[]* lpLiuYueRu = &lpPR->binLinYueRu;
                        uint[]* lpWuHou = &lpPR->binWuHou;
                        uint[]* lpANu = &lpPR->binANu;

                        // 初始化字节二进制数据
                        long lSize = uiChunkLen / MAX_PLAYABLE_PLAYER_ROLES / sizeof(uint);
                        *lpLiXiaoYao = new uint[lSize];
                        *lpZhaoLingEr = new uint[lSize];
                        *lpLiuYueRu = new uint[lSize];
                        *lpWuHou = new uint[lSize];
                        *lpANu = new uint[lSize];

                        uint[]* uipChunkBin = (uint[]*)lpChunkBin;

                        for (int i = 0; i < uiChunkLen / sizeof(uint);)
                        {
                            for (int j = 0; j < lSize; j++)
                            {
                                (*lpLiXiaoYao)[j] = (*uipChunkBin)[i++];
                                (*lpZhaoLingEr)[j] = (*uipChunkBin)[i++];
                                (*lpLiuYueRu)[j] = (*uipChunkBin)[i++];
                                (*lpWuHou)[j] = (*uipChunkBin)[i++];
                                (*lpANu)[j] = (*uipChunkBin)[i++];
                            }
                        }

                        lpPR->LiXiaoYao = LEVELUPMAGIC.ToBase(lpLiXiaoYao);
                        lpPR->ZhaoLingEr = LEVELUPMAGIC.ToBase(lpZhaoLingEr);
                        lpPR->LinYueRu = LEVELUPMAGIC.ToBase(lpLiuYueRu);
                        lpPR->WuHou = LEVELUPMAGIC.ToBase(lpWuHou);
                        lpPR->ANu = LEVELUPMAGIC.ToBase(lpANu);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("LevelUpMagic");
                    }

                    uiChunkIndex = 11;
                    //
                    // 读取队员战斗动作特效设定
                    //
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nFightEffect = (uint)(uiChunkLen / sizeof(FIGHTEFFECT));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binFightEffect;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使脚本指针指向其对应的字节数组
                        lpGameData->lprgFightEffect = FIGHTEFFECT.ToBase(lpChunkBin);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("FightEffect");
                    }

                    //
                    // 读取敌方战场坐标
                    //
                    uiChunkIndex = 13;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        lpGameData->nEnemyPos = (uint)(uiChunkLen / MAX_ENEMIES_IN_TEAM / sizeof(uint));

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binEnemyPos;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        //
                        // 初始化每个队员的结构
                        //
                        // 获取我方属性结构
                        ENEMYPOS* lpPR = &lpGameData->rgEnemyPos;

                        // 使脚本指针指向其对应的字节数组
                        uint[]* lpPos_One = &lpPR->binPos_One;
                        uint[]* lpPos_Two = &lpPR->binPos_Two;
                        uint[]* lpPos_Three = &lpPR->binPos_Three;
                        uint[]* lpPos_Four = &lpPR->binPos_Four;
                        uint[]* lpPos_Five = &lpPR->binPos_Five;

                        // 初始化字节二进制数据
                        uint lSize = lpGameData->nEnemyPos;
                        *lpPos_One = new uint[lSize];
                        *lpPos_Two = new uint[lSize];
                        *lpPos_Three = new uint[lSize];
                        *lpPos_Four = new uint[lSize];
                        *lpPos_Five = new uint[lSize];

                        uint[]* uipChunkBin = (uint[]*)lpChunkBin;

                        int iShip = 0;
                        int iTake = MAX_ENEMIES_IN_TEAM;

                        *lpPos_One = (*uipChunkBin).Skip(iTake * iShip++).Take(iTake).ToArray();
                        *lpPos_Two = (*uipChunkBin).Skip(iTake * iShip++).Take(iTake).ToArray();
                        *lpPos_Three = (*uipChunkBin).Skip(iTake * iShip++).Take(iTake).ToArray();
                        *lpPos_Four = (*uipChunkBin).Skip(iTake * iShip++).Take(iTake).ToArray();
                        *lpPos_Five = (*uipChunkBin).Skip(iTake * iShip++).Take(iTake).ToArray();

                        lpPR->pos_One = PALPOS.ToBase(lpPos_One);
                        lpPR->pos_Two = PALPOS.ToBase(lpPos_Two);
                        lpPR->pos_Three = PALPOS.ToBase(lpPos_Three);
                        lpPR->pos_Four = PALPOS.ToBase(lpPos_Four);
                        lpPR->pos_Five = PALPOS.ToBase(lpPos_Five);

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("EnemyPos");
                    }

                    //
                    // 读取对话索引
                    //
                    uiChunkIndex++;
                    {
                        // 获取 MKF 指定块长度
                        uiChunkLen = PAL_MKFGetChunkSize(uiChunkIndex, fsFiles->fsDATA);

                        // 获取块中结构体的组数
                        lpGameData->nLevelUpExp = uiChunkLen / sizeof(ushort);

                        // 获取块数据
                        byte[]* lpChunkBin = &lpGameData->binLevelUpExp;

                        // 初始化字节二进制数据
                        *lpChunkBin = new byte[uiChunkLen];

                        // 读取 MKF 指定块数据
                        PAL_MKFReadChunk(lpChunkBin, uiChunkLen, uiChunkIndex, fsFiles->fsDATA);

                        // 使对话索引指针指向其对应的字节数组
                        lpGameData->lprgLevelUpExp = (uint*)lpChunkBin;

                        //
                        // 输出读取到的数据
                        //
                        //if (fLogDebug) PAL_Debug("LevelUpExp");
                    }
                }
            }

            //return 0;
        }

        public static void
        PAL_SaveGlobalsAsTSV(
            string strObj
        )
        {
            string strContent = "";
            uint i, j, nRow;

            fixed (GAMEDATA* lpGameData = &gpGlobals.g)
            {
                switch (strObj)
                {
                    case "Scene":
                        {
                            //
                            // 解档场景
                            //
                            SCENE* lpScene;
                            EVENTOBJECT* lpEvent;

                            ushort wThisEventIndex;
                            ushort wNextEventIndex;

                            for (i = 0; i < lpGameData->nScene - 1; i++)
                            {
                                //
                                // 变量置空
                                //
                                strContent = "";

                                //
                                // 获取场景结构体数据
                                // 指针地址非 fixed 固定的，需实时获取
                                //
                                lpScene = lpGameData->GetScene();

                                //
                                // 拼接事件表头，行数自增
                                //
                                strContent += "$EventObjectID\t事件备注\t隐藏时间\t坐标:X\t坐标:Y\t图层\t互动触发器\t自动循环器\t碰撞状态\t互动方式\t形象编号\t每方向帧数\t" +
                                    $"面朝方向\t形象当前帧\t互动计次器\t**可偷窃**\t已行步数\t自动循环计次器{DocNewLine}";

                                //
                                // 获取事件索引
                                //
                                wThisEventIndex = lpScene[i].wEventObjectIndex;
                                wNextEventIndex = lpScene[i + 1].wEventObjectIndex;

                                //
                                // 表体
                                //
                                for (j = wThisEventIndex, nRow = 0; j < wNextEventIndex; j++, nRow++)
                                {
                                    //
                                    // 指针地址非 fixed 固定的，需实时获取
                                    //
                                    lpEvent = lpGameData->GetEventObject();

                                    //
                                    // 拼接事件数据
                                    //
                                    strContent += $"{j + 1}\t{PAL_GetSpriteName(lpEvent[j].wSpriteNum)}\t" +
                                        $"{lpEvent[j].sVanishTime}\t{lpEvent[j].x}\t{lpEvent[j].y}\t{lpEvent[j].sLayer}\t{lpEvent[j].wTriggerScript}\t" +
                                        $"{lpEvent[j].wAutoScript}\t{lpEvent[j].sState}\t{lpEvent[j].wTriggerMode}\t{lpEvent[j].wSpriteNum}\t{lpEvent[j].nSpriteFrames}\t" +
                                        $"{lpEvent[j].wDirection}\t{lpEvent[j].wCurrentFrameNum}\t{lpEvent[j].nScriptIdleFrame}\t{lpEvent[j].wSpritePtrOffset}\t{lpEvent[j].nSpriteFramesAuto}\t" +
                                        $"{lpEvent[j].wScriptIdleFrameCountAuto}{DocNewLine}";
                                }

                                //
                                // 拼接场景表头，行数自增
                                //
                                strContent += $"{DocNewLine}$SceneID\t地图编号\t进场触发器\t传送触发器\t场景备注{DocNewLine}";

                                //
                                // 拼接当前场景数据，行数自增
                                //
                                strContent += $"{i + 1}\t{lpScene[i].wMapNum}\t{lpScene[i].wScriptOnEnter}\t{lpScene[i].wScriptOnTeleport}\t{PAL_GetSceneName(i)}{DocNewLine}";

                                //
                                // 字符流写入文件
                                //
                                UTIL_BinaryWrite(strObj, $"{strObj}_{i + 1}", strContent);
                            }
                        }
                        break;

                    case "Object":
                        {
                            ushort wFlags;

                            string strPlayer = $"$ObjectID\t队员名称\t无效\t无效\t愤怒脚本\t虚弱脚本\t无效{PAL_FormatWINVersion("\t无效")}\t无效{DocNewLine}";
                            string strItem = $"$ObjectID\t道具名称\t图像编号\t售价\t使用脚本\t装备脚本\t投掷脚本{PAL_FormatWINVersion("\t道具描述脚本")}\t**属性掩码**\t" +
                                $"可使用\t可装备\t可投掷\t使用后会减少\t全体作用\t可典当\t李逍遥可装备\t赵灵儿可装备\t林月如可装备\t巫后可装备\t阿奴可装备\t盖罗娇可装备{DocNewLine}";
                            string strMagic = $"$ObjectID\t仙术名称\t设定编号\t无效\t施法成功脚本\t施法脚本{PAL_FormatWINVersion("\t仙术描述脚本")}\t无效\t**原属性掩码**\t" +
                                $"非战斗可用\t战斗可用\t作用于敌方\t攻击敌方全体{DocNewLine}";
                            string strEnemy = $"$ObjectID\t敌人名称\t设定编号\t巫抗\t战前脚本\t胜利脚本\t战斗脚本{PAL_FormatWINVersion("\t无效")}\t无效{DocNewLine}";
                            string strPoison = $"$ObjectID\t毒性名称\t毒性\t颜色\t我方中毒脚本\t无效\t敌方中毒脚本{PAL_FormatWINVersion("\t无效")}\t无效{DocNewLine}";
                            strContent = $"$ObjectID\t对象名称\t无效\t无效\t无效\t无效\t无效{PAL_FormatWINVersion("\t无效")}\t无效{DocNewLine}";

                            fixed (string[]* strpWords = &gpGlobals.g_TextList.saWords)
                            {
                                if (!fIsWIN95)
                                {
                                    OBJECT_OTHER_DOS* O;
                                    OBJECT_PLAYER_DOS* P;
                                    OBJECT_ITEM_DOS* I;
                                    OBJECT_MAGIC_DOS* M;
                                    OBJECT_ENEMY_DOS* E;
                                    OBJECT_POISON_DOS* PO;

                                    //
                                    // 表体
                                    //
                                    for (i = 0; i < lpGameData->nObject; i++)
                                    {
                                        //
                                        // 指针地址非 fixed 固定的，需实时获取
                                        //
                                        O = lpGameData->GetObject_DOS();

                                        //
                                        // 获取当前对象名称
                                        //
                                        string strThisWord = $">{PAL_GetWord(i)}<";

                                        if (i >= 36 && i <= 41)
                                        {
                                            //
                                            // 队员 ObjectID 36～41
                                            //
                                            P = &((OBJECT_PLAYER_DOS*)O)[i];

                                            strPlayer += $"{i}\t{strThisWord}\t{(*P).wReserved0}\t{(*P).wReserved1}\t{(*P).wScriptOnFriendDeath}\t{(*P).wScriptOnDying}\t{(*P).wReserved4}\t{(*P).wReserved5}{DocNewLine}";
                                        }
                                        else if (i >= 61 && i <= 294)
                                        {
                                            //
                                            // 道具 ObjectID 61～294
                                            //
                                            I = (OBJECT_ITEM_DOS*)&(O)[i];

                                            wFlags = (*I).wFlags;

                                            strItem += $"{i}\t{strThisWord}\t{(*I).wBitmap}\t{(*I).wPrice}\t{(*I).wScriptOnUse}\t{(*I).wScriptOnEquip}\t{(*I).wScriptOnThrow}\t{(*I).wFlags}\t" +
                                                $"{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kUsable)}\t{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kEquipable)}\t" +
                                                $"{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kThrowable)}\t{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kConsuming)}\t" +
                                                $"{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kApplyToAll)}\t{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kSellable)}";

                                            for (j = 0; j <= MAX_PLAYABLE_PLAYER_ROLES; j++)
                                            {
                                                strItem += $"\t{PAL_GetItemFlags(wFlags, (ushort)((int)ITEMFLAG.kEquipableByPlayerRole_First << (int)j))}";
                                            }

                                            strItem += DocNewLine;
                                        }
                                        else if (i >= 295 && i <= 397)
                                        {
                                            //
                                            // 仙术 ObjectID 295～397
                                            //
                                            M = (OBJECT_MAGIC_DOS*)&(O)[i];

                                            wFlags = (*M).wFlags;

                                            strMagic += $"{i}\t{strThisWord}\t{(*M).wMagicNumber}\t{(*M).wReserved1}\t{(*M).wScriptOnSuccess}\t{(*M).wScriptOnUse}\t{(*M).wReserved2}\t{(*M).wFlags}\t" +
                                                $"{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kUsableOutsideBattle)}\t{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kUsableInBattle)}\t" +
                                                $"{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kUsableToEnemy)}\t{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kApplyToAll)}{DocNewLine}";
                                        }
                                        else if (i >= 398 && i <= 550)
                                        {
                                            //
                                            // 敌人 ObjectID 398～550
                                            //
                                            E = (OBJECT_ENEMY_DOS*)&(O)[i];

                                            strEnemy += $"{i}\t{strThisWord}\t{(*E).wEnemyID}\t{(*E).wResistanceToSorcery}\t{(*E).wScriptOnTurnStart}\t{(*E).wScriptOnBattleEnd}\t{(*E).wScriptOnReady}\t{(*E).wReserved6}{DocNewLine}";
                                        }
                                        else if (i >= 551 && i <= 564)
                                        {
                                            //
                                            // 毒性 ObjectID 551～564
                                            //
                                            PO = (OBJECT_POISON_DOS*)&(O)[i];

                                            strPoison += $"{i}\t{strThisWord}\t{(*PO).wPoisonLevel}\t{(*PO).wColor}\t{(*PO).wPlayerScript}\t{(*PO).wReserved}\t{(*PO).wEnemyScript}\t{(*PO).wReserved6}{DocNewLine}";
                                        }
                                        else
                                        {
                                            //
                                            // 系统 ObjectID Other
                                            //
                                            strContent += $"{i}\t{strThisWord}\t{(*O).wReserved0}\t{(*O).wReserved1}\t{(*O).wReserved2}\t{(*O).wReserved3}\t{(*O).wReserved4}\t{(*O).wReserved5}{DocNewLine}";
                                        }
                                    }

                                    //
                                    // 字符流写入文件
                                    //
                                    UTIL_BinaryWrite(strObj, "Other", strContent);
                                    UTIL_BinaryWrite(strObj, "Player", strPlayer);
                                    UTIL_BinaryWrite(strObj, "Item", strItem);
                                    UTIL_BinaryWrite(strObj, "Magic", strMagic);
                                    UTIL_BinaryWrite(strObj, "Enemy", strEnemy);
                                    UTIL_BinaryWrite(strObj, "Poison", strPoison);
                                }
                                else
                                {
                                    OBJECT_OTHER* O;
                                    OBJECT_PLAYER* P;
                                    OBJECT_ITEM* I;
                                    OBJECT_MAGIC* M;
                                    OBJECT_ENEMY* E;
                                    OBJECT_POISON* PO;

                                    for (i = 0; i < lpGameData->nObject; i++)
                                    {
                                        //
                                        // 指针地址非 fixed 固定的，需实时获取
                                        //
                                        O = lpGameData->GetObject();

                                        //
                                        // 获取当前对象名称
                                        //
                                        string strThisWord = $">{PAL_GetWord(i)}<";

                                        if (i >= 36 && i <= 41)
                                        {
                                            //
                                            // 队员 ObjectID 36～41
                                            //
                                            P = &((OBJECT_PLAYER*)O)[i];

                                            strPlayer += $"{i}\t{strThisWord}\t{(*P).wReserved0}\t{(*P).wReserved1}\t{(*P).wScriptOnFriendDeath}\t{(*P).wScriptOnDying}\t{(*P).wReserved4}\t" +
                                                $"{(*P).wReserved_WIN}\t{(*P).wReserved5}{DocNewLine}";
                                        }
                                        else if (i >= 61 && i <= 294)
                                        {
                                            //
                                            // 道具 ObjectID 61～294
                                            //
                                            I = (OBJECT_ITEM*)&(O)[i];

                                            wFlags = (*I).wFlags;

                                            strItem += $"{i}\t{strThisWord}\t" +
                                                $"{(*I).wBitmap}\t{(*I).wPrice}\t{(*I).wScriptOnUse}\t{(*I).wScriptOnEquip}\t{(*I).wScriptOnThrow}\t{(*I).wScriptDesc_WIN}\t{(*I).wFlags}\t" +
                                                $"{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kUsable)}\t{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kEquipable)}\t" +
                                                $"{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kThrowable)}\t{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kConsuming)}\t" +
                                                $"{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kApplyToAll)}\t{PAL_GetItemFlags(wFlags, (ushort)ITEMFLAG.kSellable)}";

                                            for (j = 0; j <= MAX_PLAYABLE_PLAYER_ROLES; j++)
                                            {
                                                strItem += $"\t{PAL_GetItemFlags(wFlags, (ushort)((int)ITEMFLAG.kEquipableByPlayerRole_First << (int)j))}";
                                            }

                                            strItem += DocNewLine;
                                        }
                                        else if (i >= 295 && i <= 397)
                                        {
                                            //
                                            // 仙术 ObjectID 295～397
                                            //
                                            M = (OBJECT_MAGIC*)&(O)[i];

                                            wFlags = (*M).wFlags;

                                            strMagic += $"{i}\t{strThisWord}\t" +
                                                $"{(*M).wMagicNumber}\t{(*M).wReserved1}\t{(*M).wScriptOnSuccess}\t{(*M).wScriptOnUse}\t{(*M).wScriptDesc_WIN}\t{(*M).wReserved2}\t{(*M).wFlags}\t" +
                                                $"{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kUsableOutsideBattle)}\t{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kUsableInBattle)}\t" +
                                                $"{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kUsableToEnemy)}\t{PAL_GetMagicFlags(wFlags, (ushort)MAGICFLAG.kApplyToAll)}{DocNewLine}";
                                        }
                                        else if (i >= 398 && i <= 550)
                                        {
                                            //
                                            // 敌人 ObjectID 398～550
                                            //
                                            E = (OBJECT_ENEMY*)&(O)[i];

                                            strEnemy += $"{i}\t{strThisWord}\t{(*E).wEnemyID}\t{(*E).wResistanceToSorcery}\t{(*E).wScriptOnTurnStart}\t{(*E).wScriptOnBattleEnd}\t" +
                                                $"{(*E).wScriptOnReady}\t{(*E).wReserved_WIN}\t{(*E).wReserved6}{DocNewLine}";
                                        }
                                        else if (i >= 551 && i <= 564)
                                        {
                                            //
                                            // 毒性 ObjectID 551～564
                                            //
                                            PO = (OBJECT_POISON*)&(O)[i];

                                            strPoison += $"{i}\t{strThisWord}\t{(*PO).wPoisonLevel}\t{(*PO).wColor}\t{(*PO).wPlayerScript}\t{(*PO).wReserved}\t{(*PO).wEnemyScript}\t" +
                                                $"{(*PO).wReserved_WIN}\t{(*PO).wReserved6}{DocNewLine}";
                                        }
                                        else
                                        {
                                            //
                                            // 系统 ObjectID Other
                                            //
                                            strContent += $"{i}\t{strThisWord}\t" +
                                                $"{(*O).wReserved0}\t{(*O).wReserved1}\t{(*O).wReserved2}\t{(*O).wReserved3}\t{(*O).wReserved4}\t{(*O).wReserved_WIN}\t{(*O).wReserved5}{DocNewLine}";
                                        }
                                    }

                                    //
                                    // 字符流写入文件
                                    //
                                    UTIL_BinaryWrite(strObj, "Other", strContent);
                                    UTIL_BinaryWrite(strObj, "Player", strPlayer);
                                    UTIL_BinaryWrite(strObj, "Item", strItem);
                                    UTIL_BinaryWrite(strObj, "Magic", strMagic);
                                    UTIL_BinaryWrite(strObj, "Enemy", strEnemy);
                                    UTIL_BinaryWrite(strObj, "Poison", strPoison);
                                }
                            }
                        }
                        break;

                    case "ScriptEntry":
                        {
                            SCRIPTENTRY* lpScriptEntry;
                            string[] strarrThisEntryMsg;

                            //
                            // 表头
                            //
                            //strContent += $"$ScriptEntryID\t脚本标题\t参数一备注\t参数一\t参数二备注\t参数二\t参数三备注\t参数三\t脚本注释{DocNewLine}";
                            strContent += $"$ScriptEntryID\t指令码\t参数一\t参数二\t参数三\t脚本注释{DocNewLine}";

                            //
                            // 表体
                            //
                            for (i = 0; i <= lpGameData->nScriptEntry; i++)
                            {
                                lpScriptEntry = lpGameData->GetScriptEntry();

                                strarrThisEntryMsg = PAL_GetScriptEntryName(lpScriptEntry[i].wOperation);

                                //strContent += $"{PAL_DecToHex(i)}\t{strarrThisEntryMsg[1]}\t#{strarrThisEntryMsg[2]}\t{lpScriptEntry[i].rgwOperand1}\t" +
                                //$"#{strarrThisEntryMsg[3]}\t{lpScriptEntry[i].rgwOperand2}\t#{strarrThisEntryMsg[4]}\t{lpScriptEntry[i].rgwOperand3}\t#{strarrThisEntryMsg[5]}{DocNewLine}";
                                strContent += $"{PAL_DecToHex(i)}\t{PAL_DecToHex(lpScriptEntry[i].wOperation)}\t{PAL_DecToHex(lpScriptEntry[i].rgwOperand1)}\t" +
                                    $"{PAL_DecToHex(lpScriptEntry[i].rgwOperand2)}\t{PAL_DecToHex(lpScriptEntry[i].rgwOperand3)}\t{strarrThisEntryMsg[5]}{DocNewLine}";
                            }
                        }
                        break;

                    case "Store":
                        {
                            STORE* lpStore;

                            //
                            // 表头
                            //
                            strContent = $"$StoreID\t货物一\t货物一备注\t货物二\t货物二备注\t货物三\t货物三备注\t货物四\t货物四备注\t" +
                                $"货物五\t货物五备注\t货物六\t货物六备注\t货物七\t货物七备注\t货物八\t货物八备注\t货物九\t货物九备注{DocNewLine}";

                            //
                            // 表体
                            //
                            for (i = 0; i < lpGameData->nStore; i++)
                            {
                                //
                                // 指针地址非 fixed 固定的，需实时获取
                                //
                                lpStore = lpGameData->GetStore();

                                //
                                // 货物数据
                                //
                                strContent += $"{i}\t{lpStore[i].lprgItems0}\t{PAL_GetWord(lpStore[i].lprgItems0)}\t{lpStore[i].lprgItems1}\t{PAL_GetWord(lpStore[i].lprgItems1)}\t" +
                                    $"{lpStore[i].lprgItems2}\t{PAL_GetWord(lpStore[i].lprgItems2)}\t{lpStore[i].lprgItems3}\t{PAL_GetWord(lpStore[i].lprgItems3)}\t" +
                                    $"{lpStore[i].lprgItems4}\t{PAL_GetWord(lpStore[i].lprgItems4)}\t{lpStore[i].lprgItems5}\t{PAL_GetWord(lpStore[i].lprgItems5)}\t" +
                                    $"{lpStore[i].lprgItems6}\t{PAL_GetWord(lpStore[i].lprgItems6)}\t{lpStore[i].lprgItems7}\t{PAL_GetWord(lpStore[i].lprgItems7)}\t" +
                                    $"{lpStore[i].lprgItems8}\t{PAL_GetWord(lpStore[i].lprgItems8)}{DocNewLine}";
                            }
                        }
                        break;

                    case "Enemy":
                        {
                            ENEMY* lpEnemy;

                            //
                            // 表头
                            //
                            strContent = "$EnemyID\t蠕动帧\t施法帧\t普攻帧\t蠕动速度\t每帧延迟\tY偏移\t普攻音效\t行动音效\t施法音效\t死亡音效\t开战音效\t体力\t缴获经验\t缴获金钱\t修行\t" +
                                $"默认仙术\t施法概率\t$攻击附带\t附带概率\t$偷窃可得\t可偷数量\t武术\t灵力\t防御\t身法\t吉运\t毒抗\t风抗\t雷抗\t水抗\t火抗\t土抗\t物抗\t行动次数\t灵葫值{DocNewLine}";

                            //
                            // 表体
                            //
                            for (i = 0; i < lpGameData->nEnemy; i++)
                            {
                                //
                                // 指针地址非 fixed 固定的，需实时获取
                                //
                                lpEnemy = lpGameData->GetEnemy();

                                strContent += $"{i}\t{lpEnemy[i].wIdleFrames}\t{lpEnemy[i].wMagicFrames}\t{lpEnemy[i].wAttackFrames}\t{lpEnemy[i].wIdleAnimSpeed}\t{lpEnemy[i].wActWaitFrames}\t" +
                                    $"{lpEnemy[i].wYPosOffset}\t{lpEnemy[i].wAttackSound}\t{lpEnemy[i].wActionSound}\t{lpEnemy[i].wMagicSound}\t{lpEnemy[i].wDeathSound}\t{lpEnemy[i].wCallSound}\t" +
                                    $"{lpEnemy[i].wHealth}\t{lpEnemy[i].wExp}\t{lpEnemy[i].wCash}\t{lpEnemy[i].wLevel}\t{lpEnemy[i].wMagic}\t{lpEnemy[i].wMagicRate}\t{lpEnemy[i].wAttackEquivItem}\t" +
                                    $"{lpEnemy[i].wAttackEquivItemRate}\t{lpEnemy[i].wStealItem}\t{lpEnemy[i].nStealItem}\t{(short)lpEnemy[i].wAttackStrength}\t{(short)lpEnemy[i].wMagicStrength}\t" +
                                    $"{(short)lpEnemy[i].wDefense}\t{(short)lpEnemy[i].wDexterity}\t{(short)lpEnemy[i].wFleeRate}\t{(short)lpEnemy[i].wPoisonResistance}\t" +
                                    $"{(short)lpEnemy[i].wElemResistance_Wind}\t{(short)lpEnemy[i].wElemResistance_Thunder}\t{(short)lpEnemy[i].wElemResistance_Water}\t" +
                                    $"{(short)lpEnemy[i].wElemResistance_Fire}\t{(short)lpEnemy[i].wElemResistance_Soil}\t{lpEnemy[i].wPhysicalResistance}\t{lpEnemy[i].wDualMove}\t" +
                                    $"{lpEnemy[i].wCollectValue}{DocNewLine}";
                            }
                        }
                        break;

                    case "EnemyTeam":
                        {
                            ENEMYTEAM* lpEnemyTeam;

                            //
                            // 表头
                            //
                            strContent = $"$EnemyTeamID\t敌方单位一\t敌方单位一名称\t敌方单位二\t敌方单位二名称\t敌方单位三\t敌方单位三名称\t敌方单位四\t敌方单位四名称\t敌方单位五\t敌方单位五名称{DocNewLine}";

                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpGameData->nEnemyTeam; i++)
                            {
                                //
                                // 指针地址非 fixed 固定的，需实时获取
                                //
                                lpEnemyTeam = lpGameData->GetEnemyTeam();

                                strContent += $"{i}\t{lpEnemyTeam[i].rgwEnemy_0}\t{PAL_GetWord(lpEnemyTeam[i].rgwEnemy_0)}\t{lpEnemyTeam[i].rgwEnemy_1}\t{PAL_GetWord(lpEnemyTeam[i].rgwEnemy_1)}\t" +
                                    $"{lpEnemyTeam[i].rgwEnemy_2}\t{PAL_GetWord(lpEnemyTeam[i].rgwEnemy_2)}\t{lpEnemyTeam[i].rgwEnemy_3}\t{PAL_GetWord(lpEnemyTeam[i].rgwEnemy_3)}\t" +
                                    $"{lpEnemyTeam[i].rgwEnemy_4}\t{PAL_GetWord(lpEnemyTeam[i].rgwEnemy_4)}{DocNewLine}";
                            }
                        }
                        break;

                    case "PlayerRoles":
                        {
                            string[] strPlayerRolesTH = {
                                "肖像画",    "战斗像",   "行走像",   "名字",     "全攻",
                                "无效数据0", "修行",     "体力上限", "真气上限", "体力",
                                "真气",      "头戴",     "披挂",     "身穿",     "手持",
                                "脚穿",      "配戴",     "武术",     "灵力",     "防御",
                                "身法",      "吉运",     "毒抗",     "风抗",     "雷抗",
                                "水抗",      "火抗",     "土抗",     "巫抗",     "物抗",
                                "绝招加成",  "虚弱受援", "法术1",    "法术2",    "法术3",
                                "法术4",     "法术5",    "法术6",    "法术7",    "法术8",
                                "法术9",     "法术10",   "法术11",   "法术12",   "法术13",
                                "法术14",    "法术15",   "法术16",   "法术17",   "法术18",
                                "法术19",    "法术20",   "法术21",   "法术22",   "法术23",
                                "法术24",    "法术25",   "法术26",   "法术27",   "法术28",
                                "法术29",    "法术30",   "法术31",   "法术32",   "行走帧号",
                                "合体法术",  "**精力上限**", "**精力**",     "死亡音效", "普攻音效",
                                "武器音效",  "倍攻音效", "施法音效", "格挡音效", "被击音效"
                            };

                            PLAYERROLES* lpPR = &lpGameData->rgPlayerRoles;

                            ushort* lpLiXiaoYao;
                            ushort* lpZhaoLingEr;
                            ushort* lpLiuYueRu;
                            ushort* lpWuHou;
                            ushort* lpANu;
                            ushort* lpGaiLuoJiao;

                            //
                            // 拼接表头
                            //
                            strContent = "$PlayerData";

                            //
                            // 拼接表头
                            //
                            for (i = 0; i < MAX_PLAYER_ROLES; i++)
                            {
                                strContent += $"\t{PAL_GetPlayerName((int)i)}";
                            }

                            //
                            // 拼接表头后需要换行......
                            //
                            strContent += DocNewLine;

                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpPR->binLiXiaoYao.Length; i++)
                            {
                                //
                                // 获取队员数据
                                //
                                lpLiXiaoYao = (ushort*)lpPR->GetPlayer(0);
                                lpZhaoLingEr = (ushort*)lpPR->GetPlayer(1);
                                lpLiuYueRu = (ushort*)lpPR->GetPlayer(2);
                                lpWuHou = (ushort*)lpPR->GetPlayer(3);
                                lpANu = (ushort*)lpPR->GetPlayer(4);
                                lpGaiLuoJiao = (ushort*)lpPR->GetPlayer(5);

                                strContent += $"#{strPlayerRolesTH[i]}\t{lpLiXiaoYao[i]}\t{lpZhaoLingEr[i]}\t{lpLiuYueRu[i]}\t{lpWuHou[i]}\t{lpANu[i]}\t{lpGaiLuoJiao[i]}{DocNewLine}";
                            }
                        }
                        break;

                    case "Magic":
                        {
                            MAGIC* lpMagic;

                            //
                            // 拼接表头
                            //
                            strContent = $"$MagicID\t形象号\t作用域\tX偏移\tY偏移\t召唤神形\t特效速度\t形象残留\t音效延迟\t耗时\t场景震动\t场景波动\t消耗精力\t消耗真气\t伤害\t系属\t音效{DocNewLine}";


                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpGameData->nMagic; i++)
                            {
                                lpMagic = lpGameData->GetMagic();

                                strContent += $"{i}\t{lpMagic[i].wEffect}\t{lpMagic[i].wType}\t{lpMagic[i].sXOffset}\t{lpMagic[i].sYOffset}\t{lpMagic[i].wSummonEffect}\t" +
                                    $"{lpMagic[i].wSpeed}\t{lpMagic[i].wKeepEffect}\t{lpMagic[i].wFireDelay}\t{lpMagic[i].sEffectTimes}\t{lpMagic[i].wShake}\t" +
                                    $"{lpMagic[i].wWave}\t{lpMagic[i].wCostSP}\t{lpMagic[i].wCostMP}\t{lpMagic[i].wBaseDamage}\t{lpMagic[i].wElemental}\t" +
                                    $"{lpMagic[i].wSound}{DocNewLine}";
                            }
                        }
                        break;

                    case "BattleField":
                        {
                            BATTLEFIELD* lpBattleField;

                            //
                            // 拼接表头
                            //
                            strContent = $"$BattleFieldID\t画面波动\t风系\t雷系\t水系\t火系\t土系\t战场备注{DocNewLine}";


                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpGameData->nBattleField; i++)
                            {
                                lpBattleField = lpGameData->GetBattleField();

                                strContent += $"{i}\t{lpBattleField[i].wScreenWave}\t{lpBattleField[i].rgsMagicEffect_Wind}\t{lpBattleField[i].rgsMagicEffect_Thunder}\t" +
                                    $"{lpBattleField[i].rgsMagicEffect_Water}\t{lpBattleField[i].rgsMagicEffect_Fire}\t{lpBattleField[i].rgsMagicEffect_Soil}\t{PAL_GetFightBackPicName(i)}{DocNewLine}";
                            }
                        }
                        break;

                    case "LevelUpMagic":
                        {
                            LEVELUPMAGIC_ALL* lpPR = &lpGameData->rgLevelUpMagic;

                            LEVELUPMAGIC* lpLiXiaoYao;
                            LEVELUPMAGIC* lpZhaoLingEr;
                            LEVELUPMAGIC* lpLiuYueRu;
                            LEVELUPMAGIC* lpWuHou;
                            LEVELUPMAGIC* lpANu;

                            //
                            // 拼接表头
                            //
                            strContent = "$LevelUpMagic";

                            //
                            // 拼接表头
                            //
                            for (i = 0; i < MAX_PLAYABLE_PLAYER_ROLES; i++)
                            {
                                strContent += $"\t{PAL_GetPlayerName((int)i)}";
                            }

                            //
                            // 拼接表头后需要换行......
                            //
                            strContent += DocNewLine;

                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpPR->binLiXiaoYao.Length; i++)
                            {

                                lpLiXiaoYao = lpPR->GetLevelUpMagic(0);
                                lpZhaoLingEr = lpPR->GetLevelUpMagic(1);
                                lpLiuYueRu = lpPR->GetLevelUpMagic(2);
                                lpWuHou = lpPR->GetLevelUpMagic(3);
                                lpANu = lpPR->GetLevelUpMagic(4);

                                strContent += $"#{i}_修行\t{lpLiXiaoYao[i].wLevel}\t{lpZhaoLingEr[i].wLevel}\t{lpLiuYueRu[i].wLevel}\t{lpWuHou[i].wLevel}\t{lpANu[i].wLevel}{DocNewLine}";
                                strContent += $"#{i}_仙术\t{lpLiXiaoYao[i].wMagic}\t{lpZhaoLingEr[i].wMagic}\t{lpLiuYueRu[i].wMagic}\t{lpWuHou[i].wMagic}\t{lpANu[i].wMagic}{DocNewLine}";
                            }
                        }
                        break;

                    case "FightEffect":
                        {
                            FIGHTEFFECT* lpFightEffect;

                            //
                            // 拼接表头
                            //
                            strContent = $"$FightEffect\t普攻帧序列编号\t施法帧序列编号{DocNewLine}";

                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpGameData->nFightEffect; i++)
                            {
                                lpFightEffect = lpGameData->GetFightEffect();

                                strContent += $"#{PAL_GetFightEffectName(i)}\t{lpFightEffect[i].wAttackEffectID}\t{lpFightEffect[i].wMagicEffectID}{DocNewLine}";
                            }
                        }
                        break;

                    case "EnemyPos":
                        {
                            ENEMYPOS* lpPR = &lpGameData->rgEnemyPos;

                            PALPOS* lpPos_Five;
                            PALPOS* lpPos_Four;
                            PALPOS* lpPos_Three;
                            PALPOS* lpPos_Two;
                            PALPOS* lpPos_One;

                            //
                            // 拼接表头
                            //
                            strContent = $"$EnemyPos\t敌数1_X\t敌数1_Y\t敌数2_X\t敌数2_Y\t敌数3_X\t敌数3_Y\t敌数4_X\t敌数4_Y\t敌数5_X\t敌数5_Y{DocNewLine}";

                            //
                            // 拼接表体
                            //
                            i = 1;
                            strContent += $"#目标编号{i++}";
                            string strPosTwo = $"#目标编号{i++}";
                            string strPosThree = $"#目标编号{i++}";
                            string strPosFour = $"#目标编号{i++}";
                            string strPosFive = $"#目标编号{i++}";

                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpGameData->nEnemyPos; i++)
                            {
                                lpPos_One = lpPR->GetEnemyPos(1);
                                lpPos_Two = lpPR->GetEnemyPos(2);
                                lpPos_Three = lpPR->GetEnemyPos(3);
                                lpPos_Four = lpPR->GetEnemyPos(4);
                                lpPos_Five = lpPR->GetEnemyPos(5);

                                strContent += $"\t{lpPos_One[i].x}\t{lpPos_One[i].y}";
                                strPosTwo += $"\t{lpPos_Two[i].x}\t{lpPos_Two[i].y}";
                                strPosThree += $"\t{lpPos_Three[i].x}\t{lpPos_Three[i].y}";
                                strPosFour += $"\t{lpPos_Four[i].x}\t{lpPos_Four[i].y}";
                                strPosFive += $"\t{lpPos_Five[i].x}\t{lpPos_Five[i].y}";
                            }

                            strContent += DocNewLine;
                            strContent += strPosTwo + DocNewLine;
                            strContent += strPosThree + DocNewLine;
                            strContent += strPosFour + DocNewLine;
                            strContent += strPosFive + DocNewLine;
                        }
                        break;

                    case "LevelUpExp":
                        {
                            ushort[]* wpExp;

                            //
                            // 拼接表头
                            //
                            strContent = $"$LevelUpExp\t晋级所需经验{DocNewLine}";

                            //
                            // 拼接表体
                            //
                            for (i = 0; i < lpGameData->nLevelUpExp; i++)
                            {
                                wpExp = lpGameData->GetLevelUpExp();

                                strContent += $"#修行_{i}\t{(*wpExp)[i]}{DocNewLine}";
                            }
                        }
                        break;

                    default:
                        break;
                }

                //
                // 字符流写入文件
                //
                if (strObj != "Object" && strObj != "Scene") UTIL_BinaryWrite("Data", strObj, strContent);
            }
        }

        public static void
        PAL_FreeFile(
            FileStream fsFile
        )
        {
            if (fsFile != null) fsFile.Dispose();
        }

        public static void
        PAL_FreeGameBin(
            ref byte[] binGameBin
        )
        {
            if (binGameBin != null) Array.Clear(binGameBin);
        }

        public static void
        PAL_FreeGameBin(
            ref ushort[] binGameBin
        )
        {
            if (binGameBin != null) Array.Clear(binGameBin);
        }

        public static void
        PAL_FreeGameBin(
            ref uint[] binGameBin
        )
        {
            if (binGameBin != null) Array.Clear(binGameBin);
        }

        public static void
        PAL_FreeGameBin(
            ref string[] binGameBin
        )
        {
            if (binGameBin != null) Array.Clear(binGameBin);
        }

        public static void
        PAL_FreeGlobals()
        /*++
          Purpose:

            Release all resources occupied by the program.
            释放程序运行时占用的所有资源。

          Parameters:

            None.
            无。

          Return value:

            None.
            无。

        --*/
        {
            gpGlobals.Free();
        }

        public static void
        PAL_Shutdown()
        /*++
          Purpose:

            Free everything needed by the game.

          Parameters:

            exit_code -  The exit code return to OS.

          Return value:

            None.

        --*/
        {
            //
            // global needs be free in last
            // since subsystems may needs config content during destroy
            // which also cleared here
            // 全局数据最终需要释放
            // 由于子系统在销毁过程中可能需要配置内容，此处也已清除
            //
            PAL_FreeGlobals();

            PAL_Log("程序结束............按 Enter 退出");

            Console.ReadLine();
            Environment.Exit(0);
        }

        public static uint
        PAL_MKFGetChunkCount(
            FileStream fsFile
        )
        /*++
          Purpose:

            Get the number of chunks in an MKF archive.
            获取MKF存档中的块数。

          Parameters:

            [IN]  fp - pointer to an fopen'ed MKF file.
            指向已打开的MKF文件的指针。

          Return value:

            Integer value which indicates the number of chunks in the specified MKF file.
            指定的MKF文件中块数的整数值。

        --*/
        {
            int iNumChunk;

            int size = sizeof(int);
            byte[] baNumChunk = new byte[size];

            // 若文件为空指针则退出返回
            /*++
            if (fsFile == null)
            {
                return 0;
            }
            --*/
            PAL_Failed(fsFile == null, "Failed to obtain the number of blocks, Parameter[fsFile] The FileStream is null.", "PAL_MKFGetChunkCount");

            //
            // 定位文件指针到文件开头处
            //
            PAL_Seek(0, SeekOrigin.Begin, fsFile);

            //
            // 判断文件是否有问题
            // 取 MKF 子块数
            //
            if (PAL_Read(&baNumChunk, 1, 0, fsFile) != -1)
            {
                //
                // 与 C语言 不同的是，C# 已经帮你把正确的值拿出来了
                // 比如 0x12345678 ,C语言拿到的是 0x0x78563412 ,需要再转换一下
                // 使用 SDL_SwapLE32 函数转回 0x12345678，而 C# 则省去此操作
                // return (SDL_SwapLE32(iNumChunk) - 4) >> 2;
                //
                //
                // 字节数组转 INT
                //
                iNumChunk = PAL_ToInt(baNumChunk, 0);

                return (uint)((iNumChunk - 4) >> 2);
            }
            else
                return 0;
        }

        public static uint
        PAL_MKFGetChunkSize(
           uint uiChunkNum,
           FileStream fsFile
        )
        /*++
          Purpose:

            Get the size of a chunk in an MKF archive.

          Parameters:

            [IN]  uiChunkNum - the number of the chunk in the MKF archive.

            [IN]  fp - pointer to the fopen'ed MKF file.

          Return value:

            Integer value which indicates the size of the chunk.
            -1 if the chunk does not exist.

        --*/
        {
            uint uiOffset = 0;
            uint uiNextOffset = 0;

            int iSize = sizeof(uint);
            byte[] baOffset = new byte[iSize];
            byte[] baNextOffset = new byte[iSize];

            //
            // Get the total number of chunks.
            //
            uint uiChunkCount = PAL_MKFGetChunkCount(fsFile);
            /*++
            if (uiChunkNum >= uiChunkCount)
            {
                return -1;
            }
            --*/
            PAL_Failed(uiChunkNum >= uiChunkCount, "Failed to read chunk size, Parameter[uiChunkNum] Value out of range.", "PAL_MKFGetChunkSize");

            //
            // Get the offset of the specified chunk and the next chunk.
            //
            PAL_Seek(4 * uiChunkNum, SeekOrigin.Begin, fsFile);
            PAL_Fread(&baOffset, iSize, 0, fsFile);
            PAL_Fread(&baNextOffset, iSize, 0, fsFile);

            //
            // 与 C语言 不同的是，C# 已经帮你把正确的值拿出来了
            // 比如 0x12345678 ,C语言拿到的是 0x0x78563412 ,需要再转换一下
            // 使用 SDL_SwapLE32 函数转回 0x12345678，而 C# 则省去此操作
            // uiOffset = SDL_SwapLE32(uiOffset);
            // uiNextOffset = SDL_SwapLE32(uiNextOffset);

            uiOffset = PAL_ToUInt(baOffset);
            uiNextOffset = PAL_ToUInt(baNextOffset);

            //
            // Return the length of the chunk.
            //
            return uiNextOffset - uiOffset;
        }

        public static int
        PAL_MKFReadChunk(
            byte[]* lpBuffer,
            uint uiBufferSize,
            uint uiChunkNum,
            FileStream fsFile
        )
        /*++
          Purpose:

            Read a chunk from an MKF archive into lpBuffer.
            将MKF归档文件中的块读取到lpBuffer中。

          Parameters:

            [OUT] lpBuffer - pointer to the destination buffer.
            指向目标缓冲区的指针。

            [IN]  uiBufferSize - size of the destination buffer.
            目标缓冲区的大小。

            [IN]  uiChunkNum - the number of the chunk in the MKF archive to read.
            MKF存档文件中要读取的区块数。

            [IN]  fp - pointer to the fopen'ed MKF file.
            指向已打开的MKF文件的指针。

          Return value:

            Integer value which indicates the size of the chunk.
            -1 if there are error in parameters.
            -2 if buffer size is not enough.
            指示块大小的整数值。
            -1  若参数有错误。
            -2，若缓冲区大小不够。

        --*/
        {
            uint uiOffset;
            uint uiNextOffset;

            int iSize = sizeof(uint);
            byte[] baOffset = new byte[iSize];
            byte[] baNextOffset = new byte[iSize];

            uint uiChunkCount;
            uint uiChunkLen;

            /*++
            if (lpBuffer == null || fsFile == null || uiBufferSize == 0)
            {
                return -1;
            }
            --*/

            PAL_Failed(lpBuffer == null || fsFile == null || uiBufferSize == 0, "Failed to read chunk data, Parameter error.", "PAL_MKFReadChunk");

            //
            // Get the total number of chunks.
            // 获取块的总数。
            uiChunkCount = PAL_MKFGetChunkCount(fsFile);
            /*++
            if (uiChunkNum >= uiChunkCount)
            {
                return -1;
            }
            --*/
            PAL_Failed(uiChunkNum >= uiChunkCount, "Failed to read chunk data, Parameter[uiChunkNum] Value out of range.", "PAL_MKFReadChunk");

            //
            // Get the offset of the chunk.
            // 获取块的偏移
            //
            PAL_Seek(4 * uiChunkNum, SeekOrigin.Begin, fsFile);
            PAL_Fread(&baOffset, iSize, 0, fsFile);
            PAL_Fread(&baNextOffset, iSize, 0, fsFile);

            //
            // 与 C语言 不同的是，C# 已经帮你把正确的值拿出来了
            // 比如 0x12345678 ,C语言拿到的是 0x0x78563412 ,需要再转换一下
            // 使用 SDL_SwapLE32 函数转回 0x12345678，而 C# 则省去此操作
            // uiOffset = PAL_SwapLE32(baOffset);
            // uiNextOffset = PAL_SwapLE32(baNextOffset);

            uiOffset = PAL_ToUInt(baOffset);
            uiNextOffset = PAL_ToUInt(baNextOffset);

            //
            // Get the length of the chunk.
            // 获取块的长度
            //
            uiChunkLen = uiNextOffset - uiOffset;

            /*++
            if (uiChunkLen > uiBufferSize)
            {
                return -2;
            }
            --*/
            PAL_Failed(uiChunkLen > uiBufferSize, "Failed to read chunk data, Parameter[uiBufferSize] Memory cache size is too small.", "PAL_MKFReadChunk");

            if (uiChunkLen != 0)
            {
                PAL_Seek(uiOffset, SeekOrigin.Begin, fsFile);
                return PAL_Read(lpBuffer, (int)uiChunkLen, 0, fsFile);
            }

            PAL_Failed(uiChunkLen == 0, "Failed to read chunk data, Parameter[uiBufferSize] Failed to read block data length.", "PAL_MKFReadChunk");

            return -1;
        }

        public static void
        PAL_InitWord()
        /*
         * 目的：
         *      分割 Word 单辞档，分割每个对象的名称
         * 
         * 参数：
         *      无
         * 
         * 返回值：
         *      无
         * 
         */
        {
            //
            // 对象作用域（对象 fsWord, sw 的作用域，运行时一旦离开这个块，对象 fsWord, sw 就会被释放，等待被gc回收，防止内存泄露）
            //
            using (FileStream fsWord = UTIL_OpenRequiredFile(strResFilePath + strResFileName[11]))
            {
                //
                // 当前单辞字节数组
                //
                byte[] tmp = new byte[10];
                uint nWord = (uint)(fsWord.Length / tmp.Length);
                fixed (string[]* strpWords = &gpGlobals.g_TextList.saWords)
                {
                    *strpWords = new string[nWord];
                    //
                    // 注册Nuget包System.Text.Encoding.CodePages中的编码到.NET 6
                    //
                    UTIL_RegEncode();

                    //
                    // 拼接好每行数据
                    //
                    for (uint i = 0; i < nWord; i++)
                    {
                        //
                        // 读出所有内容放入字节数组中
                        //
                        PAL_Read(&tmp, tmp.Length, 0, fsWord);

                        //
                        // 根据资源字节数组转 GB2312 或 BIG5字符串
                        //
                        Encoding ecEncode = UTIL_GetEncode();

                        (*strpWords)[i] = ecEncode.GetString(tmp).TrimEnd();
                    }
                }
            }
        }

        public static void
        PAL_InitMessages()
        /*
         * 目的：
         *      解档 Messages 对话档，分割每组对话
         * 
         * 参数：
         *      无
         * 
         * 
         * 返回值：
         *      无
         * 
         */
        {
            uint wThisMsgIndex;
            uint wNextMsgIndex;

            //
            // 对象作用域（对象 fsWord, sw 的作用域，运行时一旦离开这个块，对象 fsWord, sw 就会被释放，等待被gc回收，防止内存泄露）
            //
            using (FileStream fsWord = UTIL_OpenRequiredFile(strResFilePath + strResFileName[10]))
            {
                fixed (GAMEDATA* lpGameData = &gpGlobals.g)
                {
                    //
                    // 当前单辞字节数组
                    //
                    uint nMsg = lpGameData->nMsgIndex - 1;
                    fixed (string[]* strpMsgs = &gpGlobals.g_TextList.saMsgs)
                    {
                        *strpMsgs = new string[nMsg];

                        //
                        // 注册Nuget包System.Text.Encoding.CodePages中的编码到.NET 6
                        //
                        UTIL_RegEncode();

                        //
                        // 拼接好每行数据
                        //
                        for (uint i = 0; i < nMsg; i++)
                        {
                            wThisMsgIndex = (*(uint[]*)lpGameData->lprgMsgIndex)[i];
                            wNextMsgIndex = (*(uint[]*)lpGameData->lprgMsgIndex)[i + 1];

                            byte[] tmp = new byte[wNextMsgIndex - wThisMsgIndex];

                            //
                            // 读出所有内容放入字节数组中
                            //
                            PAL_Read(&tmp, tmp.Length, 0, fsWord);

                            //
                            // 根据资源字节数组转 GB2312 或 BIG5字符串
                            //
                            Encoding ecEncode = UTIL_GetEncode();

                            (*strpMsgs)[i] = ecEncode.GetString(tmp);
                        }
                    }
                }
            }
        }

        public static string
        PAL_GetWord(
            uint uiWordID
        )
        {
            return (uiWordID <= 1) ? NULL : ((uiWordID == 0xFFFF) ? DISABLE : gpGlobals.g_TextList.saWords[uiWordID]);
        }

        public static string
        PAL_GetMessage(
            uint uiMessageID
        )
        {
            return gpGlobals.g_TextList.saWords[uiMessageID];
        }

        public static string
        PAL_GetSceneName(
            uint uiSceneID
        )
        {
            string strSceneName;

            if (uiSceneID >= SceneID.Length) strSceneName = SceneID[0, 1];
            else if (fIsWIN95) strSceneName = SceneID[uiSceneID, 1];
            else strSceneName = SceneID[uiSceneID + 1, 1];

            return strSceneName;
        }

        public static string[]
        PAL_GetScriptEntryName(
            uint iScriptID
        )
        {
            int nContent = 6;
            int iLen = ScriptMessage.Length / nContent;
            string[] strScriptEntryName = new string[iLen];

            if (iScriptID >= iLen) iScriptID = (uint)(iLen - 1);

            for (int i = 0; i < nContent; i++) strScriptEntryName[i] = ScriptMessage[iScriptID, i];

            return strScriptEntryName;
        }

        public static string
        PAL_GetSpriteName(
            uint uiSpriteID
        )
        {
            string strSpriteName;

            if (uiSpriteID >= SpriteID.Length) strSpriteName = SpriteID[0, 1];
            else strSpriteName = SpriteID[uiSpriteID, 1];

            return strSpriteName;
        }

        public static string
        PAL_GetFightBackPicName(
            uint uiFightBackPicID
        )
        {
            string strFightBackPicName;

            if (uiFightBackPicID >= FightBackPictureID.Length) strFightBackPicName = FightBackPictureID[0, FightBackPictureID.Length - 1];
            else strFightBackPicName = FightBackPictureID[uiFightBackPicID, !fIsWIN95 ? 1 : 2];

            return strFightBackPicName;
        }

        public static string
        PAL_GetFightEffectName(
            uint iFightEffectID
        )
        {
            string strFightEffectName;

            if (iFightEffectID >= FightEffectID.Length) strFightEffectName = FightEffectID[0, 1];
            else strFightEffectName = FightEffectID[iFightEffectID, 1];

            return strFightEffectName;
        }

        public static string
        PAL_GetPlayerName(
            int iPlayerID
        )
        {
            iPlayerID %= MAX_PLAYER_ROLES;

            fixed (GAMEDATA* lpGameData = &gpGlobals.g)
            {
                PLAYERROLES* lpPR = &lpGameData->rgPlayerRoles;

                ushort* lpPlayer = (ushort*)lpPR->GetPlayer(iPlayerID);

                return PAL_GetWord(lpPlayer[3]);
            }
        }

        public static int
        PAL_GetItemFlags(
            ushort wFlages,
            ushort iItemFlags
        )
        {
            return (wFlages & iItemFlags) != 0 ? 1 : 0;
        }

        public static int
        PAL_GetMagicFlags(
            ushort wFlages,
            ushort iItemFlags
        )
        {
            return (wFlages & iItemFlags) != 0 ? 1 : 0;
        }

        public static string
        PAL_FormatWINVersion(
            string format
        )
        {
            return !fIsWIN95 ? "" : format;
        }

        public static void
        PAL_Log(
            string format = "",
            bool fIsEndNewLine = TRUE
        )
        {
            if (fIsEndNewLine) Console.WriteLine(format);
            else Console.Write(format);
        }

        public static void
        PAL_Log(
            string format,
            params object?[]? arg
        )
        {
            Console.WriteLine(format, arg);
        }

        public static void
        PAL_Failed(
            bool fCondition,
            params object?[]? arg
        )
        {
            if (fCondition)
            {
                //
                // 程序崩溃提示
                //
                PAL_Log("\nFailed:\n\t{1}>>> {0}\n", arg);

                //
                // 结束程序
                //
                PAL_Shutdown();
            }
        }

        public static string
        PAL_DecToHex(
            long lValue,
            int fIsHaveformat = 1
        )
        {
            string strResult = "";

            if (fIsHaveformat > 1) strResult += "0x";

            if (fIsHaveformat > 0)
            {
                strResult += lValue.ToString("X4");
            }
            else if(fIsHaveformat == 0)
            {
                strResult += Convert.ToString(lValue, 16).ToUpper();
            }

            //if (fIsHaveformat > 1) strResult += "H";

            return strResult;
        }

        public static int
        PAL_ToInt(
            byte[] buf,
            int iStart = 0
        )
        {
            return BitConverter.ToInt32(buf, iStart);
        }

        public static uint
        PAL_ToUInt(
            byte[] buf,
            int iStart = 0
        )
        {
            return BitConverter.ToUInt32(buf, iStart);
        }
    }
}
