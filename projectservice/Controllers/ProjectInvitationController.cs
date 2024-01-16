using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projectservice.Dto;
using projectservice.Services;
using System.Security.Claims;

namespace projectservice.Controllers
{
    [Route("api/project/invite")]
    [ApiController]
    public class ProjectInvitationController : ControllerBase
    {
        private readonly IProjectInvitationService _projectInvitationService;

        public ProjectInvitationController(IProjectInvitationService projectInvitationService)
        {
            _projectInvitationService = projectInvitationService;
        }

        [Authorize(Roles = "User")]
        [HttpPost()]
        public async Task<IActionResult> InviteToProject(ProjectInvitationDto inviteDto)
        {
            try
            {
                // Retrieve the email claim from the user's claims
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");

                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }

                var result = await _projectInvitationService.CreateProjectInvite(inviteDto, emailClaim.Value);
                if (result.Result)
                {
                    return Ok(result.Message);
                }
                else
                {
                    return BadRequest(result.Message);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("join")]
        public async Task<IActionResult> JoinProject([FromQuery] string token)
        {
            try
            {
                var result = await _projectInvitationService.ConsumeProjectInvite(token);
                if (result.Result)
                {
                    return Redirect($"https://labelgenius.vercel.app/login");
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
