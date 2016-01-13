using System;
using System.Collections.Generic;
using Amazon.SimpleQueue.Messages;
using StackExchange.Redis;

namespace Amazon.SimpleQueue
{
    public sealed class LocalMessageQueue : IMessageQueue
    {
        private static readonly ConnectionMultiplexer _redis;
        private readonly IMessageQueueConfig _config;
        private readonly ISubscriber _subscriber;

        static LocalMessageQueue()
        {
            var config = new ConfigurationOptions
            {
                AllowAdmin = true,
                EndPoints = { { "localhost", 6379 } },
                AbortOnConnectFail = false,
                SyncTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
            };

            _redis = ConnectionMultiplexer.Connect(config);
        }

        public LocalMessageQueue(IMessageQueueConfig config)
        {
            _config = config;
            _subscriber = _redis.GetSubscriber();
        }

        public void Processed(AmazonDeleteMessage message)
        {
            throw new NotImplementedException();
        }

        public void Send(object value)
        {
            string json = _config.DataSerializer.ToJson(value);
            _subscriber.Publish(_config.QueueUrl, json);
        }

        public List<TMessage> Receive<TMessage>()
            where TMessage : AmazonDataMessage, new()
        {
            throw new NotImplementedException();
        }


//        public List<TMessage> Receive<TMessage>()
//        {
//            string json = null;
//            var syncEvent = new AutoResetEvent(false);
//
//            Action<RedisChannel, RedisValue> oneTimeHandler = null;
//            oneTimeHandler = (redisChannel, redisValue) =>
//            {
//                _subscriber.Unsubscribe(_config.QueueUrl, oneTimeHandler);
//                json = redisValue;
//                syncEvent.Set();
//            };
//
//            _subscriber.Subscribe(_config.QueueUrl, oneTimeHandler);
//            syncEvent.WaitOne();
//
//            var result = new List<TMessage> { _config.DataSerializer.FromJson<TMessage>(json).Value };
//            return result;
//        }
    }
}