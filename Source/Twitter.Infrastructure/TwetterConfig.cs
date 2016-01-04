using System.Collections.Generic;

namespace Twitter.Infrastructure
{
    public sealed class TwetterConfig
    {
        public TwetterConfig()
        {
            TrackKeywords = new List<string>();
            FollowUserIds = new List<string>();
        }

        public List<string> TrackKeywords { get; set; }
        public List<string> FollowUserIds { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
    }
}