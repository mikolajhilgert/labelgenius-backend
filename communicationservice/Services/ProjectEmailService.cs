using Azure;
using Azure.Communication.Email;

namespace communicationservice.Services
{
    public class ProjectEmailService : IProjectEmailService
    {
        private readonly EmailClient _emailClient;
        private readonly string _emailSenderAddress = "DoNotReply@b6078d3d-8ca5-4bbe-a839-9357e911c5e7.azurecomm.net";
        public ProjectEmailService(IConfiguration config)
        {
            _emailClient = new EmailClient(config.GetSection("AzureCommunication:ConnectionString").Value);
        }

        public async Task<bool> SendProjectUserInvitation(string targetUser, string projectOwner, string inviteToken, string projectName, string projectId)
        {
            var emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                _emailSenderAddress,
                targetUser,
                "\"<html><h1>Hello world via email.</h1l></html>\"",
                "Hello world via email.");

            return emailSendOperation.Value.Status.Equals(true);
        }
    }
}
