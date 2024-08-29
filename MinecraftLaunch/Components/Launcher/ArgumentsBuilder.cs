using MinecraftLaunch.Extensions;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Resolver.Arguments;
using System.Collections.Immutable;

#if NET8_0

using System.Collections.Frozen;

#endif

namespace MinecraftLaunch.Components.Launcher;

internal sealed class ArgumentsBuilder(GameEntry _gameEntity, LaunchConfig _launchConfig) {
    public IEnumerable<string> Build() {
        var jvmArguments = JvmArgumentResolver.Resolve(_gameEntity.GameJsonEntry);
        var gameArguments = GameArgumentResolver.Resolve(_gameEntity.GameJsonEntry);

        //合并父级核心参数
        if (_gameEntity.IsInheritedFrom) {
            jvmArguments = JvmArgumentResolver
                .Resolve(_gameEntity.InheritsFrom.GameJsonEntry)
                .Union(jvmArguments);

            gameArguments = GameArgumentResolver
                .Resolve(_gameEntity.InheritsFrom.GameJsonEntry)
                .Union(gameArguments);
        }

        var jvmArgumentsReplace = new Dictionary<string, string>()
        {
            { "${launcher_version}", "4" },
            { "${launcher_name}", "MinecraftLaunch" },

            { "${classpath}", GetClasspath().ToPath() },
            { "${natives_directory}", GetNativesDirectory() },
            { "${classpath_separator}", Path.PathSeparator.ToString() },
            { "${library_directory}", _gameEntity.ToLibrariesPath().ToPath() },
            {
                "${version_name}", _gameEntity.IsInheritedFrom
                ? _gameEntity.InheritsFrom.Id
                : _gameEntity.Id
            },
        };

        var gameArgumentsReplace = new Dictionary<string, string>() {
                { "${user_properties}", "{}" },
                { "${auth_player_name}", _launchConfig.Account.Name },
                { "${auth_session}", _launchConfig.Account.AccessToken },
                { "${auth_uuid}", _launchConfig.Account.Uuid.ToString("N") },
                { "${auth_access_token}", _launchConfig.Account.AccessToken },
                { "${user_type}", _launchConfig.Account.Type.Equals(AccountType.Microsoft) ? "MSA" : "Mojang" },

                { "${game_assets}", Path.Combine(_gameEntity.GameFolderPath, "assets").ToPath() },
                { "${assets_root}", Path.Combine(_gameEntity.GameFolderPath, "assets").ToPath() },
                { "${assets_index_name}", Path.GetFileNameWithoutExtension(_gameEntity.AssetsIndexJsonPath) },
                { "${game_directory}", _gameEntity.ToVersionDirectoryPath(_launchConfig.IsEnableIndependencyCore).ToPath() },

                { "${version_name}", _gameEntity.Id.ToPath() },
                { "${version_type}", _launchConfig.LauncherName.ToPath() },
            };

#if NET8_0
        var fastJvmParametersReplace = jvmArgumentsReplace.ToFrozenDictionary();
        var fastGameParametersReplace = gameArgumentsReplace.ToFrozenDictionary();
#else
        var fastJvmParametersReplace = jvmArgumentsReplace.ToImmutableDictionary();
        var fastGameParametersReplace = gameArgumentsReplace.ToImmutableDictionary();
#endif

        yield return $"-Dlog4j2.formatMsgNoLookups=true";
        yield return $"-Xms{_launchConfig.JvmConfig.MinMemory}M";
        yield return $"-Xmx{_launchConfig.JvmConfig.MaxMemory}M";
        yield return $"-Dminecraft.client.jar={_gameEntity.JarPath.ToPath()}";

        foreach (var arg in JvmArgumentResolver.GetEnvironmentJVMArguments()) {
            yield return arg;
        }

        foreach (var arg in jvmArguments) {
            yield return arg.Replace(fastJvmParametersReplace);
        }

        yield return _gameEntity.MainClass;

        foreach (var arg in gameArguments) {
            yield return arg.Replace(fastGameParametersReplace);
        }

        //Custom args
        if (_launchConfig.GameWindowConfig != null) {
            yield return $"--width {_launchConfig.GameWindowConfig.Width}";
            yield return $"--height {_launchConfig.GameWindowConfig.Height}";

            if (_launchConfig.GameWindowConfig.IsFullscreen) yield return "--fullscreen";
        }

        if (_launchConfig.ServerConfig != null
            && _launchConfig.ServerConfig.Port != 0
            && !string.IsNullOrEmpty(_launchConfig.ServerConfig.Ip)) {
            yield return $"--server {_launchConfig.ServerConfig.Ip}";
            yield return $"--port {_launchConfig.ServerConfig.Port}";
        }

    }

    private string GetClasspath() {
        var libraries = new LibrariesResolver(_gameEntity)
            .GetLibraries()
            .Where(x => x is not null)
            .ToImmutableArray();

        var classPath = string.Join(Path.PathSeparator,
            libraries.Select(lib => lib?.Path));

        if (!string.IsNullOrEmpty(_gameEntity.JarPath)) {
            classPath += $"{Path.PathSeparator}{_gameEntity.JarPath}";
        }

        return classPath;
    }

    private string GetNativesDirectory() {
        return _launchConfig.NativesFolder != null && _launchConfig.NativesFolder.Exists
            ? _launchConfig.NativesFolder.FullName.ToString()
            : Path.Combine(_gameEntity.ToVersionDirectoryPath(_launchConfig.IsEnableIndependencyCore), "natives").ToPath();
    }
}