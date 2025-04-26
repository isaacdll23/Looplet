using MongoDB.Bson;
using System.Text.Json;

namespace Looplet.Shared.Extensions;

public static class JsonElementExtensions
{
    public static BsonValue ToBson(this JsonElement e)
    {
        return e.ValueKind switch
        {
            JsonValueKind.Object => new BsonDocument(e.EnumerateObject().ToDictionary(prop => prop.Name, prop => ToBson(prop.Value))),
            JsonValueKind.Array => new BsonArray(e.EnumerateArray().Select(ToBson)),
            JsonValueKind.String => new BsonString(e.GetString()),
            JsonValueKind.Number when e.TryGetInt64(out var l) => new BsonInt64(l),
            JsonValueKind.Number when e.TryGetDouble(out var d) => new BsonDouble(d),
            JsonValueKind.Number => new BsonDouble(e.GetDouble()),
            JsonValueKind.True or JsonValueKind.False => new BsonBoolean(e.GetBoolean()),
            JsonValueKind.Null or JsonValueKind.Undefined => BsonNull.Value,
            _ => throw new NotSupportedException($"Unsupported JSON kind {e.ValueKind}")
        };
    }
}
