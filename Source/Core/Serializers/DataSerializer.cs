using System;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using Newtonsoft.Json;
using NLog;

namespace Core.Serializers
{
    public sealed class DataSerializer : IDataSerializer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string ToJson(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public Option<TData> FromJson<TData>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<TData>(value).ToOption();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return Option<TData>.Empty;
            }
        }
    }
}