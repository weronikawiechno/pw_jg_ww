using System;
using System.Timers;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class RealTimeManager
    {
        private readonly System.Timers.Timer _timer; //Timer
        private double _deltaTime;
        private double _elapsedTime;
        private DateTime _lastUpdateTime;
        private readonly double _targetFrameTime;
        private bool _running = false;

        public double DeltaTime => _deltaTime;
        public double ElapsedTime => _elapsedTime;

        public event Action? OnFrame;

        public RealTimeManager(double targetFPS = 60.0)
        {
            _targetFrameTime = 1.0 / targetFPS;
            _timer = new System.Timers.Timer(_targetFrameTime * 1000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
        }

        public void Start()
        {
            if (_running) return;
            _elapsedTime = 0;
            _lastUpdateTime = DateTime.Now;
            _timer.Start();
            _running = true;
        }

        public void Stop()
        {
            _timer.Stop();
            _running = false;
        }

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;
            _elapsedTime += _deltaTime;
            OnFrame?.Invoke();
        }

        public bool IsDeadlineMet(double maxProcessingTime)
        {
            return _deltaTime <= maxProcessingTime;
        }
    }
}
