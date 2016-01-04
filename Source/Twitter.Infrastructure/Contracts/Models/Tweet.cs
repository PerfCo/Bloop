using Newtonsoft.Json;

namespace Twitter.Infrastructure.Contracts.Models
{
    /// <summary>
    ///     https://dev.twitter.com/overview/api/tweets
    /// </summary>
    public sealed class Tweet
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("in_reply_to_status_id")]
        public object InReplyToStatusId { get; set; }

        [JsonProperty("in_reply_to_status_id_str")]
        public object InReplyToStatusIdString { get; set; }

        [JsonProperty("in_reply_to_user_id")]
        public object InReplyToUserId { get; set; }

        [JsonProperty("in_reply_to_user_id_str")]
        public object InReplyToUserIdString { get; set; }

        [JsonProperty("in_reply_to_screen_name")]
        public object InReplyToScreenName { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("entities")]
        public Entities.EntityList Entities { get; set; }

        [JsonProperty("retweeted")]
        public bool Retweeted { get; set; }

        [JsonProperty("possibly_sensitive")]
        public bool PossiblySensitive { get; set; }

        [JsonProperty("filter_level")]
        public string FilterLevel { get; set; }

        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("timestamp_ms")]
        public string TimestampMs { get; set; }

        public override string ToString()
        {
            return $"id: {Id}, text: {Text}, lang: {Language}";
        }
    }
}