using projectservice.Dto;

namespace projectservice.Services
{
    public interface IProjectService
    {
        Task<(bool Result, string Message)> CreateProject(CreateProjectDTO dto);
        Task<(bool Result, string Message)> DeleteProject(string projectId, string userEmail);
        Task<(bool Result, string Message)> DeleteUserFromAllProjects(string userEmail);
        Task<(bool Result, string Message)> UpdateProject(UpdateProjectDTO dto);
        Task<(bool Result, string Message, ResponseProjectDto Project)> GetProject(string projectId, string userEmail);
        Task<(bool Result, Dictionary<string, Dictionary<string, string>> Projects)> GetProjects(string userEmail);
        Task<(bool IsInProject, bool IsProjectCreator)> UserRoleInProject(string projectId, string userEmail);
    }
}
