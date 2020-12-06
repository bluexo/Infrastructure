using System;
using System.Collections;
using System.Runtime.CompilerServices;

using UniRx;

using UnityEngine;

namespace Origine
{
    public abstract class GameEntityComponent<TEntity> : EntityComponent where TEntity : GameEntity
    {
        public TEntity Owner { get; private set; }
        public Transform Transform { get; private set; }

        protected CompositeDisposable disposables;

        public override void OnInit(Entity gameEntity)
        {
            base.OnInit(gameEntity);
            disposables = new CompositeDisposable();
            Owner = gameEntity as TEntity;
            if (Owner.Self) Transform = Owner.Self.transform;
        }

        protected Coroutine StartCoroutine(IEnumerator enumerator) => Owner.StartCoroutine(enumerator);

        protected void StartCoroutine(float delay, Action action) => StartCoroutine(InvokeAsync(delay, action));

        private IEnumerator InvokeAsync(float delay, Action action)
        {
            if (delay <= 0)
            {
                action?.Invoke();
                yield break;
            }
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        protected void StopCoroutine(Coroutine coroutine) => Owner.StopCoroutine(coroutine);

        public override void OnDestroy()
        {
            base.OnDestroy();
            disposables.Dispose();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}