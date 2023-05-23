using MinecraftLaunch.Modules.Models;
using MinecraftLaunch.Modules.Models.Auth;
using MinecraftLaunch.Modules.Toolkits;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace MinecraftLaunch.Modules.Authenticator
{
    /// <summary>
    /// 第三方验证器
    /// </summary>
    public partial class YggdrasilAuthenticator : AuthenticatorBase
    {
        /// <summary>
        /// 身份验证方法
        /// </summary>
        /// <returns></returns>
        public new IEnumerable<YggdrasilAccount> Auth() => AuthAsync().Result;

        /// <summary>
        /// 异步身份验证方法 
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public async ValueTask<IEnumerable<YggdrasilAccount>> AuthAsync()
        {
            var ru = Uri;
            string content = string.Empty;
            var requestJson = new
            {
                clientToken = Guid.NewGuid().ToString("N"),
                username = Email,
                password = Password,
                requestUser = false,
                agent = new
                {
                    name = "Minecraft",
                    version = 1
                }
            }.ToJson();
            List<YggdrasilAccount> accounts = new();
            var res = await HttpWrapper.HttpPostAsync($"{Uri}{(string.IsNullOrEmpty(Uri) ? "https://authserver.mojang.com" : "/authserver")}/authenticate", requestJson);
            content = await res.Content.ReadAsStringAsync();

            ClientToken = content.ToJsonEntity<YggdrasilResponse>().ClientToken;
            foreach (var i in content.ToJsonEntity<YggdrasilResponse>().UserAccounts)
                accounts.Add(new YggdrasilAccount()
                {
                    AccessToken = content.ToJsonEntity<YggdrasilResponse>().AccessToken,
                    ClientToken = content.ToJsonEntity<YggdrasilResponse>().ClientToken,
                    Name = i.Name,
                    Uuid = Guid.Parse(i.Uuid),
                    YggdrasilServerUrl = this.Uri,
                    Email = this.Email,
                    Password = this.Password
                });
            return accounts;
        }

        /// <summary>
        /// 异步刷新验证方法
        /// </summary>
        /// <returns></returns>
        public async ValueTask<YggdrasilAccount> RefreshAsync(YggdrasilAccount selectProfile)
        {
            var content = new
            {
                clientToken = selectProfile.ClientToken,
                accessToken = selectProfile.AccessToken,
                requestUser = true
            }.ToJson();

            using var res = await HttpWrapper.HttpPostAsync($"{Uri}{(string.IsNullOrEmpty(Uri) ? "https://authserver.mojang.com" : "/authserver")}/refresh", content);
            string result = await res.Content.ReadAsStringAsync();
            await Console.Out.WriteLineAsync(result);
            res.EnsureSuccessStatusCode();
            var responses = result.ToJsonEntity<YggdrasilResponse>().UserAccounts;

            foreach (var i in responses)
            {
                if (i.Uuid.Equals(selectProfile.Uuid.ToString().Replace("-", ""))) {
                    return new()
                    {
                        AccessToken = result.ToJsonEntity<YggdrasilResponse>().AccessToken,
                        ClientToken = result.ToJsonEntity<YggdrasilResponse>().ClientToken,
                        Name = i.Name,
                        Uuid = Guid.Parse(i.Uuid),
                        YggdrasilServerUrl = this.Uri!,
                        Email = this.Email!,
                        Password = this.Password!
                    };
                }
            }
            
            return null!;//执行到此处的原因可能是此角色已删除的原因导致的
        }

        /// <summary>
        /// 异步登出方法
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> SignoutAsync()
        {
            string content = JsonConvert.SerializeObject(
                new
                {
                    username = Email,
                    password = Password
                }
            );
            
            using var res = await HttpWrapper.HttpPostAsync($"{Uri}{(string.IsNullOrEmpty(Uri) ? "https://authserver.mojang.com" : "/authserver")}/signout", content);
            
            return res.IsSuccessStatusCode;
        }

        /// <summary>
        /// 异步验证方法
        /// </summary>
        /// <param name="accesstoken"></param>
        /// <returns></returns>
        public async ValueTask<bool> ValidateAsync(string accesstoken)
        {
            var content = new
            {
                clientToken = ClientToken,
                accesstoken = accesstoken
            }.ToJson();
            using var res = await HttpWrapper.HttpPostAsync($"{Uri}/authserver/validate", content);
            return res.IsSuccessStatusCode;
        }
    }

    partial class YggdrasilAuthenticator
    {
        public YggdrasilAuthenticator() { }

        /// <summary>
        /// 标准第三方验证器构造方法
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public YggdrasilAuthenticator(string uri, string email, string password)
        {
            Uri = uri;
            Email = email;
            Password = password;
        }

        /// <summary>
        /// LittleSkin验证器接口
        /// </summary>
        /// <param name="IsLittleSkin"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public YggdrasilAuthenticator(bool IsLittleSkin, string email, string password)
        {
            if (IsLittleSkin)
                Uri = "https://littleskin.cn/api/yggdrasil";
            Email = email;
            Password = password;
        }

        /// <summary>
        /// Mojang验证构造器（已弃用）
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>                
        [Obsolete] public YggdrasilAuthenticator(string email, string password) { }
    }

    partial class YggdrasilAuthenticator
    {
        public string? Uri { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string ClientToken { get; set; }
        //public string AccessToken { get; set; } = string.Empty;
    }

    internal class Model
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
    }
}