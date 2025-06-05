using System;
using System.Diagnostics;
using System.Threading;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    // ETAP 3
    internal class RealTimeManager
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private DateTime _lastUpdateTime;
        private double _deltaTime;
        private readonly double _targetFrameTime;

        public double DeltaTime => _deltaTime;
        public double ElapsedTime => _stopwatch.Elapsed.TotalSeconds;

        public RealTimeManager(double targetFPS = 60.0)
        {
            _targetFrameTime = 1.0 / targetFPS;
        }

        public void Start()
        {
            _stopwatch.Start();
            _lastUpdateTime = DateTime.Now;
        }

        public void Update()
        {
            var currentTime = DateTime.Now;
            _deltaTime = (currentTime - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = currentTime;

            _deltaTime = Math.Min(_deltaTime, 0.033); 
        }

        public void WaitForNextFrame()
        {
            var frameTime = _deltaTime;
            if (frameTime < _targetFrameTime)
            {
                var waitTime = (int)((_targetFrameTime - frameTime) * 1000);
                if (waitTime > 0)
                    Thread.Sleep(waitTime);
            }
        }

        public bool IsDeadlineMet(double maxProcessingTime)
        {
            return _deltaTime <= maxProcessingTime;
        }
    }
}