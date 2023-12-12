namespace communicationservice.Services
{
    public interface IProjectEmailService
    {
        Task<bool> SendProjectUserInvitation(string targetUser, string projectOwner, string inviteToken, string projectName, string projectId);
    }
}
