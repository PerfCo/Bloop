using Twitter.Infrastructure.Contracts.Models;

namespace Twitter.Infrastructure.Contracts
{
    public interface ITwitterObserver
    {
        void Notify(Tweet tweet);
    }
}