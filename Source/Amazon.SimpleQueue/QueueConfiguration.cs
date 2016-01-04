using System;
using System.Configuration;
using Core.Serializers;

namespace Amazon.SimpleQueue
{
    public sealed class QueueConfiguration : IMessageQueueConfig
    {
        private QueueConfiguration()
        {
        }

        public string QueueUrl { get; set; }
        public IDataSerializer DataSerializer { get; set; }

        public static QueueConfiguration Create()
        {
            return new QueueConfiguration();
        }

        public QueueConfiguration Configure(Action<QueueConfiguration> action)
        {
            action(this);
            return this;
        }

        public IMessageQueue CreateQueue()
        {
            Validate();
            return new AmazonMessageQueue(this);
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(QueueUrl))
            {
                throw new ConfigurationErrorsException(nameof(QueueUrl));
            }
            if (DataSerializer == null)
            {
                throw new ConfigurationErrorsException(nameof(DataSerializer));
            }
        }
    }
}