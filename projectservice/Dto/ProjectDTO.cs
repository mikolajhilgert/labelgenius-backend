using System.ComponentModel.DataAnnotations;

namespace projectservice.Dto
{
    public class CreateProjectDTO
    {
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        [Required]
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectCreator { get; set; } = string.Empty;
        [Required]
        public Dictionary<string, string> LabelClasses { get; set; } = new();
        [Required]
        public List<IFormFile> FormFiles { get; set; } = new List<IFormFile>();
        [Required]
        public bool IsActive { get; set; } = true;

        // Images as Bytes
        public List<(string FileName, byte[] ImageAsByteArray)> Images { get; set; } = new List<(string FileName, byte[] Image)>();
    }
    public class UpdateProjectDTO
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        [Required]
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectCreator { get; set; } = string.Empty;
        [Required]
        public Dictionary<string, string> LabelClasses { get; set; } = new();
        [Required]
        public bool IsActive { get; set; } = true;
    }
    public class ResponseProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectCreator { get; set; } = string.Empty;
        public Dictionary<string, string> LabelClasses { get; set; } = new();
        public Dictionary<string, string> Images { get; set; } = new();
        public string ImageSasToken { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public bool UserIsOwner { get; set; }
    }
}
