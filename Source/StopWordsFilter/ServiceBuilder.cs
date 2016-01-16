using System;
using Nelibur.Sword.Extensions;
using Ninject;
using NLog;
using StopWordsFilter.Dependencies;
using StopWordsFilter.Processors;

namespace StopWordsFilter
{
    public sealed class ServiceBuilder : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IKernel _kernel = new StandardKernel(new ApplicationDependencyModule());
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
            StartProcessors();
            _logger.Info("StopWordsFilter is running. Press <Esc> or <Enter> to stop.");
        }

        private void StartProcessors()
        {
            _kernel.Get<StopWordsProcessor>().Start();
        }

        private void StopProcessors()
        {
            _kernel.Get<StopWordsProcessor>().SafeDispose(_logger.Error);
        }
    }
}