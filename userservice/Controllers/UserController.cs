using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using userservice.Services;

namespace userservice.Controllers
{
    [Route("api/me")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "User")]
        [HttpPost("delete")]
        public async Task<ActionResult<string>> DeleteSelf()
        {
            // Retrieve the email claim from the user's claims
            var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");

            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                return BadRequest("User not logged in or does not exist");
            }

            // Get the authentication token from the request headers
            if (!Request.Headers.TryGetValue("Cookie", out var cookies) ||
                !cookies.ToString().Contains("token="))
            {
                return BadRequest("Authentication token is missing in the request headers");
            }

            string authToken = cookies.ToString().Split("token=")[1];

            var deletionResult = await _userService.DeleteUser(emailClaim.Value, authToken);

            if (deletionResult.Result)
            {
                return Ok("User has been deleted successfully");
            }

            return BadRequest(deletionResult.Message);
        }
    }
}
