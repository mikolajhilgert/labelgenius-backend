using Azure;
using Azure.Communication.Email;
using communicationservice.Utils;

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
            var replacements = new Dictionary<string, string>
            {
                { "project_name", projectName },
                { "project_link", "https://labelgenius.pics/api/project/invite/"+inviteToken }
            };

            var emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                _emailSenderAddress,
                targetUser,
                subject: "You have been invited to a LabelGenius project!",
                htmlContent: EmailTemplateProcessor.ProcessTemplate(replacements));

            return emailSendOperation.Value.Status.Equals(true);
        }
    }
}
