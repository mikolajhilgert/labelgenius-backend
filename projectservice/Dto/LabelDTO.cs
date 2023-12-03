using System.ComponentModel.DataAnnotations;

namespace projectservice.Dto
{
    public class ImageLabelsDTO
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string ImageId { get; set; } = string.Empty;
        [Required]
        public List<LabelDTO> Labels { get; set; } = new();
        public string Creator { get; set; } = string.Empty;
    }
    public class LabelDTO
    {
        public string Id { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;
        public double Height { get; set; } = 0.0;
        public double Width { get; set; } = 0.0;
    }
}

