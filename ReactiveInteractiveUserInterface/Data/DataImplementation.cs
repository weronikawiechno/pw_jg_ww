using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            BallsList.Clear();

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(
                    random.Next((int)_ballDiameter, (int)(_width - _ballDiameter)),
                    random.Next((int)_ballDiameter, (int)(_height - _ballDiameter))
                );

                Vector velocity = new(
                    (random.NextDouble() - 0.5) * 5,
                    (random.NextDouble() - 0.5) * 5
                );

                Ball newBall = new(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;

        private readonly Timer MoveTimer;
        private readonly ConcurrentBag<Ball> BallsList = new(); // Thread-safe collection

        private double _width = 400;
        private double _height = 400;
        private double _ballDiameter = 30;

        public void UpdateBoundaries(double width, double height)
        {
            _width = width;
            _height = height;
        }

        private void Move(object? x)
        {
            Parallel.ForEach(BallsList, item =>
            {
                // Since Ball.Move expects a Vector, cast or convert IVector
                item.Move((Vector)item.Velocity);

                foreach (var other in BallsList)
                {
                    if (item != other)
                    {
                        item.ResolveCollision(other);
                    }
                }

                LockAndHandleWallCollision(item);
            });
        }

        private void LockAndHandleWallCollision(Ball ball)
        {
            lock (ball)
            {
                Vector position = ball.GetPosition();

                if (position.x < 0 || position.x > _width)
                {
                    ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);
                }

                if (position.y < 0 || position.y > _height)
                {
                    ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);
                }
            }
        }

        #endregion private
    }
}