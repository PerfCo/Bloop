using Nelibur.Sword.DataStructures;

namespace Core.Serializers
{
    public interface IDataSerializer
    {
        string ToJson(object value);
        Option<TData> FromJson<TData>(string value);
    }
}