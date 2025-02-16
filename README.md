<div align="center">

<img Height="100" Width="100" src="https://blessing-studio.cn/wp-content/uploads/2025/01/IMG_2186.png"/>

# MinecraftLaunch

### 模块化高性能的 Minecraft 启动核心

![Star](https://img.shields.io/github/stars/Blessing-Studio/MinecraftLaunch?logo=github&label=Star&style=for-the-badge)
![License](https://img.shields.io/github/license/Blessing-Studio/MinecraftLaunch?logo=github&label=开源协议&style=for-the-badge&color=ff7a35)
![NugetVersion](https://img.shields.io/nuget/v/MinecraftLaunch?logo=nuget&label=Nuget包版本&style=for-the-badge)
![NugetDownload](https://img.shields.io/nuget/dt/MinecraftLaunch?logo=nuget&label=Nuget包下载量&style=for-the-badge)

</div>

## 关于

此项目基于 .NET 8 编写，几乎所有组件均已实现异步化，部分组件也已支持 MVVM 模式的服务调用

>
> **注意：**
> 
> 4.0 版本进行了全面重构，所有组件与 3.0 均不支持，在升级时请参考文档的升级须知！
>

## 未来更新计划

| 功能                                                             | 状态                |
| ---------------------------------------------------------------  | ------------------- |
| 离线账户验证器                                                    | 🟩                 |
| 外置账户验证器                                                    | 🟩                 |
| 微软账户验证器                                                    | 🟩                 |
| 原版核心安装器                                                    | 🟩                 |
| Java 安装器                                                       | 🟥                |
| Curseforge 格式整合包安装器                                        | 🟩                 |
| Modrinth 格式整合包安装器                                          | 🟩                 |
| Forge（Neo） 安装器                                               | 🟩                 |
| Optifine 安装器                                                   | 🟩                 |
| Fabric 安装器                                                     | 🟩                 |
| Quilt 安装器                                                      | 🟩                 | 
| 游戏日志分析                                                       | 🟩                 |
| 游戏崩溃分析                                                       | 🟩                 |
| 查找 .minecraft 中的游戏核心                                       | 🟩                 |
| 创建、启动、管理 Minecraft 进程                                    | 🟩                 |
| 管理 launchprofile.json 的数据                                     | 🟩                |
| 查找已安装的 Java 运行时                                           | 🟩                |
| 支持第三方下载镜像源 [BMCLAPI](https://bmclapidoc.bangbang93.com/) | 🟩                |


## 安装此项目到你的项目里

MinecraftLaunch作为 NuGet 包发布，你可以在任意 NuGet 包管理器安装到你的项目里或使用命令行安装：

```
dotnet add package MinecraftLaunch
```

你可以通过以下命令安装旧版本MinecraftLaunch：

```
dotnet add package MinecraftLaunch --version 3.0.0
```

有特殊需求？你可以手动从 [这里](https://www.nuget.org/packages/MinecraftLaunch) 寻找你想要的安装方法.

## 使用教程

现在，MinecraftLaunch的改动几乎使得 Xilu Blog 里的教程无法在新版 MinecraftLaunch 上使用，所以你可能无法从旧文档得到较多帮助.

不过，我们准备好了[新文档](https://wiki.blessing-studio.cn/)！



### 与我们联系

仍然无法获得帮助？

你可以加入我们的群聊一起讨论，我们会尽可能的帮助你.

你可以通过以下方式加入我们的群聊：

- QQ群：682528253
- QQ频道：https://pd.qq.com/s/5eqzllk3y

## 鸣谢

此项目部分代码参考了 [Fluent Core](https://github.com/Xcube-Studio/Natsurainko.FluentCore) 的代码

整合包安装器代码参考了 [ProjBobcat](https://github.com/Corona-Studio/ProjBobcat) 的代码

在此由衷感谢以上核心的作者


## 开源协议

这个项目在 MIT 许可下分发，具体详情可见 [LICENSE](还没创建) .
