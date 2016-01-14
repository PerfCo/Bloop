namespace Amazon.SimpleQueue.Messages
{
    public class AmazonDataMessage
    {
        public AmazonDeleteMessage DeleteMessage { get; set; }
        public int AttemptCount { get; set; }
        public string RawData { get; set; }
    }
}