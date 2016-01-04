using Amazon.SQS;
using Amazon.SQS.Model;
using Core.Serializers;

namespace Amazon.SimpleQueue
{
    public sealed class AmazonMessageQueue : IMessageQueue
    {
        private readonly AmazonSQSClient _client = new AmazonSQSClient();
        private readonly IMessageQueueConfig _config;
        private readonly IDataSerializer _dataSerializer;

        public AmazonMessageQueue(IMessageQueueConfig config)
        {
            _config = config;
            _dataSerializer = config.DataSerializer;
        }

        public void Send(object value)
        {
            var data = _dataSerializer.ToJson(value);
            var request = new SendMessageRequest
            {
                QueueUrl = _config.QueueUrl,
                MessageBody = data
            };
            _client.SendMessage(request);
        }
    }
}