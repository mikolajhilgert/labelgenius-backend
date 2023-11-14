using projectservice.Dto;

namespace projectservice.Services
{
    public interface IProjectService
    {
        Task<(bool Result, string Message)> CreateProject(ProjectDto dto);
    }
}
