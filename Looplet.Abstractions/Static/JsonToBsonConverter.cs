using MongoDB.Bson;
using System.Text.Json;

namespace Looplet.Abstractions.Static;

public static class JsonToBsonConverter
{
    public static BsonValue ConvertJsonElementToBsonValue(JsonElement e)
    {
        return e.ValueKind switch
        {
            JsonValueKind.Object => new BsonDocument(e.EnumerateObject().ToDictionary(prop => prop.Name, prop => ConvertJsonElementToBsonValue(prop.Value))),
            JsonValueKind.Array => new BsonArray(e.EnumerateArray().Select(ConvertJsonElementToBsonValue)),
            JsonValueKind.String => new BsonString(e.GetString()),
            JsonValueKind.Number when e.TryGetInt64(out var l) => new BsonInt64(l),
            JsonValueKind.Number when e.TryGetDouble(out var d) => new BsonDouble(d),
            JsonValueKind.Number => new BsonDouble(e.GetDouble()),
            JsonValueKind.True or JsonValueKind.False => new BsonBoolean(e.GetBoolean()),
            JsonValueKind.Null or JsonValueKind.Undefined => BsonNull.Value,
            _ => throw new NotSupportedException($"Unsupported JSON kind {e.ValueKind}")
        };
    }

    public static BsonDocument? ConvertJsonElementToBsonDocument(JsonElement element)
    {

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        BsonDocument? parameters = null;

        parameters = [.. element
            .EnumerateObject()
            .Select(p => new BsonElement(p.Name, ConvertJsonElementToBsonValue(p.Value)))];

        return parameters;
    }

    public static BsonDocument? ConvertJsonElementToBsonDocument(JsonElement? element)
    {
        BsonDocument? parameters = null;
        if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object)
        {
            parameters = [.. element.Value
            .EnumerateObject()
            .Select(p => new BsonElement(p.Name, ConvertJsonElementToBsonValue(p.Value)))];
        }

        return parameters;
    }

}
