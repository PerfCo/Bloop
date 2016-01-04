using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleQueue;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using Nelibur.Sword.Threading.ThreadPools;
using NLog;

namespace StopWordsFilter.Processors
{
    public sealed class StopWordsProcessor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMessageQueue _inputQueue;
        private readonly IMessageQueue _resultQueue;
        private readonly ITinyThreadPool _threadPool;
        private Option<CancellationTokenSource> _loopTask = Option<CancellationTokenSource>.Empty;

        public StopWordsProcessor(IMessageQueue inputQueue, IMessageQueue resultQueue, ITinyThreadPool threadPool)
        {
            _inputQueue = inputQueue;
            _resultQueue = resultQueue;
            _threadPool = threadPool;
        }

        public void Start()
        {
            if (_loopTask.HasValue)
            {
                return;
            }

            var tokenSource = new CancellationTokenSource();
            Task.Run(() => ProcessRequests(tokenSource.Token), tokenSource.Token);
            _loopTask = tokenSource.ToOption();
            _logger.Info("StopWordsProcessor started");
        }

        public void Stop()
        {
            _logger.Info("StopWordsProcessor stopped");
            _loopTask.Do(x => x.Cancel());
            _loopTask = Option<CancellationTokenSource>.Empty;
        }

        private void ProcessRequests(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    throw new NotImplementedException();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
    }
}