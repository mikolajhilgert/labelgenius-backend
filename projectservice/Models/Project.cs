using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace projectservice.Models
{
    public class Project
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [BsonElement("projectDescription")]
        public string ProjectDescription { get; set; } = string.Empty;

        [BsonElement("projectCreator")]
        public string ProjectCreator { get; set; } = string.Empty;

        [BsonElement("labelClasses")]
        public Dictionary<string, string> LabelClasses { get; set; } = new Dictionary<string, string>();
        [BsonElement("pictureUrls")]
        public List<string> PictureUrls { get; set; } = new List<string>();
    }
}
