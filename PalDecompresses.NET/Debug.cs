using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static PalDecompresses.Global;
using static PalDecompresses.PalUtil;

namespace PalDecompresses
{
    public unsafe class
    Debug
    {
        public static void
        PAL_Debug(
            string strObj
        )
        {
            int i;

            fixed (GAMEDATA* lpGameData = &gpGlobals.g)
            {
                switch (strObj)
                {
                    case "EventObject":
                        {
                            EVENTOBJECT* lpEvent = lpGameData->lprgEventObject;

                            PAL_Log("$EventObjectID\t隐藏时间\t坐标:X\t坐标:Y\t图层\t互动触发器\t自动循环器\t碰撞状态\t互动方式\t形象编号\t每方向帧数\t面朝方向\t形象当前帧\t" +
                                        "互动计次器\t**可偷窃**\t已行步数\t自动循环计次器");

                            for (i = 0; i < lpGameData->nEventObject; i++)
                            {
                                PAL_Log($"{i}\t" +
                                        $"{lpEvent[i].sVanishTime}\t{lpEvent[i].x}\t{lpEvent[i].y}\t{lpEvent[i].sLayer}\t{lpEvent[i].wTriggerScript}\t" +
                                        $"{lpEvent[i].wAutoScript}\t{lpEvent[i].sState}\t{lpEvent[i].wTriggerMode}\t{lpEvent[i].wSpriteNum}\t{lpEvent[i].nSpriteFrames}\t" +
                                        $"{lpEvent[i].wDirection}\t{lpEvent[i].wCurrentFrameNum}\t{lpEvent[i].nScriptIdleFrame}\t{lpEvent[i].wSpritePtrOffset}\t{lpEvent[i].nSpriteFramesAuto}\t" +
                                        $"{lpEvent[i].wScriptIdleFrameCountAuto}{DocNewLine}");
                            }
                        }
                        break;

                    case "Scene":
                        {
                            SCENE* lpScene = lpGameData->lprgScene;

                            PAL_Log($"@SceneID\t地图编号\t进场触发器\t传送触发器");

                            for (i = 0; i < lpGameData->nScene; i++)
                            {
                                PAL_Log($"{i + 1}\t{lpScene[i].wMapNum}\t{lpScene[i].wScriptOnEnter}\t{lpScene[i].wScriptOnTeleport}{DocNewLine}");
                            }

                        }
                        break;

                    case "Object":
                        {
                            if (!fIsWIN95)
                            {
                                OBJECT_DOS obj = lpGameData->rgObject_DOS;

                                for (i = 0; i < lpGameData->nObject; i++)
                                {
                                    OBJECT_OTHER_DOS* O = &obj.other[i];
                                    PAL_Log("Other:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", (*O).wReserved0, (*O).wReserved1, (*O).wReserved2, (*O).wReserved3, (*O).wReserved4, (*O).wReserved5);

                                    OBJECT_PLAYER_DOS* P = &obj.player[i];
                                    PAL_Log("Player:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", (*P).wReserved0, (*P).wReserved1, (*P).wScriptOnFriendDeath, (*P).wScriptOnDying, (*P).wReserved4, (*P).wReserved5);

                                    OBJECT_ITEM_DOS* I = &obj.item[i];
                                    PAL_Log("Item:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", (*I).wBitmap, (*I).wPrice, (*I).wScriptOnUse, (*I).wScriptOnEquip, (*I).wScriptOnThrow, (*I).wFlags);

                                    OBJECT_MAGIC_DOS* M = &obj.magic[i];
                                    PAL_Log("Magic:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", (*M).wMagicNumber, (*M).wReserved1, (*M).wScriptOnSuccess, (*M).wScriptOnUse, (*M).wReserved2, (*M).wFlags);

                                    OBJECT_ENEMY_DOS* E = &obj.enemy[i];
                                    PAL_Log("Enemy:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", (*E).wEnemyID, (*E).wResistanceToSorcery, (*E).wScriptOnTurnStart, (*E).wScriptOnBattleEnd, (*E).wScriptOnReady, (*E).wReserved6);

                                    OBJECT_POISON_DOS* PO = &obj.poison[i];
                                    PAL_Log("Poison:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", (*PO).wPoisonLevel, (*PO).wColor, (*PO).wPlayerScript, (*PO).wReserved, (*PO).wEnemyScript, (*PO).wReserved6);

                                    PAL_Log();
                                }
                            }
                            else
                            {
                                OBJECT obj = lpGameData->rgObject;
                                OBJECT_OTHER** other = &obj.other;
                                OBJECT_PLAYER** player = &obj.player;
                                OBJECT_ITEM** item = &obj.item;
                                OBJECT_MAGIC** magic = &obj.magic;
                                OBJECT_ENEMY** enemy = &obj.enemy;
                                OBJECT_POISON** poison = &obj.poison;

                                for (i = 0; i < lpGameData->nObject; i++)
                                {
                                    OBJECT_OTHER* O = &(*other)[i];
                                    PAL_Log("Other:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}", (*O).wReserved0, (*O).wReserved1, (*O).wReserved2, (*O).wReserved3, (*O).wReserved4, (*O).wReserved5, (*O).wReserved5);

                                    OBJECT_PLAYER* P = &(*player)[i];
                                    PAL_Log("Player:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}", (*P).wReserved0, (*P).wReserved1, (*P).wScriptOnFriendDeath, (*P).wScriptOnDying, (*P).wReserved4, (*P).wReserved_WIN, (*P).wReserved5);

                                    OBJECT_ITEM* I = &(*item)[i];
                                    PAL_Log("Item:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}", (*I).wBitmap, (*I).wPrice, (*I).wScriptOnUse, (*I).wScriptOnEquip, (*I).wScriptOnThrow, (*I).wScriptDesc_WIN, (*I).wFlags);

                                    OBJECT_MAGIC* M = &(*magic)[i];
                                    PAL_Log("Magic:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}", (*M).wMagicNumber, (*M).wReserved1, (*M).wScriptOnSuccess, (*M).wScriptOnUse, (*M).wScriptDesc_WIN, (*M).wReserved2, (*M).wFlags);

                                    OBJECT_ENEMY* E = &(*enemy)[i];
                                    PAL_Log("Enemy:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}", (*E).wEnemyID, (*E).wResistanceToSorcery, (*E).wScriptOnTurnStart, (*E).wScriptOnBattleEnd, (*E).wScriptOnReady, (*E).wReserved_WIN, (*E).wReserved6);

                                    OBJECT_POISON* PO = &(*poison)[i];
                                    PAL_Log("Poison:\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}", (*PO).wPoisonLevel, (*PO).wColor, (*PO).wPlayerScript, (*PO).wReserved, (*PO).wEnemyScript, (*PO).wReserved_WIN, (*PO).wReserved6);

                                    PAL_Log();
                                }
                            }
                        }
                        break;

                    case "Store":
                        {
                            STORE* lpStore = lpGameData->lprgStore;

                            for (i = 0; i < lpGameData->nStore; i++)
                            {
                                PAL_Log("Store{0}:\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}\t\t{8}\t\t{9}", i,
                                    lpStore[i].lprgItems0, lpStore[i].lprgItems1, lpStore[i].lprgItems2, lpStore[i].lprgItems3, lpStore[i].lprgItems4,
                                    lpStore[i].lprgItems5, lpStore[i].lprgItems6, lpStore[i].lprgItems7, lpStore[i].lprgItems8);
                            }
                        }
                        break;

                    case "MsgIndex":
                        {
                            uint[]* uipMsg = (uint[]*)lpGameData->lprgMsgIndex;

                            for (i = 0; i < lpGameData->nMsgIndex; i++)
                            {
                                PAL_Log("MsgIndex{0}:\t{1}", i, (*uipMsg)[i]);
                            }
                        }
                        break;

                    case "Enemy":
                        {
                            ENEMY* lpEnemy = lpGameData->lprgEnemy;

                            for (i = 0; i < lpGameData->nEnemy; i++)
                            {
                                PAL_Log("Enemy{0}:\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}\t\t{8}\t\t{9}\t\t{10}" +
                                    "\t\t{11}\t\t{12}\t\t{13}\t\t{14}\t\t{15}\t\t{16}\t\t{17}\t\t{18}\t\t{19}\t\t{20}" +
                                    "\t\t{21}\t\t{22}\t\t{23}\t\t{24}\t\t{25}\t\t{26}\t\t{27}\t\t{28}\t\t{29}\t\t{30}" +
                                    "\t\t{31}\t\t{32}\t\t{33}\t\t{34}\t\t{35}", i,
                                    lpEnemy[i].wIdleFrames, lpEnemy[i].wMagicFrames, lpEnemy[i].wAttackFrames, lpEnemy[i].wIdleAnimSpeed, lpEnemy[i].wActWaitFrames,
                                    lpEnemy[i].wYPosOffset, lpEnemy[i].wAttackSound, lpEnemy[i].wActionSound, lpEnemy[i].wMagicSound, lpEnemy[i].wDeathSound,
                                    lpEnemy[i].wCallSound, lpEnemy[i].wHealth, lpEnemy[i].wExp, lpEnemy[i].wCash, lpEnemy[i].wLevel,
                                    lpEnemy[i].wMagic, lpEnemy[i].wMagicRate, lpEnemy[i].wAttackEquivItem, lpEnemy[i].wAttackEquivItemRate, lpEnemy[i].wStealItem,
                                    lpEnemy[i].nStealItem, lpEnemy[i].wAttackStrength, lpEnemy[i].wMagicStrength, lpEnemy[i].wDefense, lpEnemy[i].wDexterity,
                                    lpEnemy[i].wFleeRate, lpEnemy[i].wPoisonResistance, lpEnemy[i].wElemResistance_Wind, lpEnemy[i].wElemResistance_Thunder, lpEnemy[i].wElemResistance_Water,
                                    lpEnemy[i].wElemResistance_Fire, lpEnemy[i].wElemResistance_Soil, lpEnemy[i].wPhysicalResistance, lpEnemy[i].wDualMove, lpEnemy[i].wCollectValue);
                            }
                        }
                        break;

                    case "EnemyTeam":
                        {
                            ENEMYTEAM* lpEnemyTeam = lpGameData->lprgEnemyTeam;

                            for (i = 0; i < lpGameData->nEnemyTeam; i++)
                            {
                                PAL_Log("EnemyTeam{0}:\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", i,
                                    lpEnemyTeam[i].rgwEnemy_0, lpEnemyTeam[i].rgwEnemy_1, lpEnemyTeam[i].rgwEnemy_2, lpEnemyTeam[i].rgwEnemy_3, lpEnemyTeam[i].rgwEnemy_4);
                            }
                        }
                        break;

                    case "Player":
                        {
                            PLAYERROLES* lpPR = &lpGameData->rgPlayerRoles;

                            ushort* lpLiXiaoYao = (ushort*)lpPR->LiXiaoYao;
                            ushort* lpZhaoLingEr = (ushort*)lpPR->ZhaoLingEr;
                            ushort* lpLiuYueRu = (ushort*)lpPR->LinYueRu;
                            ushort* lpWuHou = (ushort*)lpPR->WuHou;
                            ushort* lpANu = (ushort*)lpPR->ANu;
                            ushort* lpGaiLuoJiao = (ushort*)lpPR->GaiLuoJiao;

                            PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", "李逍遥", "赵灵儿", "林月如", "巫后", "阿奴", "盖罗娇");
                            for (i = 0; i < lpPR->binLiXiaoYao.Length; i++)
                            {
                                PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", lpLiXiaoYao[i], lpZhaoLingEr[i], lpLiuYueRu[i], lpWuHou[i], lpANu[i], lpGaiLuoJiao[i]);
                            }
                        }
                        break;

                    case "Magic":
                        {
                            MAGIC* lpMagic = lpGameData->lprgMagic;

                            for (i = 0; i < lpGameData->nMagic; i++)
                            {
                                PAL_Log("magic{0}:\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}\t\t{8}\t\t{9}\t\t{10}" +
                                    "\t\t{11}\t\t{12}\t\t{13}\t\t{14}\t\t{15}\t\t{16}", i,
                                    lpMagic[i].wEffect, lpMagic[i].wType, lpMagic[i].sXOffset, lpMagic[i].sYOffset, lpMagic[i].wSummonEffect,
                                    lpMagic[i].wSpeed, lpMagic[i].wKeepEffect, lpMagic[i].wFireDelay, lpMagic[i].sEffectTimes, lpMagic[i].wShake,
                                    lpMagic[i].wWave, lpMagic[i].wCostSP, lpMagic[i].wCostMP, lpMagic[i].wBaseDamage, lpMagic[i].wElemental,
                                    lpMagic[i].wSound);
                            }
                        }
                        break;

                    case "BattleField":
                        {
                            BATTLEFIELD* lpBattleField = lpGameData->lprgBattleField;

                            for (i = 0; i < lpGameData->nBattleField; i++)
                            {
                                BATTLEFIELD battleField = lpBattleField[i];
                                PAL_Log("BattleField{0}:\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", i,
                                    battleField.wScreenWave, battleField.rgsMagicEffect_Wind, battleField.rgsMagicEffect_Thunder, battleField.rgsMagicEffect_Water, battleField.rgsMagicEffect_Fire,
                                    battleField.rgsMagicEffect_Soil);
                            }
                        }
                        break;

                    case "LevelUpMagic":
                        {
                            LEVELUPMAGIC_ALL* lpPR = &lpGameData->rgLevelUpMagic;

                            LEVELUPMAGIC* lpLiXiaoYao = lpPR->LiXiaoYao;
                            LEVELUPMAGIC* lpZhaoLingEr = lpPR->ZhaoLingEr;
                            LEVELUPMAGIC* lpLiuYueRu = lpPR->LinYueRu;
                            LEVELUPMAGIC* lpWuHou = lpPR->WuHou;
                            LEVELUPMAGIC* lpANu = lpPR->ANu;

                            PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", "李逍遥", "赵灵儿", "林月如", "巫后", "阿奴", "盖罗娇");
                            for (i = 0; i < lpPR->binLiXiaoYao.Length; i++)
                            {
                                PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}", lpLiXiaoYao[i].wLevel, lpZhaoLingEr[i].wLevel, lpLiuYueRu[i].wLevel, lpWuHou[i].wLevel, lpANu[i].wLevel);
                                PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}", lpLiXiaoYao[i].wMagic, lpZhaoLingEr[i].wMagic, lpLiuYueRu[i].wMagic, lpWuHou[i].wMagic, lpANu[i].wMagic);
                            }
                        }
                        break;

                    case "FightEffect":
                        {
                            FIGHTEFFECT* lpFightEffect = lpGameData->lprgFightEffect;

                            PAL_Log("FightEffect:\t{0}\t\t{1}", "普攻", "施法");
                            for (i = 0; i < lpGameData->nFightEffect; i++)
                            {
                                PAL_Log("FightEffect{0}:\t{1}\t\t{2}", i,
                                    lpFightEffect[i].wAttackEffectID, lpFightEffect[i].wMagicEffectID);
                            }
                        }
                        break;

                    case "EnemyPos":
                        {
                            ENEMYPOS* lpPR = &lpGameData->rgEnemyPos;

                            PALPOS* lpPos_Five = lpPR->pos_Five;
                            PALPOS* lpPos_Four = lpPR->pos_Four;
                            PALPOS* lpPos_Three = lpPR->pos_Three;
                            PALPOS* lpPos_Two = lpPR->pos_Two;
                            PALPOS* lpPos_One = lpPR->pos_One;

                            PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}\t\t{8}\t\t{9}",
                                "五敌X", "五敌Y", "四敌X", "四敌Y", "三敌X", "三敌Y", "二敌X", "二敌Y", "一敌X", "一敌Y");
                            for (i = 0; i < lpGameData->nEnemyPos; i++)
                            {
                                PAL_Log("{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}\t\t{8}\t\t{9}",
                                    lpPos_Five[i].x, lpPos_Five[i].y,
                                    lpPos_Four[i].x, lpPos_Four[i].y,
                                    lpPos_Three[i].x, lpPos_Three[i].y,
                                    lpPos_Two[i].x, lpPos_Two[i].y,
                                    lpPos_One[i].x, lpPos_One[i].y);
                            }
                        }
                        break;

                    case "LevelUpExp":
                        {
                            ushort[]* wpExp = (ushort[]*)lpGameData->lprgLevelUpExp;

                            for (i = 0; i < lpGameData->nLevelUpExp; i++)
                            {
                                PAL_Log("LevelUpExp{0}:\t{1}", i, (*wpExp)[i]);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
