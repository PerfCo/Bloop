namespace Amazon.SimpleQueue
{
    public interface IMessageQueue
    {
        void Send(object value);
    }
}