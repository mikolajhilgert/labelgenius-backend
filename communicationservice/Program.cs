using Azure.Messaging.ServiceBus;
using communicationservice.Services;
using communicationservice.Utils;
using Newtonsoft.Json;
using System.Text;

var builder = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

IConfiguration config = builder.Build();

var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();

await using var serviceBusClient = new ServiceBusClient(config.GetSection("ServiceBus:ConnectionString").Value);
ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(config.GetSection("ServiceBus:QueueName").Value);

ProjectEmailService projectEmailService = new ProjectEmailService(config);

logger.LogInformation("The CommunicationService is starting.");

while (true)
{
    try
    {
        ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

        ProjectInvitation message = JsonConvert.DeserializeObject<ProjectInvitation>(Encoding.UTF8.GetString(receivedMessage.Body));

        if (message != null)
        {
            await projectEmailService.SendProjectUserInvitation(message.Reciever, message.Sender, message.InviteToken, message.ProjectName, message.ProjectID);

            logger.LogInformation(receivedMessage.Body.ToString() + "" + "has been received");
        }

        await receiver.CompleteMessageAsync(receivedMessage);
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

