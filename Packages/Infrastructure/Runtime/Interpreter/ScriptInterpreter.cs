using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Jint;
using Esprima;
using System;
using System.Reflection;
using UnityEditor;
using System.Linq;

namespace Origine
{
    /// <summary>
    /// JS 脚本解释器
    /// </summary>
    internal class ScriptInterpreter : GameModule, IInterpreter
    {
        readonly Dictionary<string, IScriptScope> scopes = new Dictionary<string, IScriptScope>();
        readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        readonly GameContext _gameContext;

        public IScriptScope Global { get; private set; }

        public ScriptInterpreter(GameContext gameContext)
        {
            _gameContext = gameContext;
            Global = GetOrCreate(nameof(Global));
        }

        public IScriptScope GetOrCreate(string scopeName)
        {
            if (!scopes.ContainsKey(scopeName))
            {
                var engine = new Engine(options =>
                {
                    options.AllowClr(AssemblyCollection.GetAssemblies());
                    options.LimitRecursion(sbyte.MaxValue);
                });
                scopes[scopeName] = new ScriptScope(scopeName, engine);
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