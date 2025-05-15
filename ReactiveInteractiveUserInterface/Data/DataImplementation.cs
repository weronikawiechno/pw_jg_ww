using System;
using System.Collections.Generic;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private readonly object _ballsLock = new object();
        private readonly List<Ball> BallsList = new List<Ball>();
        private bool _isRunning = false;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (_isRunning) return;
            _isRunning = true;
            
            Random random = new Random();
            
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new Vector(
                    random.Next(30, 400 - 30),
                    random.Next(30, 400 - 30)
                );
                Vector velocity = new Vector(
                    (random.NextDouble() - 0.5) * 5,
                    (random.NextDouble() - 0.5) * 5
                );

                Ball newBall = new Ball(startingPosition, velocity);
                
                lock (_ballsLock)
                {
                    BallsList.Add(newBall);
                }
                
                
                upperLayerHandler(startingPosition, newBall);
            }
        }

        public override void MoveBall(IBall ball)
        {
            lock (_ballsLock)
            {
                if (ball is Ball internalBall)
                {
                    internalBall.Move();
                }
            }
        }

        public override void Stop()
        {
            lock (_ballsLock)
            {
                BallsList.Clear();
            }
            _isRunning = false;
        }

        public override void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}