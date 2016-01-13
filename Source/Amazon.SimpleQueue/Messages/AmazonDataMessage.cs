namespace Amazon.SimpleQueue.Messages
{
    public abstract class AmazonDataMessage
    {
        public AmazonDeleteMessage DeleteMessage { get; set; }
        public int AttemptCount { get; set; }
        public string RawData { protected get; set; }
    }
}