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
                
                // Notify the business layer about the new ball
                upperLayerHandler(startingPosition, newBall);
            }
        }

        public override void MoveBall(IBall ball)
        {
            lock (_ballsLock)
            {
                // Cast to our internal ball type
                Ball internalBall = ball as Ball;
                if (internalBall == null) return;
                
                // Move the ball
                internalBall.Move();
                
                // Handle collisions
                HandleCollisions(internalBall);
            }
        }

        private void HandleCollisions(Ball ball)
        {
            // Ball-to-ball collisions
            foreach (var other in BallsList)
            {
                if (ball != other)
                {
                    ball.ResolveCollision(other);
                }
            }

            // Boundary collisions
            Vector position = ball.GetPosition();
            if (position.x < 0 || position.x > 400)
            {
                ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);
            }

            if (position.y < 0 || position.y > 400)
            {
                ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);
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