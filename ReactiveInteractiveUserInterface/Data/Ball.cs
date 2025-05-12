namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }
        public double Mass { get; set; } = 1.0; // Default mass
        public double Diameter { get; set; } = 30.0; // Diameter for collision detection

        #endregion IBall

        #region private

        private Vector Position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

        // Public method to get the current position of the ball
        public Vector GetPosition()
        {
            return Position;
        }

        internal void ResolveCollision(Ball other)
        {
            Vector delta = new Vector(Position.x - other.Position.x, Position.y - other.Position.y);
            double distance = Math.Sqrt(delta.x * delta.x + delta.y * delta.y);

            if (distance < (this.Diameter + other.Diameter) / 2)
            {
                // Normalize the delta vector
                double nx = delta.x / distance;
                double ny = delta.y / distance;

                // Relative velocity
                Vector dv = new Vector(Velocity.x - other.Velocity.x, Velocity.y - other.Velocity.y);
                double velocityAlongNormal = dv.x * nx + dv.y * ny;

                if (velocityAlongNormal > 0) return; // Balls are separating, no collision

                // Elastic collision impulse
                double impulse = (2 * velocityAlongNormal) / (this.Mass + other.Mass);
                Velocity = new Vector(
                    Velocity.x - impulse * other.Mass * nx,
                    Velocity.y - impulse * other.Mass * ny
                );

                other.Velocity = new Vector(
                    other.Velocity.x + impulse * this.Mass * nx,
                    other.Velocity.y + impulse * this.Mass * ny
                );
            }
        }

        #endregion private
    }
}