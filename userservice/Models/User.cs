using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace userservice.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

    }
}
