using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleQueue.Messages;
using Amazon.SQS;
using Amazon.SQS.Model;
using Core.Serializers;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using NLog;

namespace Amazon.SimpleQueue
{
    public sealed class AmazonMessageQueue : IMessageQueue
    {
        private const string ApproximateReceiveCount = "ApproximateReceiveCount";
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly AmazonSQSClient _client = new AmazonSQSClient();
        private readonly IMessageQueueConfig _config;
        private readonly IDataSerializer _dataSerializer;

        public AmazonMessageQueue(IMessageQueueConfig config)
        {
            _config = config;
            _dataSerializer = config.DataSerializer;
        }

        public void Processed(AmazonDeleteMessage message)
        {
            RemoveMessage(message);
        }

        public void Send(object value)
        {
            string data = _dataSerializer.ToJson(value);
            var request = new SendMessageRequest
            {
                QueueUrl = _config.QueueUrl,
                MessageBody = data
            };
            _client.SendMessage(request);
        }

        public List<AmazonDataMessage> Receive()
        {
            Option<ReceiveMessageResponse> messageResponse = ReceiveMessage();
            Option<List<Message>> messages = messageResponse.Map(x => x.Messages);
            if (messages.HasNoValue)
            {
                return new List<AmazonDataMessage>();
            }
            return messages.Value.ConvertAll(CreateMessage).ToValue().ToList();
        }

        private Option<AmazonDataMessage> CreateMessage(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Body))
            {
                return Option<AmazonDataMessage>.Empty;
            }
            var deleteMessage = new AmazonDeleteMessage
            {
                ReceiptHandle = message.ReceiptHandle
            };
            var result = new AmazonDataMessage
            {
                DeleteMessage = deleteMessage,
                RawData = message.Body,
                AttemptCount = GetAttemptCount(message)
            };
            return result.ToOption();
        }

        private static int GetAttemptCount(Message message)
        {
            string attemptCount;
            if (message.Attributes.TryGetValue(ApproximateReceiveCount, out attemptCount) == false)
            {
                return int.MaxValue;
            }
            int result;
            if (int.TryParse(attemptCount, out result))
            {
                return result;
            }
            return int.MaxValue;
        }

        private Option<ReceiveMessageResponse> ReceiveMessage()
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = _config.MaxNumberOfMessages,
                    QueueUrl = _config.QueueUrl,
                    AttributeNames = new List<string> {ApproximateReceiveCount}
                };
                return _client.ReceiveMessage(request).ToOption();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Option<ReceiveMessageResponse>.Empty;
            }
        }

        private void RemoveMessage(AmazonDeleteMessage message)
        {
            try
            {
                var request = new DeleteMessageRequest
                {
                    QueueUrl = _config.QueueUrl,
                    ReceiptHandle = message.ReceiptHandle
                };
                _client.DeleteMessage(request);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }
    }
}