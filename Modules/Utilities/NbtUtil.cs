using NbtLib;

namespace MinecraftLaunch.Modules.Utilities {
    public class NbtUtil {   
        /// <summary>
        /// Load the Nbt file
        /// </summary>
        /// <param name="path"></param>
        public static NbtCompoundTag Load(string path) {
            Stream stream = null;            
            if (File.Exists(path)) {
                return NbtConvert.ParseNbtStream(File.OpenRead(path));
            }

            return null;
        }        
    }
}
