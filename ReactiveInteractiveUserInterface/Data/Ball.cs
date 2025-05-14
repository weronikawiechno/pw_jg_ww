namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }
        public double Mass { get; set; } = 1.0;
        public double Diameter { get; set; } = 30.0;

        private Vector Position;

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
        }

        internal void Move()
        {
            Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
            NewPositionNotification?.Invoke(this, Position);
        }

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
                double nx = delta.x / distance;
                double ny = delta.y / distance;

                Vector dv = new Vector(Velocity.x - other.Velocity.x, Velocity.y - other.Velocity.y);
                double velocityAlongNormal = dv.x * nx + dv.y * ny;

                if (velocityAlongNormal > 0) return;

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
    }
}