﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Auth
{
    public class XBLAuthenticateRequestModel
    {
        [JsonProperty("Properties")]
        public XBLAuthenticatePropertiesModel Properties { get; set; } = new XBLAuthenticatePropertiesModel();

        [JsonProperty("RelyingParty")]
        public string RelyingParty { get; set; } = "http://auth.xboxlive.com";

        [JsonProperty("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }

    public class XBLAuthenticateResponseModel
    {
        [JsonProperty("IssueInstant")]
        public string IssueInstant { get; set; }

        [JsonProperty("NotAfter")]
        public string NotAfter { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("DisplayClaims")]
        public DisplayClaimsModel DisplayClaims { get; set; }
    }

    public class XBLAuthenticatePropertiesModel
    {
        [JsonProperty("AuthMethod")]
        public string AuthMethod { get; set; } = "RPS";

        [JsonProperty("SiteName")]
        public string SiteName { get; set; } = "user.auth.xboxlive.com";

        [JsonProperty("RpsTicket")]
        public string RpsTicket { get; set; } = "d=<access token>";
    }
    public class DisplayClaimsModel
    {
        [JsonProperty("xui")]
        public List<JObject> Xui { get; set; }
    }
}
