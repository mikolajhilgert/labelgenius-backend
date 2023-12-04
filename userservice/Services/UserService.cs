using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using userservice.Models;

namespace userservice.Services
{

    public class UserService : IUserService
    {
        private readonly IConfiguration _config;
        private readonly Repositories.IUserRepository _userRepository;
        private readonly EventHubProducerClient _eventProducer;
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration config, Repositories.IUserRepository userRepository, ILogger<UserService> logger)
        {
            _config = config;
            _userRepository = userRepository;
            _eventProducer = new EventHubProducerClient(config.GetSection("EventHubConfig:EventHubConnectionString").Value,
                                                        config.GetSection("EventHubConfig:EventHubName").Value);
            _logger = logger;
        }

        public async Task<(bool Result, string Message)> DeleteUser(string email, string token)
        {
            // Remove user from database
            bool result = await _userRepository.DeleteUser(email);

            if (result)
            {
                try
                {
                    // Remove user from Firebase
                    await DeleteUserAsync(token);

                    // Publish deletion event to eventhub
                    var projectEvent = new ActionEvent { EventType = "deleteuserdata", DeleteUserEmail = email };
                    var emailEvent = new List<EventData> { new EventData(JsonConvert.SerializeObject(projectEvent)) };
                    await _eventProducer.SendAsync(emailEvent);

                    _logger.LogInformation("Deletion event published for user with email: {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing user and publishing deletion event for user with email: {Email}", email);
                }
            }
            else
            {
                return (false, "User could not be deleted");    
            }

            return (true, "User has been deleted succesfully");
        }

        private async Task DeleteUserAsync(string token)
        {
            try
            {
                string apiKey = _config.GetSection("FirebaseSettings:apiKey").Value;
                string RequestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={apiKey}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new { idToken = token };
                string jsonData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(RequestUri, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while making the HTTP request to delete the account.");
            }
        }
    }
}
