using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace projectservice.Models
{
    public class Project
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("creator")]
        public string Creator { get; set; } = string.Empty;

        [BsonElement("labelClasses")]
        public Dictionary<string, string> LabelClasses { get; set; } = new();

        [BsonElement("imageUrls")]
        public Dictionary<string, string> ImageUrls { get; set; } = new();

        [BsonElement("labellingUsers")]
        public List<string> LabellingUsers { get; set; } = new();

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        [BsonElement("creationDate")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}
