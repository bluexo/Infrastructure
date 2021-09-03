
using System;
using System.Diagnostics;

namespace Love.Network
{
    /// <summary>
    /// 计时器
    /// </summary>
    public class NetworkTimer
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public long UnscaledTime => stopwatch.ElapsedMilliseconds;

        public void Start() => stopwatch.Start();
        public void Stop() => stopwatch.Stop();
    }
}