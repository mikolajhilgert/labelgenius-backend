using Firebase.Auth;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using userservice.Dto;

namespace userservice.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IFirebaseAuthClient _firebaseAuthClient;
        private readonly Repositories.IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration config, IFirebaseAuthClient firebaseAuthClient, Repositories.IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _config = config;
            _firebaseAuthClient = firebaseAuthClient;
            _userRepository = userRepository;
            _logger = logger;
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

        public async Task<(bool Result, string Message)> ResetPassword(UserResetPasswordDto userDto)
        {
            try
            {
                await _firebaseAuthClient.ResetEmailPasswordAsync(userDto.Email);
                return (true, "Password reset email has been sent");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }

        private async Task SendVerificationEmailAsync(UserCredential firebaseUserCredential)
        {
            try
            {
                string apiKey = _config.GetSection("FirebaseSettings:apiKey").Value;
                string RequestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new { requestType = "VERIFY_EMAIL", idToken = await firebaseUserCredential.User.GetIdTokenAsync() };
                string jsonData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(RequestUri, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while making the HTTP request to send verification email.");
            }
        }
    }
}
