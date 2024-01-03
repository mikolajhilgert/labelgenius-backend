using Azure.Messaging.ServiceBus;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using projectservice.Dto;
using projectservice.Models;
using projectservice.Utils;
using System.Text.Json;

namespace projectservice.Services
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly IProjectService _projectService;
        private readonly IMongoCollection<ProjectInvitation> _invites;
        private readonly IMongoCollection<Project> _projects;
        private readonly string _projectCollectionName = "Projects";
        private readonly string _projectInvitationCollectionName = "ProjectInvitations";

        public ProjectInvitationService(IConfiguration config, IProjectService projectservice, IMongoClient mongoClient)
        {
            var mongoDB = mongoClient.GetDatabase(config.GetSection("MongoDbSettings:DatabaseName").Value);
            _serviceBusClient = new ServiceBusClient(config.GetSection("ServiceBus:ConnectionString").Value);
            _serviceBusSender = _serviceBusClient.CreateSender(config.GetSection("ServiceBus:QueueName").Value);
            _invites = mongoDB.GetCollection<ProjectInvitation>(_projectInvitationCollectionName);
            _projects = mongoDB.GetCollection<Project>(_projectCollectionName);
            _projectService = projectservice;
        }

        public async Task<(bool Result, string Message)> CreateProjectInvite(ProjectInvitationDto invitationDto, string userEmail)
        {
            try
            {
                var (IsInProject, IsProjectCreator) = await _projectService.UserRoleInProject(invitationDto.ProjectId, userEmail);

                if (!IsProjectCreator)
                {
                    return (false, "You cannot invite users as you are not the creator of this project.");
                }
                else if (IsInProject)
                {
                    return (false, "This user is already in the project!");
                }

                (string generatedToken, string generatedSecret) = InvitationTokenUtil.CreateInvitationToken(invitationDto.InviteeEmail, invitationDto.ProjectId, userEmail);

                ProjectInvitation projectInvitation = new()
                {
                    ProjectId = invitationDto.ProjectId,
                    Invitee = invitationDto.InviteeEmail,
                    Sender = userEmail,
                    Secret = generatedSecret
                };

                // Add project invitation 
                await _invites.InsertOneAsync(projectInvitation);

                // Add Token field 
                JObject jsonObject = JObject.Parse(JsonSerializer.Serialize(projectInvitation));
                jsonObject["token"] = InvitationTokenUtil.Base64Encode(generatedToken);

                // Create a new ServiceBusMessage with the JSON string as its body
                ServiceBusMessage serviceBusMessage = new(jsonObject.ToString());

                // Send the message
                await _serviceBusSender.SendMessageAsync(serviceBusMessage);

                return (true, "Project invitation has been sent");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool Result, string Message)> ConsumeProjectInvite(string inviteToken)
        {
            try
            {
                // Decode and parse the invite token
                string baseDecodedToken = InvitationTokenUtil.Base64Decode(inviteToken);
                var (InviteeEmail, Sender, ProjectId, Secret) = InvitationTokenUtil.ParseInviteToken(baseDecodedToken);

                // Check if the user is already in the project
                var (IsInProject, _) = await _projectService.UserRoleInProject(ProjectId, InviteeEmail);
                if (IsInProject)
                {
                    return (false, "This user is already in the project");
                }

                // Filter as project invitation with invitee email
                var filter = Builders<ProjectInvitation>.Filter.Eq("invitee", InviteeEmail);

                // Add the user to the project
                var filter2 = Builders<Project>.Filter.Eq("_id", ObjectId.Parse(ProjectId));
                var update = Builders<Project>.Update.AddToSet(p => p.LabellingUsers, InviteeEmail);
                var result = await _projects.UpdateOneAsync(filter2, update);

                // Delete all outstanding invitations for user
                await _invites.DeleteManyAsync(filter);

                return (true, "User has been added as a project labeller");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
