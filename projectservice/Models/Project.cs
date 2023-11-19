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

        [BsonElement("projectLabelClasses")]
        public Dictionary<string, string> LabelClasses { get; set; } = new Dictionary<string, string>();

        [BsonElement("projectImageUrls")]
        public List<string> ImageUrls { get; set; } = new List<string>();

        [BsonElement("projectLabellingUsers")]
        public List<string> LabellingUsers { get; set; } = new List<string>();

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        [BsonElement("projectCreationDate")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [BsonElement("projectIsActive")]
        public bool IsActive { get; set; } = true;
    }
}
