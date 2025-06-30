using IDC.Utilities.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.Extensions;

internal static class MongoExtensions
{
    #region Data Manipulation
    internal static JObject UpdateProcessTime(this JObject json) =>
        json.PropUpsert(
            path: "processing_time",
            value: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        );

    internal static BsonDocument UpdateProcessTime(this BsonDocument bson)
    {
        bson["processing_time"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return bson;
    }
    #endregion Data Manipulation


    internal static JObject? AsJObject(this BsonDocument? bson) =>
        bson == null ? null : JObject.Parse(bson.ToString());

    internal static BsonDocument? AsBsonDocument(this JObject? json) =>
        json == null ? null : BsonDocument.Parse(json.ToString());

    internal static Dictionary<string, object?> AsDictionary(this JObject? json) =>
        json == null ? [] : json.ToObject<Dictionary<string, object?>>()!;
}
