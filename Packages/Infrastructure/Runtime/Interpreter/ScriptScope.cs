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
    internal class ScriptScope : IScope
    {
        public string Name { get; private set; }
        private readonly Engine _engine;

        public ScriptScope(string name, Engine engine)
        {
            Name = name;
            _engine = engine;
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

        public void OnDispose()
        {
            _engine.ResetCallStack();
            _engine.ResetMemoryUsage();
            _engine.ResetStatementsCount();
            _engine.ResetTimeoutTicks();
        }
    }
}