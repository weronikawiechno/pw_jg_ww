//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the Watch button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________
using System;
using System.Collections.Generic;
using System.Threading;
using TP.ConcurrentProgramming.Data;
using TP.ConcurrentProgramming.BusinessLogic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("PresentationView")]

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly DataAbstractAPI _dataLayer;
        private readonly List<Thread> _threads = new List<Thread>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isRunning = false;
        private DiagnosticsLogger _logger;
        private RealTimeManager _realTimeManager; 

        public BusinessLogicImplementation(DataAbstractAPI dataLayer)
        {
            _dataLayer = dataLayer;
            _logger = new DiagnosticsLogger();
            _realTimeManager = new RealTimeManager(50.0); 
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> handler)
        {
            if (_isRunning) return;
            _isRunning = true;

            _realTimeManager.Start(); // Uruchomienie Timera

            var balls = new List<Ball>();

            _dataLayer.Start(numberOfBalls, (vector, dataBall) =>
            {
                Ball ball = new Ball(dataBall);
                balls.Add(ball);

                IPosition position = new Position(vector.x, vector.y);
                handler(position, ball);

                var thread = new Thread(() => BallThreadLoop(ball, _cts.Token));
                _threads.Add(thread);
                thread.Start();
            });

            foreach (var ball in balls)
            {
                ball.SetOtherBalls(balls);
            }
        }

        private void BallThreadLoop(IBall ball, CancellationToken token)
        {
            Data.IBall dataBall = ((Ball)ball).DataBall;
            var localTimeManager = new RealTimeManager(50.0);
            double targetFrameTimeMs = 1000.0 / 50.0; // 50 FPS

            localTimeManager.Start();

            double lastElapsed = localTimeManager.ElapsedTime;

            while (!token.IsCancellationRequested)
            {
                double start = localTimeManager.ElapsedTime;

                double deltaTime = start - lastElapsed;
                lastElapsed = start;

                UpdateBallVelocityWithTime(dataBall, deltaTime);

                _dataLayer.MoveBall(dataBall);

                _logger.LogBallState((Ball)ball, localTimeManager.ElapsedTime);

                if (!localTimeManager.IsDeadlineMet(0.02))
                {
                    _logger.LogDeadlineMiss((Ball)ball, localTimeManager.DeltaTime, 0.02);
                }

                 Thread.Sleep((int)targetFrameTimeMs);
            }
        }

        // ETAP 3
        private void UpdateBallVelocityWithTime(Data.IBall dataBall, double deltaTime)
        {
            var currentVelocity = dataBall.Velocity;

            var newVelX = currentVelocity.x;
            var newVelY = currentVelocity.y;

            if (_realTimeManager.ElapsedTime % 2.0 < deltaTime * 2)
            {
                newVelX -= 0.7;
                newVelY -= 0.7;
            }

            var maxSpeed = 10.0;
            var currentSpeed = Math.Sqrt(newVelX * newVelX + newVelY * newVelY);
            if (currentSpeed > maxSpeed)
            {
                var scale = maxSpeed / currentSpeed;
                newVelX *= scale;
                newVelY *= scale;
            }

            dataBall.Velocity = new TP.ConcurrentProgramming.Data.Vector(newVelX, newVelY);
        }

        public override void Stop()
        {
            _cts.Cancel();
            
            foreach (var thread in _threads)
            {
                thread.Join();
            }
            _threads.Clear();
            
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            
            _isRunning = false;

            _dataLayer.Stop();
        }

        public override void Dispose()
        {
            Stop();
            _logger?.Dispose(); // Dispose logger
            _cts.Dispose();
        }
    }
}