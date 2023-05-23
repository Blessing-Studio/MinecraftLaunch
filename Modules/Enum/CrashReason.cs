using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Enum
{
    public enum CrashReason
    {
        //妈的懒得写英文了，整的多长一串
        //Game
        文件或内容校验失败,
        特定方块导致崩溃,
        特定实体导致崩溃,
        材质过大或显卡配置不足,
        光影或资源包导致OpenGL1282错误,
        无法加载纹理,

        //Mod
        Mod配置文件导致游戏崩溃,
        ModMixin失败,
        Mod加载器报错,
        Mod初始化失败,
        Mod文件被解压,
        Mod过多导致超出ID限制,
        Mod导致游戏崩溃,
        Mod重复安装,

        //ModLoader
        OptiFine与Forge不兼容,
        Fabric报错,
        Fabric报错并给出解决方案,
        Forge报错,
        低版本Forge与高版本Java不兼容,
        版本Json中存在多个Forge,
        OptiFine导致无法加载世界,

        //Log
        崩溃日志堆栈分析发现关键字,
        崩溃日志堆栈分析发现Mod名称,
        MC日志堆栈分析发现关键字,
        
        //Jvm
        内存不足,
        使用JDK,
        显卡不支持OpenGL,
        使用OpenJ9,
        Java版本过高,
        不支持的Java类版本错误,
        使用32位Java导致JVM无法分配足够多的内存,

        //Player
        玩家手动触发调试崩溃,
    }
}
