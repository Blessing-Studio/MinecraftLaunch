namespace MinecraftLaunch.Utilities {
    public class DirectoryUtil {
        public static void DeleteAllFiles(string folder) =>
            DeleteAllFiles(new DirectoryInfo(folder));

        public static void DeleteAllFiles(DirectoryInfo directory) {
            foreach (FileInfo file in directory.EnumerateFiles()) {
                file.Delete();
            }

            foreach (var item in directory.EnumerateDirectories()) {
                DeleteAllFiles(item);
                item.Delete();
            }
        }

        public static IEnumerable<FileInfo> FindAll(DirectoryInfo directory, string file) {
            foreach (var item in directory.EnumerateFiles()) {
                if (item.Name == file) {
                    yield return item;
                }
            }

            foreach (var item in directory.EnumerateDirectories()) {
                foreach (var info in FindAll(item, file)) {
                    yield return info;
                }
            }
        }

    }
}
