<div align="center">

<img src="http://blessing-studio.cn/wp-content/uploads/2024/06/组件-8.png"/>

# MinecraftLaunch


### 跨平台的 C# Minecraft启动器核心

![Star](https://img.shields.io/github/stars/Blessing-Studio/MinecraftLaunch?logo=github&label=Star&style=for-the-badge)
![License](https://img.shields.io/github/license/Blessing-Studio/MinecraftLaunch?logo=github&label=开源协议&style=for-the-badge&color=ff7a35)
![NugetVersion](https://img.shields.io/nuget/v/MinecraftLaunch?logo=nuget&label=Nuget包版本&style=for-the-badge)
![NugetDownload](https://img.shields.io/nuget/dt/MinecraftLaunch?logo=nuget&label=Nuget包下载量&style=for-the-badge)

</div>

## 关于

这个项目旨在帮助开发人员更快的开发自己的启动Minecraft的项目，它可以被用在由C# .NET开发的面向Windows，MacOS，Linux其一或跨平台的启动器上.

## 项目支持的内容

- 支持桌面平台的跨平台调用 (Windows/Linux/Mac上的调试均已通过)

- Minecraft游戏核心的查找

- Minecraft的参数生成、启动封装

- 对离线、微软、外置、统一通行证验证的支持

- 支持多线程高速补全Assets、Libraries等游戏资源

- 支持自动安装Forge、Fabric、OptiFine、Quilt、NeoForged加载器

- 支持对CurseForge,Modrinth的api的封装

- 支持游戏崩溃探测

- 支持游戏日志解析

- 支持对游戏存档、模组、资源包的解析、管理

- 支持从Bmclapi、Mcbbs下载源进行文件补全

## 使用此项目的条件

- 你的项目只能支持Windows，MacOS，Linux其一或跨平台.
- 你的项目必须使用C# .NET 7.0及以上.
- 你的项目不是UWP项目.

## 安装此项目到你的项目里

MinecraftLaunch作为NuGet包发布，你可以在任意NuGet包管理器安装到你的项目里或使用命令行安装：

```
dotnet add package MinecraftLaunch
```

你可以通过以下命令安装旧版本MinecraftLaunch：
```
dotnet add package MinecraftLaunch --version 2.0.0
```
有特殊需求？你可以手动从 [这里](https://www.nuget.org/packages/MinecraftLaunch) 寻找你想要的安装方法.

## 使用教程

现在，MinecraftLaunch的改动几乎使得Xilu Blog里的教程无法在新版MinecraftLaunch上使用，所以你可能无法从旧文档得到较多帮助.

不过，我们准备好了新文档！

https://www.blessingta.link/

### Tips：MinecraftLaunch的新文档暂未开发完成，所以有一部分文档会显示404.

仍然无法获得帮助？

你可以加入我们的群聊一起讨论，我们会尽可能的帮助你.

你可以通过以下方式加入我们的群聊：

- QQ群：682528253
- QQ频道：https://pd.qq.com/s/5eqzllk3y

## 贡献者

### [Xilu](https://baka_hs.gitee.io/xilu-baka/)

一个平平无奇的 C# 程序设计爱好者，平时喜欢整有用的以及没用的烂活

[GitHub](https://github.com/YangSpring114)
[Bilibili](https://space.bilibili.com/1098028524?spm_id_from=333.999.0.0)
[爱发电](https://afdian.net/a/WonderLab)

### Ddggdd135

一个喜欢编程的学生, Minecraft肝帝, C#爱好者

[GitHub](https://github.com/JWJUN233233)
[Bilibili](https://space.bilibili.com/1049351987)

### Starcloudsea

啥活都想整的没脑子C#编程爱好者和视频创作者

啥都没在做，因为Mac没到（悲）

[GitHub](https://github.com/Starcloudsea)
[Bilibili](https://space.bilibili.com/2123349162?spm_id_from=333.1007.0.0)
[爱发电](https://afdian.net/a/Starcloudsea)

## 开源协议

这个项目在MIT许可下分发，具体详情可见 [LICENSE](还没创建) .
