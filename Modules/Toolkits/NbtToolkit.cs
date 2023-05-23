using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NbtLib;

namespace MinecraftLaunch.Modules.Toolkits {
    public class NbtToolkit {   
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
