using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.InteropServices;

using static System.Formats.Asn1.AsnWriter;

using static PalDecompresses.PalUtil;

namespace PalDecompresses
{
    public unsafe class Global
    {
        public const bool TRUE = true;
        public const bool FALSE = false;

        public static readonly char PathDSC = (Environment.OSVersion.Platform == PlatformID.Win32NT) ? '\\' : '/';
        public static readonly string DocNewLine = Environment.NewLine;

        public const string NULL = "**NULL**";
        public const string DISABLE = "**DISABLE**";

        public static bool fIsWIN95 = FALSE;

        public static bool fLogDebug = TRUE;

        // 
        // GB2312编码注册开关
        //
        public static bool fIsRegEncode = FALSE;

        //
        // 定义所需资源文件（在类里定义的一切类型都必须是静态的，其他地方才能调用）
        //
        public static string
            strResFilePath = fLogDebug ? $"D:{PathDSC}SWORD{PathDSC}Pal98{PathDSC}" : $".{PathDSC}",
            strSaveDocPath = $"{strResFilePath}PALMOD{PathDSC}",
            strSaveDocExt = ".TSV",
            strOriginDetails = "源详情",
            strNone = "无";

        public static string[] strResFileName = {
            "FBP.MKF", "MGO.MKF", "BALL.MKF", "DATA.MKF", "F.MKF",
            "FIRE.MKF", "RGM.MKF", "SSS.MKF", "ABC.MKF", "MAP.MKF",
            "M.MSG", "WORD.DAT"
        },
        strCSVDocName = {
            "Event", "Scene", "Object", "Script", "Store",
            "Enemy", "EnemyTeam", "PlayerRoles", "MagicData", "BattleField",
            "LevelUpMagic", "HeroActionEffect", "EnemyPos", "LevelUpExp"
        },
        strSssOutTextName = {

        },
        strOutMessageName = {
            "Messages.txt", "Word.txt"
        },
        //
        // 以 $ 开头的字符自动配对其对应的 Word 单辞
        //
        strStoreTH = {
            "商铺",
            "$货品1", "$货品2", "$货品3", "$货品4", "$货品5",
            "$货品6", "$货品7", "$货品8", "$货品9"
        },
        strEnemyTH = {
            "敌方",
            "蠕动帧", "施法帧", "普攻帧", "蠕动速度", "每帧延迟",
            "Y偏移", "普攻音效", "行动音效", "施法音效", "死亡音效",
            "开战音效", "体力", "缴获经验", "缴获金钱", "修行",
            "$默认仙术", "施法概率", "$攻击附带", "附带概率", "$偷窃可得",
            "可偷数量", "武术", "灵力", "防御", "身法",
            "吉运", "毒抗", "风抗", "雷抗", "水抗",
            "火抗", "土抗", "物抗", "行动次数", "灵葫值"
        },
        strEnemyTeamTH = {
            "队列",
            "$目标1", "$目标2", "$目标3", "$目标4", "$目标5"
        },
        strPlayerRolesTH = {
            "肖像画", "战斗像", "行走像", "名字", "全攻",
            "无效数据0", "修行", "体力上限", "真气上限", "体力",
            "真气", "头戴", "披挂", "身穿", "手持",
            "脚穿", "配戴", "武术", "灵力", "防御",
            "身法", "吉运", "毒抗", "风抗", "雷抗",
            "水抗", "火抗", "土抗", "巫抗", "物抗",
            "绝招加成", "虚弱受援", "法术1", "法术2",
            "法术3", "法术4", "法术5", "法术6", "法术7",
            "法术8", "法术9", "法术10", "法术11", "法术12",
            "法术13", "法术14", "法术15", "法术16", "法术17",
            "法术18", "法术19", "法术20", "法术21", "法术22",
            "法术23", "法术24", "法术25", "法术26", "法术27",
            "法术28", "法术29", "法术30", "法术31", "法术32",
            "行走帧号", "合体法术", "精力上限", "精力", "死亡音效",
            "普攻音效", "武器音效", "倍攻音效", "施法音效", "格挡音效",
            "被击音效 "
        },
        strMagicTH = {
            "仙术",
            "形象号", "作用域", "X偏移", "Y偏移", "召唤神形",
            "特效速度", "形象残留", "音效延迟", "耗时", "场景震动",
            "场景波动", "消耗精力", "消耗真气", "伤害", "系属",
            "音效"
        },
        strBattleFieldTH = {
            "环境",
            "画面波动", "风系", "雷系", "水系", "火系",
            "土系"
        },
        strLevelUpMagicTH = {
            "仙术组",
            "所需修行1", "$队员1", "所需修行2", "$队员2", "所需修行3",
            "$队员3", "所需修行4", "$队员4", "所需修行5", "$队员5"
        },
        strNoneTH = {
            "白昼",     "Houou",      "SDLPalLHYY",  "风羽Cyone丶",  "登明",   "EME",            "法尔法Official",   "寒泠",       "wjjjjj12", "新手上路",
            "白胃大叔", "忽悠老板❤", "I_Love_Me❤", "凤羽大姊姊❤", "蹬学长", "对岸的大姊姊❤", "解放军坦克军大佬", "高冷男神❤", "点石成√", "潜水大佬",
            "白胃大叔", "忽悠老闆❤", "I_Love_Me❤", "鳳羽大姊姊❤", "蹬學長", "對岸的大姊姊❤", "解放軍坦克軍大佬", "高冷男神❤", "點石成√", "潜水大佬"
        },
        strPlayerEffectTH = {
            "队员", "普攻特效", "施法特效"
        },
        strEnemyPosTH = {
            "敌位组",
            "X1", "Y1", "X2", "Y2", "X3",
            "Y3", "X4", "Y4", "X5", "Y5"
        },
        strLevelUpExpTH = {
            "修行", "所需经验"
        },
        strEventObjectTH = {
            "事件",
            "隐时间", "方位X", "方位Y", "图层", "触发脚本",
            "自动脚本", "状态", "触发模式", "形象号", "每面帧数",
            "方向", "当前帧数", "空闲帧数", "可偷窃？", "总帧数",
            "空闲计数"
        },
        strSceneTH = {
            "场景",
            "地图编号", "进场脚本", "传送脚本", "事件索引"
        },
        strScriptTH = {
            "指令号", "参数1", "参数2", "参数3"
        };

