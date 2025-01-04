using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Event;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Extensions;
using System.Data;

namespace MinecraftLaunch.Components.Installer;

/// <summary>
/// 复合安装器
/// </summary>
public sealed class CompositionInstaller : InstallerBase {
    private readonly string _customId;
    private readonly InstallerBase _subInstaller;
    private readonly InstallerBase _mainInstaller;
    private readonly OptiFineInstallEntity _entity;

    /// <summary>
    /// 自定义下载进度计算表达式
    /// </summary>
    public override Func<double, double> CalculateExpression { get; set; }
        = x => x.ToPercentage(0.0d, 0.6d);

    public event EventHandler SubInstallerCompleted;

    public override GameEntry InheritedFrom { get; set; }

    public CompositionInstaller(InstallerBase installerBase, string customId, OptiFineInstallEntity entity = default) {
        if (installerBase is NeoForgeInstaller or QuiltInstaller) {
            throw new ArgumentException("选择的安装器类型不支持复合安装");
        }

        _entity = entity;
        _customId = customId;
        _mainInstaller = installerBase;

        InheritedFrom = _mainInstaller.InheritedFrom;
    }

    public CompositionInstaller(InstallerBase mainInstaller, InstallerBase subInstaller, string customId, OptiFineInstallEntity entity = default) {
        if (mainInstaller is OptifineInstaller || subInstaller is OptifineInstaller) {
            throw new ArgumentException("选择的安装器类型不支持复合安装");
        }

        _entity = entity;
        _customId = customId;
        _subInstaller = subInstaller;
        _mainInstaller = mainInstaller;
    }

    public override async Task<bool> InstallAsync(CancellationToken cancellation = default) {
        _mainInstaller.ProgressChanged += OnProgressChanged;
        await _mainInstaller.InstallAsync(cancellation);

        SubInstallerCompleted?.Invoke(this, default);
        if (_entity is null && _subInstaller is null) {
            CalculateExpression = null;
            ReportProgress(1.0d, "Installation is complete", TaskStatus.RanToCompletion);
            ReportCompleted();
            return true;
        }

        ReportProgress(0.6d, "Start installing the sub loader", TaskStatus.WaitingToRun);
        //sub1
        if (_subInstaller is not null) {
            //handle gameEntry
            if (_mainInstaller is VanlliaInstaller) {
                _subInstaller.InheritedFrom = _mainInstaller.InheritedFrom;
            }

            CalculateExpression = x => x.ToPercentage(0.6d, 0.8d);
            _subInstaller.ProgressChanged += OnProgressChanged;
            await _subInstaller.InstallAsync(cancellation);
        }

        //sub1 end
        if (_entity is null) {
            CalculateExpression = null;
            ReportProgress(1.0d, "Installation is complete", TaskStatus.RanToCompletion);
            ReportCompleted();
            return true;
        }

        //sub2
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
        ReportProgress(e.Progress, e.ProgressStatus, e.Status, e.Speed);
    }
}