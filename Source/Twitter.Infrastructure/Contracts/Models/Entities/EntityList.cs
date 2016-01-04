using System.Collections.Generic;
using Newtonsoft.Json;

namespace Twitter.Infrastructure.Contracts.Models.Entities
{
    public sealed class EntityList
    {
        [JsonProperty("hashtags")]
        public List<Hashtag> Hashtags { get; set; }

        [JsonProperty("trends")]
        public List<object> Trends { get; set; }

        [JsonProperty("urls")]
        public List<object> Urls { get; set; }

        [JsonProperty("user_mentions")]
        public List<object> UserMentions { get; set; }

        [JsonProperty("symbols")]
        public List<object> Symbols { get; set; }
    }
}