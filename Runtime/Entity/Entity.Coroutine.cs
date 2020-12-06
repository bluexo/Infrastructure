using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;

using UnityEngine;

namespace Origine
{
    public partial class Entity
    {
        private readonly HashSet<Coroutine> _coroutines = new HashSet<Coroutine>();

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            var co = MainThreadDispatcher.StartCoroutine(enumerator);
            _coroutines.Add(co);
            return co;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            _coroutines.Remove(coroutine);
            MainThreadDispatcher.StopCoroutine(coroutine);
        }
    }
}
