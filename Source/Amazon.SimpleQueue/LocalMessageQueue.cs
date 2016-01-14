using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.SimpleQueue.Messages;
using StackExchange.Redis;

namespace Amazon.SimpleQueue
{
    public sealed class LocalMessageQueue : IMessageQueue
    {
        private static readonly ConnectionMultiplexer _redis;
        private readonly IMessageQueueConfig _config;
        private readonly object _lockObject = new object();
        private readonly List<AmazonDataMessage> _receivedMessages = new List<AmazonDataMessage>();
        private readonly ISubscriber _subscriber;

        static LocalMessageQueue()
        {
            var config = new ConfigurationOptions
            {
                AllowAdmin = true,
                EndPoints = { { "localhost", 6379 } },
                AbortOnConnectFail = false,
                SyncTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds
            };

            _redis = ConnectionMultiplexer.Connect(config);
        }

        public LocalMessageQueue(IMessageQueueConfig config)
        {
            _config = config;
            _subscriber = _redis.GetSubscriber();
            _subscriber.Subscribe(_config.QueueUrl, OnMessageReceived);
        }

        public void Processed(AmazonDeleteMessage message)
        {
        }

        public void Send(object value)
        {
            string json = _config.DataSerializer.ToJson(value);
            _subscriber.Publish(_config.QueueUrl, json);
        }

        public List<AmazonDataMessage> Receive()
        {
            SpinWait.SpinUntil(() => _receivedMessages.Count > 0);
            return DequeueRecievedMessages();
        }

        private List<AmazonDataMessage> DequeueRecievedMessages()
        {
            lock (_lockObject)
            {
                var result = _receivedMessages;
                _receivedMessages.Clear();
                return result;
            }
        }

        private void OnMessageReceived(RedisChannel redisChannel, RedisValue redisValue)
        {
            AmazonDataMessage message = _config.DataSerializer.FromJson<AmazonDataMessage>(redisValue).Value;
            lock (_lockObject)
            {
                _receivedMessages.Add(message);
            }
        }
    }
}