        //
        // Word 单辞档对应对象属性
        //
        public static string[,] strObjectTH = {
            {
                "系统对象",
                "对象名", "未定义", "未定义", "未定义", "未定义", "未定义", "未定义",
            },
            {
                "队员对象",
                "队员名", "未定义", "未定义", "愤怒脚本", "虚弱脚本", "未定义", "未定义"
            },
            {
                "道具对象",
                "道具名", "图像编号", "售价", "使用脚本", "装备脚本", "投掷脚本", "属性掩码"
            },
            {
                "仙术对象",
                "仙术名", "设定编号", "未定义", "后序脚本", "前序脚本", "未定义", "属性掩码"
            },
            {
                "敌人对象",
                "敌人名", "设定编号", "巫抗", "战前脚本", "胜利脚本", "战斗脚本", "未定义"
            },
            {
                "毒性对象",
                "毒性名", "毒性", "颜色", "我方中毒脚本", "未定义", "敌方中毒脚本", "未定义"
            }
        };

        //
        // 我方最大队列人数
        //
        public static readonly int MAX_PLAYERS_IN_PARTY = 3,
        //
        // 我方可能的最大总人数
        //
        MAX_PLAYER_ROLES = 6,
        //
        // 我方最大可入列的人数
        //
        MAX_PLAYABLE_PLAYER_ROLES = 5,
        //
        // 商店中的最大商品数
        //
        MAX_STORE_ITEM = 9,
        //
        // 玩家的最大装备数
        //
        MAX_PLAYER_EQUIPMENTS = 6,
        //
        // 仙术属性总数（五灵）
        //
        NUM_MAGIC_ELEMENTAL = 5,
        //
        // 敌方队列最大人数
        //
        MAX_ENEMIES_IN_TEAM = 5,
        //
        // 单个队员最大可习得的仙术数
        //
        MAX_PLAYER_MAGICS = 32,
        //
        // 最大场景数
        //
        MAX_SCENES = 300,
        //
        // 最大对象数
        //
        MAX_OBJECTS = 600;

        //
        // 道具属性掩码
        //
        public enum ITEMFLAG
        {
            kUsable = (1 << 0),
            kEquipable = (1 << 1),
            kThrowable = (1 << 2),
            kConsuming = (1 << 3),
            kApplyToAll = (1 << 4),
            kSellable = (1 << 5),
            kEquipableByPlayerRole_First = (1 << 6)
        }

        //
        // 仙术属性掩码
        //
        public enum MAGICFLAG
        {
            kUsableOutsideBattle = (1 << 0),
            kUsableInBattle = (1 << 1),
            kUsableToEnemy = (1 << 3),
            kApplyToAll = (1 << 4),
        }

        //
        // 资源文件
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct FILES
        {
            public FileStream fsFBP;
            public FileStream fsMGO;
            public FileStream fsBALL;
            public FileStream fsDATA;
            public FileStream fsF;
            public FileStream fsFIRE;
            public FileStream fsRGM;
            public FileStream fsSSS;

            public void
            Free()
            {
                //
                // 释放结构体资源
                //
                PAL_FreeFile(fsFBP);
                PAL_FreeFile(fsMGO);
                PAL_FreeFile(fsBALL);
                PAL_FreeFile(fsDATA);
                PAL_FreeFile(fsF);
                PAL_FreeFile(fsFIRE);
                PAL_FreeFile(fsRGM);
                PAL_FreeFile(fsSSS);
            }
        }

