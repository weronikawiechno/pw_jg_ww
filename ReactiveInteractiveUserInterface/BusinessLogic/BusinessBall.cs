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
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    public Ball(Data.IBall ball)
    {
      ball.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }

    #endregion private
  }

  internal class BusinessBall : IBall
  {
    private readonly Data.IBall _dataBall;
        
    public event EventHandler<IPosition>? NewPositionNotification;
        
    public BusinessBall(Data.IBall dataBall)
    {
      _dataBall = dataBall;
      _dataBall.NewPositionNotification += OnDataBallPositionChanged;
    }
        
    private void OnDataBallPositionChanged(object? sender, IVector vector)
    {
      NewPositionNotification?.Invoke(this, new Position(vector.x, vector.y));
    }
        
    public Data.IBall GetDataBall()
    {
      return _dataBall;
    }
  }
}