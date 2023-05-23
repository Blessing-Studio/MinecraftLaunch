using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Auth
{
    public class JsonValueChangeResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public Authentication Authentication { get; set; } = new Authentication();

        /// <summary>
        /// 
        /// </summary>
        public WebAPI WebAPI { get; set; } = new WebAPI();
    }
}
