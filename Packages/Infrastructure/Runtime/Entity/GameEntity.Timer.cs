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
        public void RegisterTimerUntilDestroy(float durTime, float period, Action<long> action)
        {
            Observable
               .Timer(TimeSpan.FromSeconds(durTime), TimeSpan.FromSeconds(period))
               .SubscribeOnMainThread()
               .Subscribe(action).AddTo(Self);
        }

        public void RegisterDelayUntilDestroy(float tick, Action<long> action)
        {
            Observable
               .Timer(TimeSpan.FromSeconds(tick))
               .SubscribeOnMainThread()
               .Subscribe(action).AddTo(Self);
        }
    }
}