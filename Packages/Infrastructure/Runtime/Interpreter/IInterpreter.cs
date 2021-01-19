using Jint;
using System.Reflection;

namespace Origine
{
    /// <summary>
    /// 脚本解释器
    /// </summary>
    public interface IInterpreter : IScriptScope
    {
        void SetClrAssemblies(params Assembly[] assemblies);

        IScriptScope GetOrCreate(string scopeName);

        void Release(string scope);
    }
}