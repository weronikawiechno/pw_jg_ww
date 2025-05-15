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
            
            _dataLayer.Start(numberOfBalls, (vector, dataBall) =>
            {
                var businessBall = new BusinessBall(dataBall);
                
                IPosition position = new Position(vector.x, vector.y);
                
                handler(position, businessBall);
                
                var thread = new Thread(() => BallThreadLoop(businessBall, _cts.Token));
                _threads.Add(thread);
                thread.Start();
            });
        }

        private void BallThreadLoop(IBall ball, CancellationToken token)
        {
            Data.IBall dataBall = ((BusinessBall)ball).GetDataBall();
            
            while (!token.IsCancellationRequested)
            {
                _dataLayer.MoveBall(dataBall);
                
                Thread.Sleep(20);
            }
        }

        public override void Stop()
        {
            _cts.Cancel();
            
            // Wait for all threads to finish
            foreach (var thread in _threads)
            {
                thread.Join();
            }
            _threads.Clear();
            
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            
            _isRunning = false;
            
            // Tell the data layer to stop
            _dataLayer.Stop();
        }

        // Dispose method
        public override void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
}