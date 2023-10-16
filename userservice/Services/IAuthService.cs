using Firebase.Auth;
using userservice.Dto;

namespace userservice.Services
{
    public interface IAuthService
    {
        Task<Tuple<bool, string>> RegisterUser(UserRegisterDto userDto);
        Task<Tuple<bool, string, FirebaseCredential>> LoginUser(UserLoginDto userDto);
    }
}
