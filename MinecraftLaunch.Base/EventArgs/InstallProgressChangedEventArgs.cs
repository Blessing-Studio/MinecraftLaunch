using MinecraftLaunch.Base.Enums;

namespace MinecraftLaunch.Base.EventArgs;

public sealed class InstallProgressChangedEventArgs : System.EventArgs {
    public double Speed { get; set; }
    public required double Progress { get; set; }

    public int TotalStepTaskCount { get; set; }
    public int FinishedStepTaskCount { get; set; }

    public required TaskStatus Status { get; set; }
    public required InstallStep StepName { get; set; }
    public required bool IsStepSupportSpeed { get; set; }

    [Obsolete($"Replaced by {nameof(StepName)}")]
    public string ProgressStatus { get; set; }
}