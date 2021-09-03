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
    internal class ScriptScope : IScriptScope
    {
        public string Name { get; private set; }
        private readonly Engine _engine;

        public ScriptScope(string name, Engine engine)
        {
            Name = name;
            _engine = engine;
        }

        public void SetValue(string key, object parameter) => _engine.SetValue(key, parameter);

        public void Execute(string src) => _engine.Execute(src);

        public bool TryExecute<T>(string src, out T value)
        {
            value = default;

            try
            {
                value = (T)_engine.Evaluate(src).ToObject();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void OnDispose()
        {
            _engine.ResetCallStack();
            _engine.ResetConstraints();
        }
    }
}