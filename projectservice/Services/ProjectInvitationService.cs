using Azure.Messaging.ServiceBus;
using projectservice.Dto;
using System.Text.Json;

namespace projectservice.Services
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;

        public ProjectInvitationService(IConfiguration config)
        {
            _serviceBusClient = new ServiceBusClient(config.GetSection("ServiceBus:ConnectionString").Value);
            _serviceBusSender = _serviceBusClient.CreateSender(config.GetSection("ServiceBus:QueueName").Value);
        }

        public async Task<(bool Result, string Message)> CreateProjectInvite(ProjectInvitationDto invitationDto)
        {
            try
            {
                // Convert the invitationDto to a JSON string
                string jsonString = JsonSerializer.Serialize(invitationDto);

                // Create a new ServiceBusMessage with the JSON string as its body
                ServiceBusMessage serviceBusMessage = new(jsonString);

                // Send the message
                await _serviceBusSender.SendMessageAsync(serviceBusMessage);

                return (true, "Message sent successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
