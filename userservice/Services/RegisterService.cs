using Firebase.Auth;
using Firebase.Auth.Providers;
using userservice.Dto;
using System.Net.Http.Headers;

namespace userservice.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IConfiguration config;
        private readonly FirebaseAuthClient firebaseAuthClient;
        private readonly Repositories.IUserRepository userRepository;

        public RegisterService(IConfiguration config, Repositories.IUserRepository userRepository)
        {
            this.config = config;
            this.userRepository = userRepository;

            var firebaseConfig = new FirebaseAuthConfig
            {
                ApiKey = config.GetSection("FirebaseSettings:apiKey").Value,
                AuthDomain = config.GetSection("FirebaseSettings:authDomain").Value,    
                Providers = new FirebaseAuthProvider[] { new EmailProvider() }
            };

            this.firebaseAuthClient = new FirebaseAuthClient(firebaseConfig);
        }

        public async Task<Tuple<bool, string>> RegisterUser(UserRegisterDto userDto)
        {
            UserCredential firebaseUserCredential = await firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(userDto.Email, userDto.Password);
            await SendVerificationEmailAsync(firebaseUserCredential);
            return await userRepository.SaveUser(userDto, firebaseUserCredential.User.Uid);
        }

        private async Task SendVerificationEmailAsync(UserCredential firebaseUserCredential)
        {
            string tokenId = await firebaseUserCredential.User.GetIdTokenAsync();
            string RequestUri = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + this.config.GetSection("FirebaseSettings:apiKey").Value;
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
