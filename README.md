# MinecraftLaunch
![](https://img.shields.io/badge/license-MIT-green)
![](https://img.shields.io/github/repo-size/Blessing-Studio/MinecraftLaunch)
![](https://img.shields.io/github/stars/Blessing-Studio/MinecraftLaunch)
![](https://img.shields.io/github/commit-activity/y/Blessing-Studio/MinecraftLaunch)
![](https://img.shields.io/nuget/v/MinecraftLaunch?logo=nuget&label=NuGet版本)
![](https://img.shields.io/nuget/dt/MinecraftLaunch?logo=nuget&label=NuGet下载量)

下一代全能模块化的 Minecraft 启动核心
---------------------------------------------------------
### 简介
一个由C#编写的跨平台模块化 Minecraft 启动核心

+ 支持桌面平台的跨平台调用 (Windows/Linux/Mac上的调试均已通过)
+ Minecraft游戏核心的查找
+ Minecraft的参数生成、启动封装
+ 对离线、微软、外置登录验证的支持
+ 支持多线程高速补全Assets、Libraries等游戏资源
+ 支持自动安装Forge、Fabric、OptiFine,Quilt加载器
+ 支持对CurseForge,Modrinth的api的封装
+ 支持游戏崩溃探测
+ 支持游戏日志解析
+ 支持对游戏存档、模组、资源包的解析、管理
+ 支持从[Bmclapi、Mcbbs](https://bmclapidoc.bangbang93.com/)下载源进行文件补全
  + 在此感谢bangbang93提供镜像站服务 如果您支持我们 可以 [赞助Bmclapi](https://afdian.net/@bangbang93)
  
  本项目依赖框架: .Net6.0 或 .Net7.0
  
###  声明
+ BMCLAPI是@bangbang93开发的BMCL的一部分，用于解决国内线路对Forge和Minecraft官方使用的Amazon S3 速度缓慢的问题。BMCLAPI是对外开放的，所有需要Minecraft资源的启动器均可调用。
+ 感谢开发过程中大佬[natsurainko](https://github.com/Natsurainko)给出的建议和指导 不妨也看看他的启动核心项目[Natsurainko.FluentCore](https://github.com/Xcube-Studio/Natsurainko.FluentCore)
+ 感谢开发过程中大佬[laolarou726](https://github.com/laolarou726)给出的建议和指导 不妨也看看他的启动核心项目[Projbobcat](https://github.com/Corona-Studio/ProjBobcat)

###  使用本核心制作的一些启动器
+ ModernCraft Launcher（MCL）
+ Mexico Launcher
+ ThinICE Launcher
+ NoName Launcher
+ LobeCraft Launcher
+ Lemon Launcher
+ Rain s Minecraft Launcher
+ [BadMcen Launcher](https://github.com/BadMC-Studio/BadMcen-launcher)
# [使用文档](https://baka_hs.gitee.io/xilu-baka/)