        //
        // 事件结构
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct EVENTOBJECT
        {
            public short sVanishTime;          // vanish time (?) 一般用于怪物这种触发战斗的事件对象，战后设置隐藏该事件对象的时间
            public ushort x;                   // X coordinate on the map 地图上的X坐标
            public ushort y;                   // Y coordinate on the map 地图上的Y坐标
            public short sLayer;               // layer value 图层
            public ushort wTriggerScript;      // Trigger script entry 调查脚本
            public ushort wAutoScript;         // Auto script entry 自动脚本
            public short sState;               // state of this object 事件状态（0=隐藏 1=飘浮 2=实体(碰撞检测) 3=特殊状态(一般用作某些对象脚本的开关)）
            public ushort wTriggerMode;        // trigger mode 触发方式（有自动触发还有调查触发）
            public ushort wSpriteNum;          // number of the sprite 形象号
            public ushort nSpriteFrames;       // total number of frames of the sprite 每个方向的图像帧数
            public ushort wDirection;          // direction 事件面朝的方向
            public ushort wCurrentFrameNum;    // current frame number 当前帧数是当前方向图像序列的第几帧
            public ushort nScriptIdleFrame;    // count of idle frames, used by trigger script
            public ushort wSpritePtrOffset;    // FIXME: ???
            public ushort nSpriteFramesAuto;   // total number of frames of the sprite, used by auto script
            public ushort wScriptIdleFrameCountAuto;     // count of idle frames, used by auto script

            public static EVENTOBJECT*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (EVENTOBJECT*)(byte**)lpBytes;
                }
            }
        }

        //
        // 场景结构
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct SCENE
        {
            public ushort wMapNum;            // number of the map 地图编号
            public ushort wScriptOnEnter;     // when entering this scene, execute script from here 该场景的进场脚本
            public ushort wScriptOnTeleport;  // when teleporting out of this scene, execute script from here 该场景的传送脚本
            public ushort wEventObjectIndex;  // event objects in this scene begins from number wEventObjectIndex + 1 事件索引号

            public static SCENE*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (SCENE*)(byte**)lpBytes;
                }
            }
        }

        //
        // 对象子结构
        //
        // items DOS版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_OTHER_DOS
        {
            public ushort wReserved0;           // always zero
            public ushort wReserved1;          // always zero
            public ushort wReserved2;          // always zero
            public ushort wReserved3;          // always zero
            public ushort wReserved4;          // always zero
            public ushort wReserved5;          // always zero

            public static OBJECT_OTHER_DOS*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_OTHER_DOS*)(byte**)lpBytes;
                }
            }
        }
        //
        // items 95版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_OTHER
        {
            public ushort wReserved0;          // always zero
            public ushort wReserved1;          // always zero
            public ushort wReserved2;          // always zero
            public ushort wReserved3;          // always zero
            public ushort wReserved4;          // always zero
            public ushort wReserved_WIN;       // always zero
            public ushort wReserved5;          // always zero

            public static OBJECT_OTHER*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_OTHER*)(byte**)lpBytes;
                }
            }
        }
        //
        // items DOS版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_PLAYER_DOS
        {
            public ushort wReserved0;           // always zero
            public ushort wReserved1;           // always zero
            public ushort wScriptOnFriendDeath; // when friends in party dies, execute script from here
            public ushort wScriptOnDying;       // when dying, execute script from here
            public ushort wReserved4;           // always zero
            public ushort wReserved5;           // always zero

            public static OBJECT_PLAYER_DOS*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_PLAYER_DOS*)(byte**)lpBytes;
                }
            }
        }
        //
        // items 95版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_PLAYER
        {
            public ushort wReserved0;           // always zero
            public ushort wReserved1;           // always zero
            public ushort wScriptOnFriendDeath; // when friends in party dies, execute script from here
            public ushort wScriptOnDying;       // when dying, execute script from here
            public ushort wReserved4;           // always zero
            public ushort wReserved_WIN;        // always zero
            public ushort wReserved5;           // always zero

            public static OBJECT_PLAYER*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_PLAYER*)(byte**)lpBytes;
                }
            }
        }
        //
        // items DOS版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_ITEM_DOS
        {
            public ushort wBitmap;         // bitmap number in BALL.MKF
            public ushort wPrice;          // price
            public ushort wScriptOnUse;    // script executed when using this item
            public ushort wScriptOnEquip;  // script executed when equipping this item
            public ushort wScriptOnThrow;  // script executed when throwing this item to enemy
            public ushort wFlags;          // flags

            public static OBJECT_ITEM_DOS*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_ITEM_DOS*)(byte**)lpBytes;
                }
            }
        }
        //
        // items 95版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_ITEM
        {
            public ushort wBitmap;         // bitmap number in BALL.MKF
            public ushort wPrice;          // price
            public ushort wScriptOnUse;    // script executed when using this item
            public ushort wScriptOnEquip;  // script executed when equipping this item
            public ushort wScriptOnThrow;  // script executed when throwing this item to enemy
            public ushort wScriptDesc_WIN; // description script
            public ushort wFlags;          // flags

            public static OBJECT_ITEM*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_ITEM*)(byte**)lpBytes;
                }
            }
        }
        //
        // magics DOS版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_MAGIC_DOS
        {
            public ushort wMagicNumber;      // magic number, according to DATA.MKF #3
            public ushort wReserved1;        // always zero
            public ushort wScriptOnSuccess;  // when magic succeed, execute script from here
            public ushort wScriptOnUse;      // when use this magic, execute script from here
            public ushort wReserved2;        // always zero
            public ushort wFlags;            // flags

            public static OBJECT_MAGIC_DOS*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_MAGIC_DOS*)(byte**)lpBytes;
                }
            }
        }
        //
        // magics 95版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_MAGIC
        {
            public ushort wMagicNumber;      // magic number, according to DATA.MKF #3
            public ushort wReserved1;        // always zero
            public ushort wScriptOnSuccess;  // when magic succeed, execute script from here
            public ushort wScriptOnUse;      // when use this magic, execute script from here
            public ushort wScriptDesc_WIN;   // description script
            public ushort wReserved2;        // always zero
            public ushort wFlags;            // flags

            public static OBJECT_MAGIC*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_MAGIC*)(byte**)lpBytes;
                }
            }
        }
        //
        // enemies
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_ENEMY_DOS
        {
            public ushort wEnemyID;        // ID of the enemy, according to DATA.MKF #1.
                                           // Also indicates the bitmap number in ABC.MKF.
            public ushort wResistanceToSorcery;  // resistance to sorcery and poison (0 min, 10 max)
            public ushort wScriptOnTurnStart;    // script executed when turn starts
            public ushort wScriptOnBattleEnd;    // script executed when battle ends
            public ushort wScriptOnReady;        // script executed when the enemy is ready
            public ushort wReserved6;            // always zero

            public static OBJECT_ENEMY_DOS*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_ENEMY_DOS*)(byte**)lpBytes;
                }
            }
        }
        //
        // enemies
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_ENEMY
        {
            public ushort wEnemyID;        // ID of the enemy, according to DATA.MKF #1.
                                           // Also indicates the bitmap number in ABC.MKF.
            public ushort wResistanceToSorcery;  // resistance to sorcery and poison (0 min, 10 max)
            public ushort wScriptOnTurnStart;    // script executed when turn starts
            public ushort wScriptOnBattleEnd;    // script executed when battle ends
            public ushort wScriptOnReady;        // script executed when the enemy is ready
            public ushort wReserved_WIN;         // always zero
            public ushort wReserved6;            // always zero

            public static OBJECT_ENEMY*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_ENEMY*)(byte**)lpBytes;
                }
            }
        }
        //
        // poisons (scripts executed in each round)
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_POISON_DOS
        {
            public ushort wPoisonLevel;    // level of the poison
            public ushort wColor;          // color of avatars
            public ushort wPlayerScript;   // script executed when player has this poison (per round)
            public ushort wReserved;       // always zero
            public ushort wEnemyScript;    // script executed when enemy has this poison (per round)
            public ushort wReserved6;      // always zero

            public static OBJECT_POISON_DOS*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_POISON_DOS*)(byte**)lpBytes;
                }
            }
        }
        //
        // poisons (scripts executed in each round)
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_POISON
        {
            public ushort wPoisonLevel;    // level of the poison
            public ushort wColor;          // color of avatars
            public ushort wPlayerScript;   // script executed when player has this poison (per round)
            public ushort wReserved;       // always zero
            public ushort wEnemyScript;    // script executed when enemy has this poison (per round)
            public ushort wReserved_WIN;   // always zero
            public ushort wReserved6;      // always zero

            public static OBJECT_POISON*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (OBJECT_POISON*)(byte**)lpBytes;
                }
            }
        }
        //
        // 对象结构 DOS版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_DOS
        {
            public OBJECT_OTHER_DOS* other;
            public OBJECT_PLAYER_DOS* player;
            public OBJECT_ITEM_DOS* item;
            public OBJECT_MAGIC_DOS* magic;
            public OBJECT_ENEMY_DOS* enemy;
            public OBJECT_POISON_DOS* poison;

            public void
            Free()
            {
                other = null;
                player = null;
                item = null;
                magic = null;
                enemy = null;
                poison = null;
            }
        }
        //
        // 对象结构 95版
        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT
        {
            public OBJECT_OTHER* other;
            public OBJECT_PLAYER* player;
            public OBJECT_ITEM* item;
            public OBJECT_MAGIC* magic;
            public OBJECT_ENEMY* enemy;
            public OBJECT_POISON* poison;

            public void
            Free()
            {
                other = null;
                player = null;
                item = null;
                magic = null;
                enemy = null;
                poison = null;
            }
        }

        // 脚本结构
        [StructLayout(LayoutKind.Sequential)]
        public struct SCRIPTENTRY
        {
            public ushort wOperation;     // operation code
            //public ushort[] rgwOperand = new ushort[3];  // operands
            public ushort rgwOperand1;  // operands
            public ushort rgwOperand2;  // operands
            public ushort rgwOperand3;  // operands

            public static SCRIPTENTRY*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (SCRIPTENTRY*)(byte**)lpBytes;
                }
            }
        }

        // 店铺结构
        [StructLayout(LayoutKind.Sequential)]
        public struct STORE
        {
            public ushort lprgItems0;
            public ushort lprgItems1;
            public ushort lprgItems2;
            public ushort lprgItems3;
            public ushort lprgItems4;
            public ushort lprgItems5;
            public ushort lprgItems6;
            public ushort lprgItems7;
            public ushort lprgItems8;

            public static STORE*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (STORE*)(byte**)lpBytes;
                }
            }
        }

        // 敌方属性结构
        [StructLayout(LayoutKind.Sequential)]
        public struct ENEMY
        {
            public ushort wIdleFrames;         // total number of frames when idle
            public ushort wMagicFrames;        // total number of frames when using magics
            public ushort wAttackFrames;       // total number of frames when doing normal attack
            public ushort wIdleAnimSpeed;      // speed of the animation when idle
            public ushort wActWaitFrames;      // FIXME: ???
            public ushort wYPosOffset;
            public short wAttackSound;         // sound played when this enemy uses normal attack
            public short wActionSound;         // FIXME: ???
            public short wMagicSound;          // sound played when this enemy uses magic
            public short wDeathSound;          // sound played when this enemy dies
            public short wCallSound;           // sound played when entering the battle
            public ushort wHealth;             // total HP of the enemy
            public ushort wExp;                // How many EXPs we'll get for beating this enemy
            public ushort wCash;               // how many cashes we'll get for beating this enemy
            public ushort wLevel;              // this enemy's level
            public ushort wMagic;              // this enemy's magic number
            public ushort wMagicRate;          // chance for this enemy to use magic
            public ushort wAttackEquivItem;    // equivalence item of this enemy's normal attack
            public ushort wAttackEquivItemRate;// chance for equivalence item
            public ushort wStealItem;          // which item we'll get when stealing from this enemy
            public ushort nStealItem;          // total amount of the items which can be stolen
            public ushort wAttackStrength;     // normal attack strength
            public ushort wMagicStrength;      // magical attack strength
            public ushort wDefense;            // resistance to all kinds of attacking
            public ushort wDexterity;          // dexterity
            public ushort wFleeRate;           // chance for successful fleeing
            public ushort wPoisonResistance;   // resistance to poison
            public ushort wElemResistance_Wind;       // resistance to elemental magics: Wind
            public ushort wElemResistance_Thunder;    // resistance to elemental magics: Thunder
            public ushort wElemResistance_Water;      // resistance to elemental magics: Water
            public ushort wElemResistance_Fire;       // resistance to elemental magics: Fire
            public ushort wElemResistance_Soil;       // resistance to elemental magics: Soil
            public ushort wPhysicalResistance; // resistance to physical attack
            public ushort wDualMove;           // whether this enemy can do dual move or not
            public ushort wCollectValue;       // value for collecting this enemy for items

            public static ENEMY*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (ENEMY*)(byte**)lpBytes;
                }
            }
        }

        // 敌方队列结构
        [StructLayout(LayoutKind.Sequential)]
        public struct ENEMYTEAM
        {
            public ushort rgwEnemy_0;
            public ushort rgwEnemy_1;
            public ushort rgwEnemy_2;
            public ushort rgwEnemy_3;
            public ushort rgwEnemy_4;

            public static ENEMYTEAM*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (ENEMYTEAM*)(byte**)lpBytes;
                }
            }
        }

        //
        // 队员初始属性结构
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct PLAYER
        {
            public ushort rgwAvatar;             // avatar (shown in status view)
            public ushort rgwSpriteNumInBattle;  // sprite displayed in battle (in F.MKF)
            public ushort rgwSpriteNum;          // sprite displayed in normal scene (in MGO.MKF)
            public ushort rgwName;               // name of player class (in WORD.DAT)
            public ushort rgwAttackAll;          // whether player can attack everyone in a bulk or not
            public ushort rgwUnknown1;           // FIXME: ???
            public ushort rgwLevel;              // level
            public ushort rgwMaxHP;              // maximum HP
            public ushort rgwMaxMP;              // maximum MP
            public ushort rgwHP;                 // current HP
            public ushort rgwMP;                 // current MP
            public ushort rgwEquipment_Head;     // equipments hat
            public ushort rgwEquipment_Bady;     // equipments clothes
            public ushort rgwEquipment_Shoulder; // equipments cloak
            public ushort rgwEquipment_Hand;     // equipments weapon
            public ushort rgwEquipment_Foot;     // equipments shoe
            public ushort rgwEquipment_Wear;     // equipments jewelry
            public ushort rgwAttackStrength;     // normal attack strength
            public ushort rgwMagicStrength;      // magical attack strength
            public ushort rgwDefense;            // resistance to all kinds of attacking
            public ushort rgwDexterity;          // dexterity
            public ushort rgwFleeRate;           // chance of successful fleeing
            public ushort rgwPoisonResistance;   // resistance to poison
            public ushort rgwElementalResistance_Wind;       // resistance to elemental magics Wind
            public ushort rgwElementalResistance_Thunder;    // resistance to elemental magics Thunder
            public ushort rgwElementalResistance_Water;      // resistance to elemental magics Water
            public ushort rgwElementalResistance_Fire;       // resistance to elemental magics Fire
            public ushort rgwElementalResistance_Soil;       // resistance to elemental magics Soil
            public ushort rgwSorceryResistance;        // 未知属性 暂时用来作为巫抗
            public ushort rgwPhysicalResistance;       // 未知属性 暂时用来作为物抗
            public ushort rgwUniqueSkillResistance;    // 未知属性 暂时用来作为绝技抗性，绝技加成
            public ushort rgwCoveredBy;                // who will cover me when I am low of HP or not sanepublic ushort rgwMagic_0;     // magics_0
            public ushort rgwMagic_0;     // magics_0
            public ushort rgwMagic_1;     // magics_1
            public ushort rgwMagic_2;     // magics_2
            public ushort rgwMagic_3;     // magics_3
            public ushort rgwMagic_4;     // magics_4
            public ushort rgwMagic_5;     // magics_5
            public ushort rgwMagic_6;     // magics_6
            public ushort rgwMagic_7;     // magics_7
            public ushort rgwMagic_8;     // magics_8
            public ushort rgwMagic_9;     // magics_9
            public ushort rgwMagic_10;    // magics_10
            public ushort rgwMagic_11;    // magics_11
            public ushort rgwMagic_12;    // magics_12
            public ushort rgwMagic_13;    // magics_13
            public ushort rgwMagic_14;    // magics_14
            public ushort rgwMagic_15;    // magics_15
            public ushort rgwMagic_16;    // magics_16
            public ushort rgwMagic_17;    // magics_17
            public ushort rgwMagic_18;    // magics_18
            public ushort rgwMagic_19;    // magics_19
            public ushort rgwMagic_20;    // magics_20
            public ushort rgwMagic_21;    // magics_21
            public ushort rgwMagic_22;    // magics_22
            public ushort rgwMagic_23;    // magics_23
            public ushort rgwMagic_24;    // magics_24
            public ushort rgwMagic_25;    // magics_25
            public ushort rgwMagic_26;    // magics_26
            public ushort rgwMagic_27;    // magics_27
            public ushort rgwMagic_28;    // magics_28
            public ushort rgwMagic_29;    // magics_29
            public ushort rgwMagic_30;    // magics_30
            public ushort rgwMagic_31;    // magics_31
            public ushort rgwWalkFrames;         // walk frame (???)
            public ushort rgwCooperativeMagic;   // cooperative magic
            public ushort rgwMaxSP;              // 未知属性 暂时用作 maximum SP
            public ushort rgwSP;                 // 未知属性 暂时用作 current SP
            public ushort rgwDeathSound;         // sound played when player dies
            public ushort rgwAttackSound;        // sound played when player attacks
            public ushort rgwWeaponSound;        // weapon sound (???)
            public ushort rgwCriticalSound;      // sound played when player make critical hits
            public ushort rgwMagicSound;         // sound played when player is casting a magic
            public ushort rgwCoverSound;         // sound played when player cover others
            public ushort rgwDyingSound;         // sound played when player is dying

            public static PLAYER*
            ToBase(
                ushort[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    ushort* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (PLAYER*)(ushort**)lpBytes;
                }
            }
        }

        //
        // 所有队员初始属性结构
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct PLAYERROLES
        {
            public PLAYER* LiXiaoYao;          // 我方属性结构_李逍遥
            public ushort[] binLiXiaoYao;      // 我方属性数据_李逍遥

            public PLAYER* ZhaoLingEr;         // 我方属性结构_赵灵儿
            public ushort[] binZhaoLingEr;     // 我方属性数据_赵灵儿

            public PLAYER* LinYueRu;           // 我方属性结构_林月如
            public ushort[] binLinYueRu;       // 我方属性数据_林月如

            public PLAYER* WuHou;              // 我方属性结构_巫后
            public ushort[] binWuHou;          // 我方属性数据_巫后

            public PLAYER* ANu;                // 我方属性结构_阿奴
            public ushort[] binANu;            // 我方属性数据_阿奴

            public PLAYER* GaiLuoJiao;         // 我方属性结构_盖罗娇
            public ushort[] binGaiLuoJiao;     // 我方属性数据_盖罗娇

            public PLAYER*
            GetPlayer(
                int iPlayerID
            )
            {
                iPlayerID %= MAX_PLAYER_ROLES;

                switch (iPlayerID)
                {
                    default:
                    case 0:
                        fixed (
                            ushort[]* lpEnemyTeam = &binLiXiaoYao
                        )
                        {
                            return PLAYER.ToBase(lpEnemyTeam);
                        }

                    case 1:
                        fixed (
                            ushort[]* lpEnemyTeam = &binZhaoLingEr
                        )
                        {
                            return PLAYER.ToBase(lpEnemyTeam);
                        }

                    case 2:
                        fixed (
                            ushort[]* lpEnemyTeam = &binLinYueRu
                        )
                        {
                            return PLAYER.ToBase(lpEnemyTeam);
                        }

                    case 3:
                        fixed (
                            ushort[]* lpEnemyTeam = &binWuHou
                        )
                        {
                            return PLAYER.ToBase(lpEnemyTeam);
                        }

                    case 4:
                        fixed (
                            ushort[]* lpEnemyTeam = &binANu
                        )
                        {
                            return PLAYER.ToBase(lpEnemyTeam);
                        }

                    case 5:
                        fixed (
                            ushort[]* lpEnemyTeam = &binGaiLuoJiao
                        )
                        {
                            return PLAYER.ToBase(lpEnemyTeam);
                        }
                }
            }

            public void
            Free()
            {
                //
                // 释放结构体数据
                //
                LiXiaoYao = null;
                PAL_FreeGameBin(ref binLiXiaoYao);

                ZhaoLingEr = null;
                PAL_FreeGameBin(ref binZhaoLingEr);

                LinYueRu = null;
                PAL_FreeGameBin(ref binLinYueRu);

                WuHou = null;
                PAL_FreeGameBin(ref binWuHou);

                ANu = null;
                PAL_FreeGameBin(ref binANu);

                GaiLuoJiao = null;
                PAL_FreeGameBin(ref binGaiLuoJiao);
            }
        }

        // 敌方属性结构
        [StructLayout(LayoutKind.Sequential)]
        public struct MAGIC
        {
            public ushort wEffect;               // effect sprite
            public ushort wType;                 // type of this magic
            // public ushort wXOffset;
            // public ushort wYOffset;
            public short sXOffset;
            public short sYOffset;
            public ushort wSummonEffect;         // summon effect sprite (in F.MKF)
            public short wSpeed;                 // speed of the effect
            public ushort wKeepEffect;           // FIXME: ???
            public ushort wFireDelay;            // start frame of the magic fire stage
            // public ushort wEffectTimes;          // total times of effect
            public short sEffectTimes;          // total times of effect
            public ushort wShake;                // shake screen
            public ushort wWave;                 // wave screen
            public ushort wCostSP;               // 未知，暂定为 SP cost
            public ushort wCostMP;               // MP cost
            public ushort wBaseDamage;           // base damage
            public ushort wElemental;            // elemental (0 = No Elemental, last = poison)
            public short wSound;                 // sound played when using this magic

            public static MAGIC*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (MAGIC*)(byte**)lpBytes;
                }
            }
        }

        // 战场卦象结构
        [StructLayout(LayoutKind.Sequential)]
        public struct BATTLEFIELD
        {
            public ushort wScreenWave;                      // level of screen waving
            public short rgsMagicEffect_Wind;               // effect of attributed magics Wind
            public short rgsMagicEffect_Thunder;            // effect of attributed magics Thunder
            public short rgsMagicEffect_Water;              // effect of attributed magics Water
            public short rgsMagicEffect_Fire;               // effect of attributed magics Fire
            public short rgsMagicEffect_Soil;               // effect of attributed magics Soil

            public static BATTLEFIELD*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (BATTLEFIELD*)(byte**)lpBytes;
                }
            }
        }

        //
        // 仙术所需修行结构
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct LEVELUPMAGIC
        {
            public ushort wLevel;    // level reached
            public ushort wMagic;    // magic learned

            public static LEVELUPMAGIC*
            ToBase(
                uint[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    uint* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (LEVELUPMAGIC*)(uint**)lpBytes;
                }
            }
        }

        //
        // 所有队员的仙术所需修行结构
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct LEVELUPMAGIC_ALL
        {
            public LEVELUPMAGIC* LiXiaoYao;
            public uint[] binLiXiaoYao;

            public LEVELUPMAGIC* ZhaoLingEr;
            public uint[] binZhaoLingEr;

            public LEVELUPMAGIC* LinYueRu;
            public uint[] binLinYueRu;

            public LEVELUPMAGIC* WuHou;
            public uint[] binWuHou;

            public LEVELUPMAGIC* ANu;
            public uint[] binANu;

            public LEVELUPMAGIC*
            GetLevelUpMagic(
                int iPlayerID
            )
            {
                iPlayerID %= MAX_PLAYABLE_PLAYER_ROLES;

                switch (iPlayerID)
                {
                    default:
                    case 0:
                        fixed (
                            uint[]* lpLevelUpMagic = &binLiXiaoYao
                        )
                        {
                            return LEVELUPMAGIC.ToBase(lpLevelUpMagic);
                        }

                    case 1:
                        fixed (
                            uint[]* lpLevelUpMagic = &binZhaoLingEr
                        )
                        {
                            return LEVELUPMAGIC.ToBase(lpLevelUpMagic);
                        }

                    case 2:
                        fixed (
                            uint[]* lpLevelUpMagic = &binLinYueRu
                        )
                        {
                            return LEVELUPMAGIC.ToBase(lpLevelUpMagic);
                        }

                    case 3:
                        fixed (
                            uint[]* lpLevelUpMagic = &binWuHou
                        )
                        {
                            return LEVELUPMAGIC.ToBase(lpLevelUpMagic);
                        }

                    case 4:
                        fixed (
                            uint[]* lpLevelUpMagic = &binANu
                        )
                        {
                            return LEVELUPMAGIC.ToBase(lpLevelUpMagic);
                        }
                }
            }

            public void
            Free()
            {
                //
                // 释放结构体数据
                //
                LiXiaoYao = null;
                PAL_FreeGameBin(ref binLiXiaoYao);

                ZhaoLingEr = null;
                PAL_FreeGameBin(ref binZhaoLingEr);

                LinYueRu = null;
                PAL_FreeGameBin(ref binLinYueRu);

                WuHou = null;
                PAL_FreeGameBin(ref binWuHou);

                ANu = null;
                PAL_FreeGameBin(ref binANu);
            }
        }

        // 队员战斗动作特效设定结构
        [StructLayout(LayoutKind.Sequential)]
        public struct FIGHTEFFECT
        {
            public ushort wAttackEffectID;    // 队员普攻行动特效
            public ushort wMagicEffectID;     // 队员施法行动特效

            public static FIGHTEFFECT*
            ToBase(
                byte[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    byte* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (FIGHTEFFECT*)(byte**)lpBytes;
                }
            }
        }

        public struct PALPOS
        {
            public ushort x;
            public ushort y;

            public static PALPOS*
            ToBase(
                uint[]* buf
            )
            {
                // 固定指针，以免内存爆炸
                fixed (
                    uint* lpBytes = *buf
                )
                {
                    // 使事件指针指向其对应的字节数组
                    return (PALPOS*)(uint**)lpBytes;
                }
            }
        }

        public struct ENEMYPOS
        {
            public PALPOS* pos_One;
            public uint[] binPos_One;

            public PALPOS* pos_Two;
            public uint[] binPos_Two;

            public PALPOS* pos_Three;
            public uint[] binPos_Three;

            public PALPOS* pos_Four;
            public uint[] binPos_Four;

            public PALPOS* pos_Five;
            public uint[] binPos_Five;

            public PALPOS*
            GetEnemyPos(
                int nEnemy
            )
            {
                nEnemy %= MAX_ENEMIES_IN_TEAM + 1;

                switch (nEnemy)
                {
                    default:
                    case 1:
                        fixed (
                            uint[]* lpEnemyTeam = &binPos_One
                        )
                        {
                            return PALPOS.ToBase(lpEnemyTeam);
                        }

                    case 2:
                        fixed (
                            uint[]* lpEnemyTeam = &binPos_Two
                        )
                        {
                            return PALPOS.ToBase(lpEnemyTeam);
                        }

                    case 3:
                        fixed (
                            uint[]* lpEnemyTeam = &binPos_Three
                        )
                        {
                            return PALPOS.ToBase(lpEnemyTeam);
                        }

                    case 4:
                        fixed (
                            uint[]* lpEnemyTeam = &binPos_Four
                        )
                        {
                            return PALPOS.ToBase(lpEnemyTeam);
                        }

                    case 5:
                        fixed (
                            uint[]* lpEnemyTeam = &binPos_Five
                        )
                        {
                            return PALPOS.ToBase(lpEnemyTeam);
                        }
                }
            }

            public void
            Free()
            {
                pos_Five = null;
                PAL_FreeGameBin(ref binPos_Five);

                pos_Four = null;
                PAL_FreeGameBin(ref binPos_Four);

                pos_Three = null;
                PAL_FreeGameBin(ref binPos_Three);

                pos_Two = null;
                PAL_FreeGameBin(ref binPos_Two);

                pos_One = null;
                PAL_FreeGameBin(ref binPos_One);
            }
        }

        //
        // game data which is available in data files.
        // 数据文件中可用的游戏数据
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct GAMEDATA
        {
            public EVENTOBJECT* lprgEventObject;    // 事件结构
            public uint nEventObject;               // 事件数目
            public byte[] binEventObject;           // 事件数据

            public SCENE* lprgScene;                // 场景结构
            public uint nScene;                     // 场景数目，将此值限制在 MAX_SCENES 以下
            public byte[] binScene;                 // 场景数据

            public OBJECT_DOS rgObject_DOS;         // 对象结构_DOS
            public OBJECT rgObject;                 // 对象结构_WIN
            public uint nObject;                    // 对象数目,将此值限制在 MAX_OBJECTS 以下
            public byte[] binObject;                // 对象数据

            public uint* lprgMsgIndex;              // 对话索引结构
            public uint nMsgIndex;                  // 对话索引数目
            public byte[] binMsgIndex;              // 对话索引数据

            public SCRIPTENTRY* lprgScriptEntry;    // 脚本结构
            public uint nScriptEntry;               // 脚本数目
            public byte[] binScriptEntry;           // 脚本数据

            public STORE* lprgStore;                // 店铺结构
            public uint nStore;                     // 店铺数目,此值限制在 MAX_STORE_ITEM 以下
            public byte[] binStore;                 // 店铺数据

            public ENEMY* lprgEnemy;                // 敌方属性结构
            public uint nEnemy;                     // 敌方属性数目
            public byte[] binEnemy;                 // 敌方属性数据

            public ENEMYTEAM* lprgEnemyTeam;        // 敌方队列店铺结构
            public uint nEnemyTeam;                 // 敌方队列数目,此值限制在 MAX_ENEMIES_IN_TEAM 以下 
            public byte[] binEnemyTeam;             // 敌方队列数据

            public PLAYERROLES rgPlayerRoles;       // 我方属性结构
            public byte[] binPlayerRoles;           // 我方属性数据

            public MAGIC* lprgMagic;                // 仙术属性结构
            public uint nMagic;                     // 仙术属性数目
            public byte[] binMagic;                 // 仙术属性数据

            public BATTLEFIELD* lprgBattleField;    // 战场卦象结构
            public uint nBattleField;               // 战场卦象数目
            public byte[] binBattleField;           // 战场卦象数据

            public LEVELUPMAGIC_ALL rgLevelUpMagic;      // 仙术所需修行结构
            public byte[] binLevelUpMagic;               // 仙术所需修行数据

            public FIGHTEFFECT* lprgFightEffect;    // 队员战斗动作特效设定结构
            public uint nFightEffect;               // 队员战斗动作特效设定数目
            public byte[] binFightEffect;           // 队员战斗动作特效设定数据

            public ENEMYPOS rgEnemyPos;             // 仙术所需修行结构
            public uint nEnemyPos;                  // 仙术所需修行数目
            public byte[] binEnemyPos;              // 仙术所需修行数据

            public uint* lprgLevelUpExp;            // 队员每修行所需经验结构
            public uint nLevelUpExp;                // 队员每修行所需经验数目
            public byte[] binLevelUpExp;            // 队员每修行所需经验数据

            public EVENTOBJECT*
            GetEventObject()
            {
                fixed (
                    byte[]* lpEvent = &binEventObject
                )
                {
                    return EVENTOBJECT.ToBase(lpEvent);
                }
            }

            public SCENE*
            GetScene()
            {
                fixed (
                    byte[]* lpScene = &binScene
                )
                {
                    return SCENE.ToBase(lpScene);
                }
            }

            public OBJECT_OTHER_DOS*
            GetObject_DOS()
            {
                fixed (
                    byte[]* lpObject_DOS = &binObject
                )
                {
                    return OBJECT_OTHER_DOS.ToBase(lpObject_DOS);
                }
            }

            public OBJECT_OTHER*
            GetObject()
            {
                fixed (
                    byte[]* lpObject = &binObject
                )
                {
                    return OBJECT_OTHER.ToBase(lpObject);
                }
            }

            public SCRIPTENTRY*
            GetScriptEntry()
            {
                fixed (
                    byte[]* lpScriptEntry = &binScriptEntry
                )
                {
                    return SCRIPTENTRY.ToBase(lpScriptEntry);
                }
            }

            public STORE*
            GetStore()
            {
                fixed (
                    byte[]* lpStore = &binStore
                )
                {
                    return STORE.ToBase(lpStore);
                }
            }

            public ENEMY*
            GetEnemy()
            {
                fixed (
                    byte[]* lpEnemy = &binEnemy
                )
                {
                    return ENEMY.ToBase(lpEnemy);
                }
            }

            public ENEMYTEAM*
            GetEnemyTeam()
            {
                fixed (
                    byte[]* lpEnemyTeam = &binEnemyTeam
                )
                {
                    return ENEMYTEAM.ToBase(lpEnemyTeam);
                }
            }

            public MAGIC*
            GetMagic()
            {
                fixed (
                    byte[]* lpMagic = &binMagic
                )
                {
                    return MAGIC.ToBase(lpMagic);
                }
            }

            public BATTLEFIELD*
            GetBattleField()
            {
                fixed (
                    byte[]* lpBattleField = &binBattleField
                )
                {
                    return BATTLEFIELD.ToBase(lpBattleField);
                }
            }

            public FIGHTEFFECT*
            GetFightEffect()
            {
                fixed (
                    byte[]* lpFightEffect = &binFightEffect
                )
                {
                    return FIGHTEFFECT.ToBase(lpFightEffect);
                }
            }

            public ushort[]*
            GetLevelUpExp()
            {
                fixed (
                    byte[]* lpLevelUpExp = &binLevelUpExp
                )
                {
                    return (ushort[]*)lpLevelUpExp;
                }
            }

            public void
            Free()
            {
                //
                // 释放游戏数据结构体数据
                //
                lprgEventObject = null;
                PAL_FreeGameBin(ref binEventObject);

                lprgScene = null;
                PAL_FreeGameBin(ref binScene);

                rgObject_DOS.Free();
                rgObject.Free();
                PAL_FreeGameBin(ref binObject);

                lprgMsgIndex = null;
                PAL_FreeGameBin(ref binMsgIndex);

                lprgScriptEntry = null;
                PAL_FreeGameBin(ref binScriptEntry);

                lprgStore = null;
                PAL_FreeGameBin(ref binStore);

                lprgEnemy = null;
                PAL_FreeGameBin(ref binEnemy);

                lprgEnemyTeam = null;
                PAL_FreeGameBin(ref binEnemyTeam);

                rgPlayerRoles.Free();
                PAL_FreeGameBin(ref binPlayerRoles);

                lprgMagic = null;
                PAL_FreeGameBin(ref binMagic);

                lprgBattleField = null;
                PAL_FreeGameBin(ref binBattleField);

                rgLevelUpMagic.Free();
                PAL_FreeGameBin(ref binLevelUpMagic);

                lprgFightEffect = null;
                PAL_FreeGameBin(ref binFightEffect);

                rgEnemyPos.Free();
                PAL_FreeGameBin(ref binEnemyPos);
            }
        }

        //
        // Word 单辞档列表
        // Msg 对话档列表
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct TEXTLIST
        {
            public string[] saWords;          // 对象名称数据
            public uint nWords;               // 对象名称数目

            public string[] saMsgs;           // 对话数据
            public uint nMsgs;                // 对话数目

            public void
            Free()
            {
                //
                // 释放消息结构体数据
                //
                PAL_FreeGameBin(ref saWords);

                PAL_FreeGameBin(ref saMsgs);
            }
        }

        //
        // 全局整体数据
        //
        [StructLayout(LayoutKind.Sequential)]
        public struct GLOBALVARS
        {
            public FILES f;
            public GAMEDATA g;
            public TEXTLIST g_TextList;

            public void
            Free()
            {
                //
                // 释放结构体内存
                //
                f.Free();

                g.Free();

                g_TextList.Free();
            }
        }

        //
        // 全部资源文件
        //
        public static GLOBALVARS gpGlobals = new GLOBALVARS();
    }
}
