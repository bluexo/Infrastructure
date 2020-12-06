using System;
using System.Collections;

namespace Origine
{
    public interface IAudioManager
    {
        IEnumerator InitializeAsync(string settingPath);
        TGroup GetGroup<TGroup>() where TGroup : AudioGroup;
        void Stop();
        void FadeIn(float duration = 1f, Action callback = null);
        void FadeOut(float duration = 1f, Action callback = null);
        void Pause();
        void Resume();
    }
}
