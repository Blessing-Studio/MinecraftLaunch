using MinecraftLaunch.Modules.Models.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models
{
    /// <summary>
    /// 验证器抽象基类
    /// </summary>
    public abstract class AuthenticatorBase
    {
        /// <summary>
        /// 验证方法
        /// </summary>
        /// <returns>游戏角色信息</returns>
        public virtual ValueTask<Account> AuthAsync(Action<string> func) { throw new Exception(); }
        /// <summary>
        /// 验证方法
        /// </summary>
        /// <returns>游戏角色信息列表</returns>
        public virtual Account Auth() { throw new Exception(); }
    }
}
