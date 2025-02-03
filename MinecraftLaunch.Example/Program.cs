using MinecraftLaunch;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Parser;
using System.Net;

DownloadMirrorManager.MaxThread = 256;
DownloadMirrorManager.IsEnableMirror = false;
ServicePointManager.DefaultConnectionLimit = int.MaxValue;

#region 原版安装器

//var stopwatch = Stopwatch.StartNew();
//var entry = await VanillaInstaller.EnumerableMinecraftAsync()
//    .FirstAsync(x => x.Id == "1.20.1");

//var installer = VanillaInstaller.Create("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft", entry);
//installer.ProgressChanged += (_, arg) =>
//    Console.WriteLine($"{arg.StepName} - {arg.FinishedStepTaskCount}/{arg.TotalStepTaskCount} - {(arg.IsStepSupportSpeed ? $"{FileDownloader.GetSpeedText(arg.Speed)} - {arg.Progress * 100:0.00}%" : $"{arg.Progress * 100:0.00}%")}");

//var minecraft = await installer.InstallAsync();
//Console.WriteLine(minecraft.Id);

#endregion

#region Forge 安装器

//var entry1 = await ForgeInstaller.EnumerableForgeAsync("1.20.1", false)
//    .FirstOrDefaultAsync();

//var installer1 = ForgeInstaller.Create("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft", "C:\\Program Files\\Java\\latest\\jre-1.8\\bin\\java.exe", entry1);
//installer1.ProgressChanged += (_, arg) =>
//    Console.WriteLine($"{arg.StepName} - {arg.FinishedStepTaskCount}/{arg.TotalStepTaskCount} - {(arg.IsStepSupportSpeed ? $"{FileDownloader.GetSpeedText(arg.Speed)} - {arg.Progress * 100:0.00}%" : $"{arg.Progress * 100:0.00}%")}");

//var minecraft1 = await installer1.InstallAsync();
//Console.WriteLine(minecraft1.Id);

#endregion

#region Optifine 安装器

//var entry2 = await OptifineInstaller.EnumerableOptifineAsync("1.12.2")
//    .LastOrDefaultAsync();

//var installer2 = OptifineInstaller.Create("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft", "C:\\Program Files\\Java\\latest\\jre-1.8\\bin\\java.exe", entry2);
//installer2.ProgressChanged += (_, arg) =>
//    Console.WriteLine($"{arg.StepName} - {arg.FinishedStepTaskCount}/{arg.TotalStepTaskCount} - {(arg.IsStepSupportSpeed ? $"{FileDownloader.GetSpeedText(arg.Speed)} - {arg.Progress * 100:0.00}%" : $"{arg.Progress * 100:0.00}%")}");

//var minecraft2 = await installer2.InstallAsync();
//Console.WriteLine(minecraft2.Id);

#endregion

#region Fabric 安装器

//var entry3 = await FabricInstaller.EnumerableFabricAsync("1.20.1")
//    .FirstOrDefaultAsync();

//var installer3 = FabricInstaller.Create("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft", entry3);
//installer3.ProgressChanged += (_, arg) =>
//    Console.WriteLine($"{arg.StepName} - {arg.FinishedStepTaskCount}/{arg.TotalStepTaskCount} - {(arg.IsStepSupportSpeed ? $"{FileDownloader.GetSpeedText(arg.Speed)} - {arg.Progress * 100:0.00}%" : $"{arg.Progress * 100:0.00}%")}");

//var minecraft3 = await installer3.InstallAsync();
//Console.WriteLine(minecraft3.Id);

#endregion

#region Quilt 安装器

//var entry4 = await QuiltInstaller.EnumerableQuiltAsync("1.20.1")
//    .FirstOrDefaultAsync();

//var installer4 = QuiltInstaller.Create("C:\\Users\\wxysd\\Desktop\\temp\\.minecraft", entry4);
//installer4.ProgressChanged += (_, arg) =>
//    Console.WriteLine($"{arg.StepName} - {arg.FinishedStepTaskCount}/{arg.TotalStepTaskCount} - {(arg.IsStepSupportSpeed ? $"{FileDownloader.GetSpeedText(arg.Speed)} - {arg.Progress * 100:0.00}%" : $"{arg.Progress * 100:0.00}%")}");

//var minecraft4 = await installer4.InstallAsync();
//Console.WriteLine(minecraft4.Id);

#endregion

#region 微软验证

//MicrosoftAuthenticator authenticator = new("Your client ID");
//var oAuth2Token = await authenticator.DeviceFlowAuthAsync(x => {
//    Console.WriteLine(x.UserCode);
//    Console.WriteLine(x.VerificationUrl);
//});

//var account = await authenticator.AuthenticateAsync(oAuth2Token);
//Console.WriteLine(account.Name);
//Console.WriteLine();

//var newAccount = await authenticator.RefreshAsync(account);
//Console.WriteLine(newAccount.Name);

#endregion

#region 第三方验证

//YggdrasilAuthenticator authenticator = new("https://littleskin.cn/api/yggdrasil", "Wxysdsb123@163.com", "wxysdsb12");
//var result = authenticator.AuthenticateAsync();
//await foreach (var item in result)
//    Console.WriteLine(item.Name);

//var newResult = await authenticator.RefreshAsync(await result.FirstAsync());
//Console.WriteLine(newResult.Name);

#endregion

#region 本地游戏读取

MinecraftParser minecraftParser = "C:\\Users\\wxysd\\Desktop\\temp\\.minecraft";

//minecraftParser.GetMinecrafts().ForEach(x => {
//    Console.WriteLine(x.Id);
//    Console.WriteLine($"是否为原版：{x.IsVanilla}");

//    if (!x.IsVanilla) {
//        Console.WriteLine("Mod 加载器：" + string.Join("，", (x as ModifiedMinecraftEntry)?.ModLoaders.Select(x => $"{x.Type}_{x.Version}")!));
//    }

//    Console.WriteLine();
//});

#endregion

#region NBT 文件操作

//var minecraft = minecraftParser.GetMinecraft("1.12.2");
//var save = await minecraft.GetNBTParser().ParseSaveAsync("New World");
//Console.WriteLine($"存档名：{save.LevelName}");
//Console.WriteLine($"种子：{save.Seed}");
//Console.WriteLine($"游戏模式：{save.GameType}");
//Console.WriteLine($"版本：{save.Version}");

#endregion

#region 启动

//MinecraftRunner runner = new(new LaunchConfig {
//    Account = new OfflineAuthenticator().Authenticate("Yang114"),
//    JavaPath = "C:\\Users\\wxysd\\AppData\\Roaming\\.minecraft\\runtime\\java-runtime-delta\\bin\\javaw.exe",
//    MaxMemorySize = 1024,
//    MinMemorySize = 512,
//}, minecraftParser);

//var process = await runner.RunAsync(minecraft4.Id);
//process.Started += (_, _) => Console.WriteLine("Launch successful!");
//process.OutputLogReceived += (_, arg) => Console.WriteLine(arg.Data);

#endregion

Console.ReadKey();