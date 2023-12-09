using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Models.Exceptions {
    public class GameResolveFailedException : Exception {
        public GameResolveFailedException(string message) : base(message) { 
        }
    }
}
