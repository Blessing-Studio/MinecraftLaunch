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
    }
}
