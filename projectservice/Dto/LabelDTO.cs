using System.ComponentModel.DataAnnotations;

namespace projectservice.Dto
{
    public class CreateLabelDTO
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string ImageId { get; set; } = string.Empty;
        [Required]
        public double X { get; set; } = 0.0;
        [Required]
        public double Y { get; set; } = 0.0;
        [Required]
        public double Height { get; set; } = 0.0;
        [Required]
        public double Width { get; set; } = 0.0;
        [Required]
        public string ClassName { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
    }
    public class UpdateLabelDTO
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        [Required]
        public string ImageId { get; set; } = string.Empty;
        [Required]
        public double X { get; set; } = 0.0;
        [Required]
        public double Y { get; set; } = 0.0;
        [Required]
        public double Height { get; set; } = 0.0;
        [Required]
        public double Width { get; set; } = 0.0;
        [Required]
        public string ClassName { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
    }
    public class ResponseLabelDTO
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ImageId { get; set; } = string.Empty;
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;
        public double Height { get; set; } = 0.0;
        public double Width { get; set; } = 0.0;
        public string ClassName { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
    }
}
