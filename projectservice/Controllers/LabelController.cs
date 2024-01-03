using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projectservice.Dto;
using projectservice.Services;
using System.Security.Claims;

namespace projectservice.Controllers
{
    [Route("api/project/label")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        [Authorize(Roles = "User")]
        [HttpPost("save")]
        public async Task<IActionResult> SaveImageLabels(List<ImageLabelsDTO> dto)
        {
            try
            {
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }
                foreach (var item in dto)
                {
                    item.Creator = emailClaim.Value;
                }
                // Manually set creator of project
                var (Result, Message) = await _labelService.SaveImageLabels(dto);
                if (Result)
                {
                    return Ok(Message);
                }
                else
                {
                    return BadRequest(Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProjectLabels(string projectId)
        {
            try
            {
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }
                var (Result, Message) = await _labelService.DeleteProjectLabels(projectId, emailClaim.Value);
                if (Result)
                {
                    return Ok(Message);
                }
                else
                {
                    return BadRequest(Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAllUserLabels()
        {
            try
            {
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }
                var (Result, Message) = await _labelService.DeleteAllUserLabels(emailClaim.Value);
                if (Result)
                {
                    return Ok(Message);
                }
                else
                {
                    return BadRequest(Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetLabelsByProject(string projectId)
        {
            try
            {
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }
                var (Result, Message, Labels) = await _labelService.GetLabelsByProject(projectId, emailClaim.Value);
                if (Result)
                {
                    return Ok(Labels);
                }
                else
                {
                    // No label for this exists yet
                    return BadRequest(Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("getAllInProject")]
        public async Task<IActionResult> GetAllLabelsByProject(string projectId)
        {
            try
            {
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }
                var (Result, Message, Labels) = await _labelService.GetAllLabelsByProject(projectId, emailClaim.Value);
                if (Result)
                {
                    return Ok(Labels);
                }
                else
                {
                    // No label for this exists yet
                    return BadRequest(Message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
