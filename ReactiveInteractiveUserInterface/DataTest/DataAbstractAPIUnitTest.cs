//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class DataAbstractAPIUnitTest
  {
    [TestMethod]
    public void ConstructorTestTestMethod()
    {
      DataAbstractAPI instance1 = DataAbstractAPI.GetDataLayer();
      DataAbstractAPI instance2 = DataAbstractAPI.GetDataLayer();
      Assert.AreSame<DataAbstractAPI>(instance1, instance2);
      instance1.Dispose();
      Assert.ThrowsException<ObjectDisposedException>(() => instance2.Dispose());
    }

    [TestMethod]
    public void GetDimensionsTestMethod()
    {
      Assert.AreEqual<Dimensions>(new(10.0, 10.0, 10.0), DataAbstractAPI.GetDimensions);
    }

    [TestMethod]
    public void GetBallTestMethod()
    {
      IVector initialPosition = new Vector(0.0, 0.0);
      IVector initialVelocity = new Vector(1.0, 1.0);
      IBall ball = DataAbstractAPI.GetBall(initialPosition, initialVelocity);
      Assert.IsNotNull(ball);
      Assert.AreEqual<IVector>(initialPosition, ball.Position);
      Assert.AreEqual<IVector>(initialVelocity, ball.Velocity);
    }
    
  }
}