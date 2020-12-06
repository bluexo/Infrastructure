using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;

using UnityEngine;

namespace Origine
{
    public partial class GameEntity
    {
        protected readonly CompositeDisposable compositeDisposables = new CompositeDisposable();

        public IDisposable RegisterTimer(float durTime, float period, Action<long> action)
        {
            var disposable = Observable
                .Timer(TimeSpan.FromSeconds(durTime), TimeSpan.FromSeconds(period))
                .SubscribeOnMainThread()
                .Subscribe(action).AddTo(compositeDisposables);
            return disposable;
        }

        public IDisposable RegisterDelay(float tick, Action<long> action)
        {
            var disposable = Observable
                .Timer(TimeSpan.FromSeconds(tick))
                .SubscribeOnMainThread()
                .Subscribe(action).AddTo(compositeDisposables);
            return disposable;
        }
    }
}