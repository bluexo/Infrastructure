using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Jint;
using Esprima;
using System;

namespace Origine
{
    /// <summary>
    /// JS 脚本解释器
    /// </summary>
    internal class JavascriptInterpreter : GameModule, IInterpreter
    {
        readonly GameContext _gameContext;
        readonly Engine _engine;

        public JavascriptInterpreter(GameContext gameContext)
        {
            _gameContext = gameContext;

            _engine = new Engine(options =>
            {
                options.AllowClr(typeof(MonoBehaviour).Assembly, typeof(UnityEngine.EventSystems.UIBehaviour).Assembly);
                options.LimitRecursion(sbyte.MaxValue);
#if DEBUG
                options.DebugMode(true);
#endif
            });

            _engine.SetValue("TYPE", new Func<string, Type>(Utility.AssemblyCollection.GetType));
            _engine.SetValue("LOG", new Action<string>(Debug.Log));
            _engine.SetValue("ERR", new Action<string>(Debug.LogError));
        }

        public void SetValue(string key, object parameter) => _engine.SetValue(key, parameter);

        public object Execute(string src) => _engine.Execute(src)
                .GetCompletionValue()
                .ToObject();

        public bool TryExecute<T>(string src, out T value)
        {
            var v = _engine.Execute(src).GetCompletionValue().ToObject();

            if (v is T t)
            {
                value = t;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public object Execute(string src, params InterpreterContext[] contexts)
        {
            foreach (var ctx in contexts)
                _engine.SetValue(ctx.Name, ctx.Value);
            return Execute(src);
        }

        public override void OnDispose()
        {
            base.OnDispose();
        }
    }
}