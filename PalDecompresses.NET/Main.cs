using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

using static PalDecompresses.Global;
using static PalDecompresses.FileUtil;
using static PalDecompresses.PalUtil;

namespace PalDecompresses
{
    public unsafe class PalDecompresses
    {
        public static void
        Main(
            string[] args
        )
        {
            //
            // 初始化全局数据
            //
            PAL_Log("DATA and SSS: 全局数据初始化中...........................", FALSE);
            PAL_InitGlobals();
            PAL_Log("完毕！");

            //
            // 读取字符消息档
            //
            PAL_Log("WORD: 单辞档初始化中.....................................", FALSE);
            PAL_InitWord();
            PAL_Log("完毕！");

            PAL_Log("MSG: 对话档初始化中......................................", FALSE);
            PAL_InitMessages();
            PAL_Log("完毕！");

            //
            // 保存全局数据为 TSV 表格档案
            //
            PAL_Log("SCENE: 场景事件数据导出中................................", FALSE);
            PAL_SaveGlobalsAsTSV("Scene");
            PAL_Log("完毕！");

            PAL_Log("OBJECT: 对象数据导出中...................................", FALSE);
            PAL_SaveGlobalsAsTSV("Object");
            PAL_Log("完毕！");

            PAL_Log("SCRIPT: 脚本数据导出中...................................", FALSE);
            PAL_SaveGlobalsAsTSV("ScriptEntry");
            PAL_Log("完毕！");

            PAL_Log("STORE: 货郎数据导出中....................................", FALSE);
            PAL_SaveGlobalsAsTSV("Store");
            PAL_Log("完毕！");

            PAL_Log("ENEMY: 敌方设定数据导出中................................", FALSE);
            PAL_SaveGlobalsAsTSV("Enemy");
            PAL_Log("完毕！");

            PAL_Log("ENEMYTEAM: 敌方队列数据导出中............................", FALSE);
            PAL_SaveGlobalsAsTSV("EnemyTeam");
            PAL_Log("完毕！");

            PAL_Log("PLAYERROLES: 队员设定数据导出中..........................", FALSE);
            PAL_SaveGlobalsAsTSV("PlayerRoles");
            PAL_Log("完毕！");

            PAL_Log("MAGIC: 仙术设定数据导出中................................", FALSE);
            PAL_SaveGlobalsAsTSV("Magic");
            PAL_Log("完毕！");

            PAL_Log("BATTLEFIELD: 战场设定数据导出中..........................", FALSE);
            PAL_SaveGlobalsAsTSV("BattleField");
            PAL_Log("完毕！");

            PAL_Log("LEVELUPMAGIC: 仙术所需等级数据导出中.....................", FALSE);
            PAL_SaveGlobalsAsTSV("LevelUpMagic");
            PAL_Log("完毕！");

            PAL_Log("FIGHTEFFECT: 队员行动特效序列帧设定数据导出中............", FALSE);
            PAL_SaveGlobalsAsTSV("FightEffect");
            PAL_Log("完毕！");

            PAL_Log("ENEMYPOS: 敌方队列战场坐标设定数据导出中.................", FALSE);
            PAL_SaveGlobalsAsTSV("EnemyPos");
            PAL_Log("完毕！");

            PAL_Log("LEVELUPEXP: 队员每修行所需经验设定数据导出中.............", FALSE);
            PAL_SaveGlobalsAsTSV("LevelUpExp");
            PAL_Log("完毕！");

            //
            // 退出程序
            //
            PAL_Shutdown();
        }
    }
}
