using Azure.Messaging.ServiceBus;
using communicationservice.Utils;
using Newtonsoft.Json;
using System.Text;

var builder = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

IConfiguration config = builder.Build();

// Create a LoggerFactory
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// Use the LoggerFactory to create a logger
var logger = loggerFactory.CreateLogger<Program>();

await using var serviceBusClient = new ServiceBusClient(config.GetSection("ServiceBus:ConnectionString").Value);
ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(config.GetSection("ServiceBus:QueueName").Value);

while (true)
{
    try
    {
        ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

        ProjectInvitation message = JsonConvert.DeserializeObject<ProjectInvitation>(Encoding.UTF8.GetString(receivedMessage.Body));

        logger.LogInformation(receivedMessage.Body.ToString() + " " + "has been received");

        await receiver.CompleteMessageAsync(receivedMessage);
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message);
    }
}
