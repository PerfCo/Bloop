using Core.Serializers;

namespace Amazon.SimpleQueue
{
    public interface IMessageQueueConfig
    {
        string QueueUrl { get; set; }
        IDataSerializer DataSerializer { get; set; }
    }
}