using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace projectservice.Dto
{
    public class ProjectDto
    {
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        [Required]
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectCreator { get; set; } = string.Empty;
        [Required]
        public Dictionary<string, string> LabelClasses { get; set; } = new Dictionary<string, string>();
        public List<(string FileName, byte[] ImageAsByteArray)> Images { get; set; } = new List<(string FileName, byte[] Image)>();
        [Required]
        public List<IFormFile>? FormFiles { get; set; }
    }
}
