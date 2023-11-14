using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projectservice.Dto;
using projectservice.Services;
using System.Security.Claims;

namespace projectservice.Controllers
{
    [Route("api/project")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [Authorize(Roles = "User")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewProject([FromForm] ProjectDto projectDto)
        {
            try
            {
                // Check if model is valid
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Retrieve the email claim from the user's claims
                var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");

                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    return BadRequest("User not logged in or does not exist");
                }

                // Manually set creator from claims (so no forgery can happen)
                projectDto.ProjectCreator = emailClaim.Value;

                if (projectDto.FormFiles != null && projectDto.FormFiles.Count > 0) {
                    // Process the uploaded files
                    foreach (var file in projectDto.FormFiles)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        // Store the file in the Images property of your DTO
                        projectDto.Images.Add((file.FileName, memoryStream.ToArray()));
                    }
                    // Remove form files from DTO
                    projectDto.FormFiles = null;
                }
                else
                {
                    return BadRequest("Include atleast one image!");
                }

                var result = await _projectService.CreateProject(projectDto);

                if (!result.Result)
                {
                    return BadRequest("Cannot create new project!");
                }

                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
