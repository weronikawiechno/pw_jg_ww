using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private readonly object _ballsLock = new object(); // Synchronizacja wątkóws
        private readonly List<Thread> _threads = new List<Thread>();
        private readonly List<Ball> BallsList = new List<Ball>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
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

                var thread = new Thread(() => BallThreadLoop(newBall, _cts.Token));
                _threads.Add(thread);
                thread.Start();
            }
        }

        private void BallThreadLoop(Ball ball, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                lock (_ballsLock) // Synchronizacja dostępu do zasobów współdzielonych
                {
                    ball.Move();
                    HandleCollisions(ball);
                }
                Thread.Sleep(20); // Zabezpieczenie przed nadmiernym zużyciem CPU
            }
            Debug.WriteLine("Thread over");
        }

        private void HandleCollisions(Ball ball)
        {
            foreach (var other in BallsList)
            {
                if (ball != other)
                {
                    ball.ResolveCollision(other);
                }
            }

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
            _cts.Cancel(); // Zatrzymanie wątków

            // Czekaj na zakończenie wszystkich wątków
            foreach (var thread in _threads)
            {
                thread.Join();
            }
            _threads.Clear();

            lock (_ballsLock)
            {
                BallsList.Clear();
            }

            // Resetowanie CancellationTokenSource dla potencjalnego przyszłego użycia
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        public override void Dispose()
        {
            Stop();
            lock (_ballsLock)
            {
                BallsList.Clear();
            }
            GC.SuppressFinalize(this);
        }
    }
}