using System.Collections.Generic;
using Amazon.SimpleQueue.Messages;

namespace Amazon.SimpleQueue
{
    public interface IMessageQueue
    {
        void Processed(AmazonDeleteMessage message);
        void Send(object value);

        List<TMessage> Receive<TMessage>()
            where TMessage : AmazonDataMessage, new();
    }
}