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
using System.Diagnostics;

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
    private Random RandomGenerator = new();
    private List<Ball> BallsList = new();

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
      foreach (Ball item in BallsList)
      {
        Vector velocity = (Vector)item.Velocity;
        
        item.Move(velocity);
        
       
        object? positionObj = item.GetType().GetField("Position", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance)?.GetValue(item);
            
        if (positionObj is Vector position)
        {
            bool needsXBounce = false;
            bool needsYBounce = false;
            
            //  X boundaries 
            if (position.x < (_ballDiameter/2) || position.x > _width - (_ballDiameter/2))
                needsXBounce = true;
                
            //  Y boundaries 
            if (position.y < (_ballDiameter/2) || position.y > _height - (_ballDiameter/2))
                needsYBounce = true;
                
            // velocity changes 
            if (needsXBounce)
                item.Velocity = new Vector(1.5*(-velocity.x), 1.5*velocity.y); // it was very unrealistic when they bounced of with the same speed
                
            if (needsYBounce)
                item.Velocity = new Vector(1.5*velocity.x, 1.5*(-velocity.y));
        }
        else
        {
            double bounceProbability = 0;
        }
      }
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}