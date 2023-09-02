using MinecraftLaunch.Launch;
using MinecraftLaunch.Modules.Models.Auth;
using MinecraftLaunch.Modules.Models.Launch;
using MinecraftLaunch.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Extensions.Game {
    public static class LaunchExtension {
        public static ExtensionLauncher SetJava(this GameCore core, string javaPath) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    JvmConfig = new JvmConfig {
                        JavaPath = new(javaPath)
                    }
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetAccount(this GameCore core, Account account) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    Account = account
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetMemory(this GameCore core, int maxMemory) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    JvmConfig = new JvmConfig {
                        MaxMemory = maxMemory
                    }
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetChinese(this GameCore core, bool isChinese) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    IsChinese = isChinese
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetLauncherName(this GameCore core, string launcherName) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    LauncherName = launcherName
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetWindowHeight(this GameCore core, int height) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    GameWindowConfig = new GameWindowConfig {
                        Height = height
                    }
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetWindowWidth(this GameCore core, int width) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    GameWindowConfig = new GameWindowConfig {
                        Width = width
                    }
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetIndependencyCore(this GameCore core, bool isEnableIndependencyCore) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    IsEnableIndependencyCore = isEnableIndependencyCore
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetAdvancedArguments(this GameCore core, IEnumerable<string> args) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    JvmConfig = new JvmConfig {
                        AdvancedArguments = args
                    }
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetGCArguments(this GameCore core, IEnumerable<string> args) {
            var launcher = new ExtensionLauncher {
                LaunchConfig = new LaunchConfig {
                    JvmConfig = new JvmConfig {
                        GCArguments = args,
                        UsedGC = true
                    }
                }
            };
            launcher.GameCore = core;

            return launcher;
        }

        public static ExtensionLauncher SetJava(this ExtensionLauncher launcher, string javaPath) {
            if (launcher.LaunchConfig.JvmConfig is null) {
                launcher.LaunchConfig.JvmConfig = new(javaPath);
            } else {
                launcher.LaunchConfig.JvmConfig.JavaPath = new(javaPath);
            }

            return launcher;
        }

        public static ExtensionLauncher SetAccount(this ExtensionLauncher launcher, Account account) {
            launcher.LaunchConfig.Account = account;
            return launcher;
        }

        public static ExtensionLauncher SetMemory(this ExtensionLauncher launcher, int maxMemory) {
            if (launcher.LaunchConfig.JvmConfig is null) {
                launcher.LaunchConfig.JvmConfig = new JvmConfig { 
                    MaxMemory = maxMemory
                };
            } else {
                launcher.LaunchConfig.JvmConfig.MaxMemory = maxMemory;
            }

            return launcher;
        }

        public static ExtensionLauncher SetChinese(this ExtensionLauncher launcher, bool isChinese) {
            launcher.LaunchConfig.IsChinese = isChinese;

            return launcher;
        }

        public static ExtensionLauncher SetLauncherName(this ExtensionLauncher launcher, string launcherName) {
            launcher.LaunchConfig.LauncherName = launcherName;

            return launcher;
        }

        public static ExtensionLauncher SetWindowHeight(this ExtensionLauncher launcher, int height) {
            if (launcher.LaunchConfig.JvmConfig is null) {
                launcher.LaunchConfig.GameWindowConfig = new GameWindowConfig {
                    Height = height
                };
            } else {
                launcher.LaunchConfig.GameWindowConfig.Height = height;
            }

            return launcher;
        }

        public static ExtensionLauncher SetWindowWidth(this ExtensionLauncher launcher, int width) {
            if (launcher.LaunchConfig.JvmConfig is null) {
                launcher.LaunchConfig.GameWindowConfig = new GameWindowConfig {
                    Width = width
                };
            } else {
                launcher.LaunchConfig.GameWindowConfig.Width = width;
            }

            return launcher;
        }

        public static ExtensionLauncher SetIndependencyCore(this ExtensionLauncher launcher, bool isEnableIndependencyCore) {
            launcher.LaunchConfig.IsEnableIndependencyCore = isEnableIndependencyCore;

            return launcher;
        }

        public static ExtensionLauncher SetAdvancedArguments(this ExtensionLauncher launcher, IEnumerable<string> args) {
            if (launcher.LaunchConfig.JvmConfig is null) {
                launcher.LaunchConfig.JvmConfig = new JvmConfig {
                    AdvancedArguments = args
                };
            } else {
                launcher.LaunchConfig.JvmConfig.AdvancedArguments = args;
            }

            return launcher;
        }

        public static ExtensionLauncher SetGCArguments(this ExtensionLauncher launcher, IEnumerable<string> args) {
            if (launcher.LaunchConfig.JvmConfig is null) {
                launcher.LaunchConfig.JvmConfig = new JvmConfig {
                    GCArguments = args,
                    UsedGC = true
                };
            } else {
                launcher.LaunchConfig.JvmConfig.GCArguments = args;
            }

            return launcher;
        }

        public static ValueTask<MinecraftLaunchResponse> LaunchTaskAsync(this ExtensionLauncher launcher) {
            JavaMinecraftLauncher mcLauncher = new(launcher.LaunchConfig, launcher.GameCore);
            return mcLauncher.LaunchTaskAsync(launcher.GameCore.Id!);
        }

        public static ValueTask<MinecraftLaunchResponse> LaunchTaskAsync(this ExtensionLauncher launcher, Action<(float, string)> action) {
            JavaMinecraftLauncher mcLauncher = new(launcher.LaunchConfig, launcher.GameCore);
            return mcLauncher.LaunchTaskAsync(launcher.GameCore.Id!, action);
        }
    }

    public class ExtensionLauncher {
        public GameCore GameCore { get; set; }

        public LaunchConfig LaunchConfig { get; set; }
    }
}
