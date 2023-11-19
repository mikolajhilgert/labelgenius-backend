using projectservice.Dto;

namespace projectservice.Services
{
    public interface IProjectService
    {
        Task<(bool Result, string Message)> CreateProject(ProjectDto dto);
        Task<(bool Result, string Message)> DeleteProject(string projectId, string userRequesting);
        Task<(bool Result, string Message)> DeleteUserFromProjects(string userEmail);
        Task<(bool Result, string Message)> UpdateProject(string projectId, ProjectDto dto);
    }
}
