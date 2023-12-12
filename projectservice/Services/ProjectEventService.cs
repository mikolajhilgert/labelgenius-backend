using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using System.Text;
using projectservice.Models;
using Newtonsoft.Json;

namespace projectservice.Services
{
    public class ProjectEventService : BackgroundService
    {
        private readonly ILogger<ProjectEventService> _logger;
        private readonly IProjectService _projectService;
        private readonly ILabelService _labelService;
        private readonly IConfiguration _config;
        BlobContainerClient _blobContainerClient;
        EventProcessorClient _processor;

        public ProjectEventService(ILogger<ProjectEventService> logger, IServiceScopeFactory factory, IConfiguration config)
        {
            _config = config;
            _logger = logger;

            var blobConnString = _config.GetSection("EventHubStorage:ConnectionString").Value;
            var container = _config.GetSection("EventHubStorage:ContainerProjectEvents").Value;
            var consumerGroup = _config.GetSection("EventHubConfig:ConsumerGroupProjectEvents").Value;
            var eventHubConnectionString = _config.GetSection("EventHubConfig:EventHubConnectionString").Value;
            var eventHubName = _config.GetSection("EventHubConfig:EventHubName").Value;

            _projectService = factory.CreateScope().ServiceProvider.GetRequiredService<IProjectService>();
            _labelService = factory.CreateScope().ServiceProvider.GetRequiredService<ILabelService>();

            _blobContainerClient = new BlobContainerClient(blobConnString, container);
            _processor = new EventProcessorClient(_blobContainerClient, consumerGroup, eventHubConnectionString, eventHubName);
            _processor.ProcessEventAsync += Processor_ProcessEventAsync;
            _processor.ProcessErrorAsync += Processor_ProcessErrorAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await _processor.StartProcessingAsync();
            }
        }

        Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogCritical("Error: " + arg.Exception.ToString());
            return Task.CompletedTask;
        }

        async Task Processor_ProcessEventAsync(ProcessEventArgs arg)
        {
            try
            {
                _logger.LogInformation($"Event received from partition {arg.Partition.PartitionId}:{arg.Data.EventBody}");

                if (arg.Data.EventBody != null)
                {
                    var message = JsonConvert.DeserializeObject<ActionEvent>(value: Encoding.UTF8.GetString(arg.Data.EventBody));

                    if (message != null && !string.IsNullOrEmpty(message.DeleteUserEmail))
                    {
                        await ProcessMessage(message);
                    }
                }

                await arg.UpdateCheckpointAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event");
            }
        }

        private async Task ProcessMessage(ActionEvent message)
        {
            switch (message.EventType)
            {
                case "deleteuserdata":
                    await _projectService.DeleteUserFromAllProjects(message.DeleteUserEmail);
                    await _labelService.DeleteAllUserLabels(message.DeleteUserEmail);
                    await _labelService.RemoveLabelsFromDeletedUsersProject(message.DeleteUserEmail);
                    break;
                default:
                    _logger.LogWarning($"Unknown event type: {message.EventType}");
                    break;
            }
        }
    }
}
