using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectservice.Models
{
    public class ProjectInvitation
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        [BsonElement("secret")]
        public string Secret { get; set; } = string.Empty;
        [BsonElement("invitee")]
        public string Invitee { get; set; } = string.Empty;
        [BsonElement("sender")]
        public string Sender { get; set; } = string.Empty;
        [BsonElement("projectid")]
        public string ProjectId { get; set; } = string.Empty;
    }
}
