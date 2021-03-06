﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleQueue;
using Amazon.SimpleQueue.Messages;
using Core.Serializers;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using Nelibur.Sword.Threading.ThreadPools;
using NLog;

namespace StopWordsFilter.Processors
{
    public sealed class StopWordsProcessor : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMessageQueue _inputQueue;
        private readonly IMessageQueue _outputQueue;
        private readonly ITinyThreadPool _threadPool;
        private Option<CancellationTokenSource> _loopTask = Option<CancellationTokenSource>.Empty;

        public StopWordsProcessor(
            IMessageQueue inputQueue,
            IMessageQueue outputQueue,
            ITinyThreadPool threadPool,
            IDataSerializer dataSerializer)
        {
            _inputQueue = inputQueue;
            _outputQueue = outputQueue;
            _threadPool = threadPool;
        }

        public void Dispose()
        {
            Stop();
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
            _threadPool.SafeDispose(_logger.Error);
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
                    List<AmazonDataMessage> messages = _inputQueue.Receive();
                    messages.ForEach(x => _threadPool.AddTask(() => Process(x)));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void Process(AmazonDataMessage message)
        {
            throw new NotImplementedException();
        }
    }
}