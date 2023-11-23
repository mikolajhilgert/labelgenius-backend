using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace projectservice.Models
{
    public class Label
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        [BsonElement("x")]
        public double X { get; set; } = 0.0;
        [BsonElement("y")]
        public double Y { get; set; } = 0.0;
        [BsonElement("height")]
        public double Height { get; set; } = 0.0;
        [BsonElement("width")]
        public double Width { get; set; } = 0.0;
        [BsonElement("className")]
        public string ClassName { get; set; } = string.Empty;
    }
}
