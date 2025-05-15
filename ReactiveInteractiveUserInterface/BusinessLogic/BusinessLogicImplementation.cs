//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________
using System;
using System.Collections.Generic;
using System.Threading;
using TP.ConcurrentProgramming.Data;
using TP.ConcurrentProgramming.BusinessLogic;
namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly DataAbstractAPI _dataLayer;
        private readonly List<Thread> _threads = new List<Thread>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isRunning = false;

        public BusinessLogicImplementation(DataAbstractAPI dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> handler)
        {
            if (_isRunning) return;
            _isRunning = true;

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

            while (!token.IsCancellationRequested)
            {
                _dataLayer.MoveBall(dataBall);

                Thread.Sleep(20);
            }
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
            _cts.Dispose();
        }
    }
}