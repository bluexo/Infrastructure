using Jint;
using System.Reflection;

namespace Origine
{
    /// <summary>
    /// 脚本解释器
    /// </summary>
    public interface IInterpreter : IScope
    {
        IScope Global { get; }

        void SetClrAssemblies(params Assembly[] assemblies);

        IScope GetOrCreate(string scopeName, params Assembly[] assemblies);

        void Release(string scope);
    }
}