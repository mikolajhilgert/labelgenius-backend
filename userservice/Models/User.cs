using MongoDB.Bson.Serialization.Attributes;

namespace userservice.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        [BsonElement("email")]
        public string email { get; set; } = string.Empty;
    }
}
