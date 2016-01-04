using System;
using Ninject;
using NLog;

namespace StopWordsFilter
{
    public sealed class ServiceBuilder : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IKernel _kernel = new StandardKernel();
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _kernel.Dispose();
            _disposed = true;
        }

        public void BuildApplication()
        {
            _logger.Debug("Starting...");

            _logger.Info("StopWordsFilter is running. Press <Esc> or <Enter> to stop.");
        }
    }
}