using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Extensions;

namespace MinecraftLaunch.Components.Installer;

/// <summary>
/// 复合安装器
/// </summary>
public sealed class CompositionInstaller : InstallerBase {
    private readonly string _customId;
    private readonly InstallerBase _installerBase;
    private readonly OptiFineInstallEntity _entity;

    /// <summary>
    /// 自定义下载进度计算表达式
    /// </summary>
    public override Func<double, double> CalculateExpression { get; set; } = x => x.ToPercentage(0.0d, 0.8d);

    public override GameEntry InheritedFrom { get; }

    public CompositionInstaller(InstallerBase installerBase, string customId, OptiFineInstallEntity entity = default) {
        if (installerBase is NeoForgeInstaller or QuiltInstaller) {
            throw new ArgumentException("选择的安装器类型不支持复合安装");
        }

        _entity = entity;
        _customId = customId;
        _installerBase = installerBase;

        InheritedFrom = _installerBase.InheritedFrom;
    }

    public override async Task<bool> InstallAsync(CancellationToken cancellation = default) {
        _installerBase.ProgressChanged += OnProgressChanged;
        await _installerBase.InstallAsync();

        if (_entity is null) {
            CalculateExpression = null;
            ReportProgress(1.0d, "Installation is complete", TaskStatus.RanToCompletion);
            ReportCompleted();
            return true;
        }

        ReportProgress(0.8d, "Start installing the sub loader", TaskStatus.WaitingToRun);

        string downloadUrl = $"https://bmclapi2.bangbang93.com/optifine/{_entity.McVersion}/{_entity.Type}/{_entity.Patch}";
        string packagePath = Path.Combine(Path.Combine(InheritedFrom.GameFolderPath, "versions", _customId, "mods"), _entity.FileName);
        var request = downloadUrl.ToDownloadRequest(packagePath.ToFileInfo());
        CalculateExpression = x => x.ToPercentage(0.8d, 1.0d);

        var result = await request.DownloadAsync(x => {
            ReportProgress(x,
                "Downloading Optifine installation package",
                TaskStatus.Running);
        }, cancellation);

        ReportCompleted();

        if (result.Type is DownloadResultType.Successful) {
            ReportProgress(1.0d, "Installation is complete", TaskStatus.RanToCompletion);
            return true;
        }

        ReportProgress(1.0d, "Installation is complete", TaskStatus.Faulted);
        return false;
    }

    private void OnCompleted(object sender, EventArgs e) {
        ReportCompleted();
    }

    private void OnProgressChanged(object sender, ProgressChangedEventArgs e) {
        ReportProgress(e.Progress, e.ProgressStatus, e.Status);
    }
}