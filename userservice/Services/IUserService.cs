using userservice.Dto;

namespace userservice.Services
{
    public interface IUserService
    {
        Task<(bool Result, string Message)> DeleteUser(string email, string token);
    }
}
