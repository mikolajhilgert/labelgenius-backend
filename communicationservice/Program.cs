using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using communicationservice.Services;
using communicationservice.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

public class Program
{
    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();

        await using var serviceBusClient = new ServiceBusClient(config.GetSection("ServiceBus:ConnectionString").Value);
        ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(config.GetSection("ServiceBus:QueueName").Value);

        ProjectEmailService projectEmailService = new(config);

        logger.LogInformation("The CommunicationService is starting.");

        while (true)
        {
            try
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

                if (receivedMessage != null)
                {
                    ProjectInvitation message = JsonConvert.DeserializeObject<ProjectInvitation>(Encoding.UTF8.GetString(receivedMessage.Body));

                    if (message != null)
                    {
                        await projectEmailService.SendProjectUserInvitation(message.Invitee, message.Sender, message.Token, message.ProjectName, message.ProjectID);

                        logger.LogInformation($"Job id: {receivedMessage.MessageId} has been recieved");

                        await receiver.CompleteMessageAsync(receivedMessage);
                    }
                }
            }
            catch (NullReferenceException)
            {
                // Do nothing
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
