using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TP.ConcurrentProgramming.BusinessLogic; 

// ETAP 3
namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class DiagnosticsLogger : IDisposable
    {
        private readonly BlockingCollection<string> _logQueue = new(1000);
        private readonly Task _writerTask;
        private readonly CancellationTokenSource _cts = new();
        private readonly string _logFilePath;
        private readonly SemaphoreSlim _writerSemaphore = new(1, 1);

        internal DiagnosticsLogger(string logFilePath = "ball_diagnostics.txt")
        {
            _logFilePath = logFilePath;
            Console.WriteLine("Logging to: " + _logFilePath); 

            _writerTask = Task.Factory.StartNew(ProcessLogs, _cts.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        internal void LogBallState(Ball ball, double elapsedTime)
        {
            try
            {
                var logEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    ElapsedTime = elapsedTime,
                    BallId = ball.GetHashCode(),
                    Position = new { X = ball.DataBall.GetPosition().x, Y = ball.DataBall.GetPosition().y },
                    Velocity = new { X = ball.DataBall.Velocity.x, Y = ball.DataBall.Velocity.y },
                    RealTimeMetrics = new
                    {
                        FrameTime = elapsedTime,
                        IsRealTime = elapsedTime < 0.033, 
                        ThreadId = Thread.CurrentThread.ManagedThreadId
                    }
                };

                // Serializacja do ASCII
                var jsonData = SerializeToAscii(logEntry);
                
                // Obsługa przepustowości - timeout
                if (!_logQueue.TryAdd(jsonData, 10)) 
                {
                    Console.WriteLine($"Warning: Log queue full, dropping ball state log for Ball {ball.GetHashCode()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        internal void LogDeadlineMiss(Ball ball, double actualTime, double deadline)
        {
            try
            {
                var logEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Event = "DeadlineMiss",
                    BallId = ball.GetHashCode(),
                    ActualTime = actualTime,
                    Deadline = deadline,
                    Overrun = actualTime - deadline
                };

                var jsonData = SerializeToAscii(logEntry);
                
                if (!_logQueue.TryAdd(jsonData, 50)) // 50ms timeout
                {
                    Console.WriteLine($"Critical: Failed to log deadline miss for Ball {ball.GetHashCode()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deadline miss logging error: {ex.Message}");
            }
        }

        internal void LogCollision(Ball ball1, Ball ball2)
        {
            try
            {
                var logEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Event = "Collision",
                    Ball1 = ball1.GetHashCode(),
                    Ball2 = ball2.GetHashCode(),
                    Ball1Position = new { X = ball1.DataBall.GetPosition().x, Y = ball1.DataBall.GetPosition().y },
                    Ball2Position = new { X = ball2.DataBall.GetPosition().x, Y = ball2.DataBall.GetPosition().y }
                };

                var jsonData = SerializeToAscii(logEntry);
                
                if (!_logQueue.TryAdd(jsonData, 25)) 
                {
                    Console.WriteLine($"Warning: Failed to log collision between Ball {ball1.GetHashCode()} and Ball {ball2.GetHashCode()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Collision logging error: {ex.Message}");
            }
        }

        private string SerializeToAscii(object obj)
        {
            // Serializacja JSON z opcjami ASCII
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            };

            var jsonString = JsonSerializer.Serialize(obj, options);
            
            var asciiBytes = Encoding.ASCII.GetBytes(jsonString);
            return Encoding.ASCII.GetString(asciiBytes);
        }

        private async void ProcessLogs()
        {
            try
            {
                using var writer = new StreamWriter(_logFilePath, true, Encoding.ASCII);
                
                await writer.WriteLineAsync("=== Diagnostics Log Started ===");
                await writer.WriteLineAsync($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                await writer.FlushAsync();

                foreach (var log in _logQueue.GetConsumingEnumerable(_cts.Token))
                {
                    await WriteWithRetry(writer, log);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Log processing cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error in log processing: {ex.Message}");
            }
        }

        private async Task WriteWithRetry(StreamWriter writer, string logData)
        {
            const int maxRetries = 3;
            const int baseDelayMs = 100;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await _writerSemaphore.WaitAsync(1000); 
                    
                    try
                    {
                        await writer.WriteLineAsync(logData);
                        await writer.FlushAsync();
                        return; 
                    }
                    finally
                    {
                        _writerSemaphore.Release();
                    }
                }
                catch (IOException ioEx) when (attempt < maxRetries - 1)
                {
                    // Przepustowość niedostępna - czekamy z exponential backoff
                    var delay = baseDelayMs * (int)Math.Pow(2, attempt);
                    Console.WriteLine($"IO Error (attempt {attempt + 1}/{maxRetries}): {ioEx.Message}. Retrying in {delay}ms");
                    
                    try
                    {
                        await Task.Delay(delay, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return; // Cancelled during delay
                    }
                }
                catch (UnauthorizedAccessException uaEx) when (attempt < maxRetries - 1)
                {
                    // Plik zablokowany przez inny proces
                    var delay = baseDelayMs * 2;
                    Console.WriteLine($"Access denied (attempt {attempt + 1}/{maxRetries}): {uaEx.Message}. Retrying in {delay}ms");
                    
                    try
                    {
                        await Task.Delay(delay, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write log entry (attempt {attempt + 1}/{maxRetries}): {ex.Message}");
                    
                    if (attempt == maxRetries - 1)
                    {
                        // Ostatnia próba 
                        Console.WriteLine($"CRITICAL: Lost log entry after {maxRetries} attempts: {logData.Substring(0, Math.Min(100, logData.Length))}...");
                    }
                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Shutting down DiagnosticsLogger...");
            
            _cts.Cancel();

            // Daj czas na dokończenie zapisu
            try 
            { 
                _writerTask.Wait(2000); // 2 sekundy na graceful shutdown
            } 
            catch (AggregateException ex)
            {
                Console.WriteLine($"Logger shutdown warning: {ex.InnerException?.Message}");
            }

            _cts.Dispose();
            _logQueue.Dispose();
            _writerSemaphore.Dispose();
            
            Console.WriteLine("DiagnosticsLogger disposed");
        }
    }
}