using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly Data.IBall _dataBall;
        private List<Ball> _otherBalls = new();
        private readonly object _ballsLock = new object();

        public Ball(Data.IBall ball)
        {
            _dataBall = ball;
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        public void SetOtherBalls(List<Ball> otherBalls)
        {
            _otherBalls = otherBalls;
        }

        public Data.IBall DataBall => _dataBall;

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            CheckCollisionWithWalls(e);
            CheckCollisionWithOtherBalls();
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        private void CheckCollisionWithWalls(Data.IVector e)
        {
            double minX = 0, maxX = 400, minY = 0, maxY = 420;
            double radius = 10;

            lock (_ballsLock)
            {
                var position = _dataBall.GetPosition();
                if (position == null) return;

                if (position.x <= minX + radius)
                {
                    _dataBall.Velocity = new Vector(-_dataBall.Velocity.x, _dataBall.Velocity.y);
                    _dataBall.SetPosition(new Vector(minX + radius, position.y));
                }
                else if (position.x >= maxX - radius)
                {
                    _dataBall.Velocity = new Vector(-_dataBall.Velocity.x, _dataBall.Velocity.y);
                    _dataBall.SetPosition(new Vector(maxX - radius, position.y));
                }

                if (position.y <= minY + radius)
                {
                    _dataBall.Velocity = new Vector(_dataBall.Velocity.x, -_dataBall.Velocity.y);
                    _dataBall.SetPosition(new Vector(position.x, minY + radius));
                }
                else if (position.y >= maxY - radius)
                {
                    _dataBall.Velocity = new Vector(_dataBall.Velocity.x, -_dataBall.Velocity.y);
                    _dataBall.SetPosition(new Vector(position.x, maxY - radius));
                }
            }
        }

        private void CheckCollisionWithOtherBalls()
        {
            double radius = 10;

            foreach (var other in _otherBalls)
            {
                if (other == this) continue;

                var posA = _dataBall.GetPosition();
                var posB = other._dataBall.GetPosition();

                var dx = posA.x - posB.x;
                var dy = posA.y - posB.y;
                var distanceSquared = dx * dx + dy * dy;
                var minDistance = 2 * radius;

                if (distanceSquared <= minDistance * minDistance)
                {
                    lock (_ballsLock)
                    {
                        var tempVelocity = _dataBall.Velocity;
                        _dataBall.Velocity = other._dataBall.Velocity;
                        other._dataBall.Velocity = tempVelocity;

                        var newPosA = new Vector(posA.x + _dataBall.Velocity.x, posA.y + _dataBall.Velocity.y);
                        var newPosB = new Vector(posB.x + other._dataBall.Velocity.x, posB.y + other._dataBall.Velocity.y);
                        _dataBall.SetPosition(newPosA);
                        other._dataBall.SetPosition(newPosB);
                    }
                }
            }
        }

        #endregion private
    }
}