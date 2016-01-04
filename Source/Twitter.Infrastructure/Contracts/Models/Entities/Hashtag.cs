using System.Collections.Generic;
using Newtonsoft.Json;

namespace Twitter.Infrastructure.Contracts.Models.Entities
{
    public sealed class Hashtag
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("indices")]
        public List<int> Indices { get; set; }
    }
}