using Firebase.Auth;
using System.Net.Http.Headers;
using userservice.Dto;

namespace userservice.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IFirebaseAuthClient _firebaseAuthClient;
        private readonly Repositories.IUserRepository _userRepository;
        public AuthService(IConfiguration config, IFirebaseAuthClient firebaseAuthClient, Repositories.IUserRepository userRepository)
        {
            this._config = config;
            this._firebaseAuthClient = firebaseAuthClient;
            this._userRepository = userRepository;
        }
        public async Task<(bool, string, FirebaseCredential)> LoginUser(UserLoginDto userDto)
        {
            try
            {
                var auth = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(userDto.Email, userDto.Password);
                if (!auth.User.Info.IsEmailVerified)
                {
                    return (false, "Your email address has not been verified", new());
                }

                return (true, "Sign-in successful", auth.User.Credential);
            }
            catch (Exception e)
            {
                return (false, e.Message, new());
            }
        }

        public async Task<(bool Result, string Message)> RegisterUser(UserRegisterDto userDto)
        {
            UserCredential firebaseUserCredential = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(userDto.Email, userDto.Password);
            await SendVerificationEmailAsync(firebaseUserCredential);
            return await _userRepository.SaveUser(userDto, firebaseUserCredential.User.Uid);
        }

        private async Task SendVerificationEmailAsync(UserCredential firebaseUserCredential)
        {
            string tokenId = await firebaseUserCredential.User.GetIdTokenAsync();
            string RequestUri = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + _config.GetSection("FirebaseSettings:apiKey").Value;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent("{\"requestType\":\"VERIFY_EMAIL\",\"idToken\":\"" + tokenId + "\"}");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(RequestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
