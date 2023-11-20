using projectservice.Dto;

namespace projectservice.Services
{
    public interface ILabelService
    {
        Task<(bool Result, string Message)> SaveLabel();
        Task<(bool Result, string Message)> UpdateLabel();
        Task<(bool Result, string Message)> DeleteLabel();
        Task<(bool Result, string Message)> GetLabel();
        Task<(bool Result, string Message)> DeleteAllUserLabels();
    }
}
