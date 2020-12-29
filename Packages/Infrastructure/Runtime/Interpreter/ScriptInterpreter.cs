using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Origine
{
    /// <summary>
    /// JS 脚本解释器
    /// </summary>
    internal class ScriptInterpreter : GameModule, IInterpreter
    {
        readonly Dictionary<string, IScope> scopes = new Dictionary<string, IScope>();
        readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        readonly GameContext _gameContext;

        public IScope Global { get; private set; }

        public ScriptInterpreter(GameContext gameContext)
        {
            _gameContext = gameContext;
            Global = GetOrCreate(nameof(Global), AppDomain.CurrentDomain.GetAssemblies());
        }

        public IScope GetOrCreate(string scopeName, params Assembly[] asms)
        {
            if (!scopes.ContainsKey(scopeName))
            {
                var array = new List<Assembly>(_assemblies.Union(asms)).ToArray();
                var _engine = new Engine(options =>
                {
                    options.AllowClr(array);
                    options.LimitRecursion(sbyte.MaxValue);
                });
                scopes[scopeName] = new ScriptScope(scopeName, _engine);
            }
            return scopes[scopeName];
        }

        public void Release(string scope)
        {
            if (!scopes.ContainsKey(scope)) return;
            scopes[scope].OnDispose();
            scopes.Remove(scope);
        }

        public void SetValue(string key, object parameter) => Global.SetValue(key, parameter);

        public object Execute(string src) => Global.Execute(src);

        public bool TryExecute<T>(string src, out T value) => Global.TryExecute(src, out value);

        public object Execute(string src, params InterpreterContext[] contexts) => Global.Execute(src, contexts);

        public override void OnDispose()
        {
            base.OnDispose();
            foreach (var scope in scopes) scope.Value.OnDispose();
            scopes.Clear();
        }

        public void SetClrAssemblies(params Assembly[] assemblies)
        {
            _assemblies.UnionWith(assemblies);
        }
    }
}