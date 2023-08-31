using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Download {
    public class HttpDownloadResponse {
        public string Message { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public FileInfo FileInfo { get; set; }
    }
}
