using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;

//ETAP 3
namespace TP.ConcurrentProgramming.BusinessLogic.Tests
{
    [TestClass]
    public class DiagnosticsLoggerTests
    {
        private string _testLogPath;
        private DiagnosticsLogger _logger;

        [TestInitialize]
        public void Setup()
        {
            _testLogPath = Path.GetTempFileName();
            _logger = new DiagnosticsLogger(_testLogPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger?.Dispose();
            if (File.Exists(_testLogPath))
                File.Delete(_testLogPath);
        }

        [TestMethod]
        public void Constructor_CreatesLogFile()
        {
            Assert.IsTrue(File.Exists(_testLogPath), "Log file should be created");
        }

        [TestMethod]
        public void LogBallState_WritesValidJson()
        {
            var mockBall = CreateMockBall();
            var elapsedTime = 0.016;

            _logger.LogBallState(mockBall, elapsedTime);
            Thread.Sleep(100);

            var logContent = File.ReadAllText(_testLogPath);
            Assert.IsTrue(logContent.Contains("BallId"), "Log should contain BallId");
            Assert.IsTrue(logContent.Contains("Position"), "Log should contain Position");
            Assert.IsTrue(logContent.Contains("Velocity"), "Log should contain Velocity");
            Assert.IsTrue(logContent.Contains("RealTimeMetrics"), "Log should contain RealTimeMetrics");
        }

        [TestMethod]
        public void LogDeadlineMiss_WritesDeadlineEvent()
        {
            var mockBall = CreateMockBall();
            var actualTime = 0.025;
            var deadline = 0.020;

            _logger.LogDeadlineMiss(mockBall, actualTime, deadline);
            Thread.Sleep(100);

            var logContent = File.ReadAllText(_testLogPath);
            Assert.IsTrue(logContent.Contains("DeadlineMiss"), "Log should contain DeadlineMiss event");
            Assert.IsTrue(logContent.Contains("Overrun"), "Log should contain Overrun calculation");
        }

        [TestMethod]
        public void LogCollision_WritesBothBalls()
        {
            var ball1 = CreateMockBall();
            var ball2 = CreateMockBall();

            _logger.LogCollision(ball1, ball2);
            Thread.Sleep(100);

            var logContent = File.ReadAllText(_testLogPath);
            Assert.IsTrue(logContent.Contains("Collision"), "Log should contain Collision event");
            Assert.IsTrue(logContent.Contains("Ball1"), "Log should contain Ball1");
            Assert.IsTrue(logContent.Contains("Ball2"), "Log should contain Ball2");
        }



        private Ball CreateMockBall()
        {
            // Create a mock ball with test data
            var dataBall = new TP.ConcurrentProgramming.Data.Ball(
                new TP.ConcurrentProgramming.Data.Vector(100, 150),
                new TP.ConcurrentProgramming.Data.Vector(2.5, -1.8)
            );
            return new Ball(dataBall);
        }
    }
}