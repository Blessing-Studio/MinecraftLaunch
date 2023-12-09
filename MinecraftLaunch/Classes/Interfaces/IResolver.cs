using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Classes.Interfaces {
    public interface IResolver {}

    public interface IResolver<T> : IResolver {
        T Resolve(string str);
    }

    public interface IResolver<TReturn, TParameter> : IResolver {
        TReturn Resolve(TParameter parameter);
    }
}
