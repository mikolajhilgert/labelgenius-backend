using projectservice.Dto;

namespace projectservice.Services
{
    public interface ILabelService
    {
        Task<(bool Result, string Message)> SaveImageLabels(List<ImageLabelsDTO> dto);
        Task<(bool Result, string Message)> DeleteProjectLabels(string projectId, string userEmail);
        Task<(bool Result, string Message)> DeleteAllUserLabels(string userEmail);
        Task<(bool Result, string Message, ImageLabelsDTO Labels)> GetLabelsByProjectAndImage(string projectId, string imageId, string userEmail);
        Task<(bool Result, string Message, List<ImageLabelsDTO> Labels)> GetLabelsByProject(string projectId, string userEmail);
        Task RemoveLabelsFromDeletedUsersProject(string userEmail);
    }
}
