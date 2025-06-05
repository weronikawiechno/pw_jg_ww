namespace TP.ConcurrentProgramming.Data
{
    public interface IBall
    {
        IVector GetPosition();
        void SetPosition(IVector v);
        IVector Velocity { get; set; }
        event EventHandler<IVector> NewPositionNotification;
    }

    public class Ball : IBall
    {
        private IVector _position;
        public IVector Velocity { get; set; }
        public event EventHandler<IVector>? NewPositionNotification;

        public Ball(IVector position, IVector velocity)
        {
            _position = position;
            Velocity = velocity;
        }

        public IVector GetPosition() => _position;

        public void SetPosition(IVector v) => _position = v;

        public void Move()
        {
            
            _position = new Vector(_position.x + Velocity.x, _position.y + Velocity.y);

           
            NewPositionNotification?.Invoke(this, _position);
        }
    }
}