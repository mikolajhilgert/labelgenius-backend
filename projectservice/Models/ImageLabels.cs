using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace projectservice.Models
{
    public class ImageLabels
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        [BsonElement("projectId")]
        public ObjectId ProjectId { get; set; } = ObjectId.Empty;
        [BsonElement("imageId")]
        public string ImageId { get; set; } = string.Empty;
        [BsonElement("creator")]
        public string Creator { get; set; } = string.Empty;
        [BsonElement("labels")]
        public List<Label> Labels { get; set; } = new();
    }
}
