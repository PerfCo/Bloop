using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NLog;

namespace Redis.LocalQueue
{
    internal class Program
    {
        private const string RedisExeFolder = "RedisServer";
        private const string RedisExeFile = "redis-server.exe";
        private const string RedisConfigurationFile = "redis.windows.conf";
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static Process _process;

        private static void Main(string[] args)
        {
            try
            {
                StartRedis();
                Console.ReadKey();
                StopRedis();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                Console.ReadKey();
            }
        }

        private static void StopRedis()
        {
            _process.Kill();
        }

        private static void StartRedis()
        {
            ProcessStartInfo processInfo = GetRedisStartInfo();
            _process = new Process {StartInfo = processInfo};
            _process.Start();
        }

        private static ProcessStartInfo GetRedisStartInfo()
        {
            var redisDirectory = Path.Combine(GetAssemblyDirectory(), RedisExeFolder);
            var configFile = Path.Combine(redisDirectory, RedisConfigurationFile);
            string arguments = $"\"{configFile}\"";

            return new ProcessStartInfo
            {
                FileName = Path.Combine(redisDirectory, RedisExeFile),
                Arguments = arguments,
                UseShellExecute = false
            };
        }

        private static string GetAssemblyDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}