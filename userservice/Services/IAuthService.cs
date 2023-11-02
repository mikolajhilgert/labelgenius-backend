using Firebase.Auth;
using userservice.Dto;

namespace userservice.Services
{
    public interface IAuthService
    {
        Task<(bool Result, string Message)> RegisterUser(UserRegisterDto userDto);
        Task<(bool Result, string Message, FirebaseCredential Credential)> LoginUser(UserLoginDto userDto);
        Task<(bool Result, string Message)> ResetPassword(UserResetPasswordDto userDto);
    }
}
