namespace MinecraftLaunch.Extensions {
    public static class FileExtension {
        public static FileInfo DiveToFile(this DirectoryInfo parentDir, string childFileNameHierarchy) {
            var divePath = parentDir.FullName;
            if (childFileNameHierarchy.Contains('/')) {
                var split = childFileNameHierarchy.Split('/');
                divePath = split.Aggregate(divePath, (current, s) => current.AppendPath(s));
            } else {
                divePath = divePath.AppendPath(childFileNameHierarchy);
            }

            return new FileInfo(divePath);
        }

        public static string AppendPath(this string path, string toAppend) {
            return Path.Combine(path, toAppend);
        }

        public static FileInfo ToFileInfo(this string path) => new(path);

        public static DirectoryInfo ToDirectoryInfo(this string path) => new(path);

        public static IEnumerable<FileInfo> FindAll(this string directory, string file) {
            var directoryInfo = directory.ToDirectoryInfo();
            foreach (var item in directoryInfo.EnumerateFiles()) {
                if (item.Name == file) {
                    yield return item;
                }
            }

            foreach (var item in directoryInfo.EnumerateDirectories()) {
                foreach (var info in item.FullName.FindAll(file)) {
                    yield return info;
                }
            }
        }
    }
}
