using projectservice.Dto;

namespace projectservice.Services
{
    public interface IProjectInvitationService
    {
        Task<(bool Result, string Message)> CreateProjectInvite(ProjectInvitationDto invitationDto, string userEmail);
        Task<(bool Result, string Message)> ConsumeProjectInvite(string inviteToken);
    }
}
