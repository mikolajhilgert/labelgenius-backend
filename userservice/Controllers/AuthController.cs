using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using userservice.Dto;
using userservice.Services;

namespace userservice.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {

        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        { _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<(bool, string)>> Register(UserRegisterDto userDto)
        {
            // Check if model is valid
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var (Result, Message) = await _authService.RegisterUser(userDto);
                if (Result) return Ok("Registration successful, please verify your email!");
                return BadRequest(Message);
            }
            catch (FirebaseAuthException ex)
            {
                switch (ex.Reason)
                {
                    case AuthErrorReason.EmailExists:
                        return BadRequest("A user with this email already exists. Please try logging in.");
                    case AuthErrorReason.WeakPassword:
                        return BadRequest("Password is not strong enough. Your password must be more than 6 characters.");
                }
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            // Check if model is valid
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (Result, Message, Credential) = await _authService.LoginUser(userDto);
            if (!Result)
            {
                return BadRequest(Message);
            }
            else
            {
                HttpContext.Response.Cookies.Append("token", Credential.IdToken,
                    new CookieOptions
                    {
                        Expires = DateTime.Now.AddMinutes(30),
                        HttpOnly = true,
                        Secure = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.None
                    });
                return Ok("You have successfully logged in!");
            }
        }

        [HttpPost("reset_password")]
        public async Task<IActionResult> ResetPassword(UserResetPasswordDto userDto)
        {
            // Check if model is valid
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (Result, Message) = await _authService.ResetPassword(userDto);
            if (!Result)
            {
                return BadRequest(Message);
            }
            else
            {
                return Ok("Password reset email has been sent!");
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("status")]
        public ActionResult<string> Status()
        {
            // Retrieve the email claim from the user's claims
            var emailClaim = (User.Identity as ClaimsIdentity)?.Claims.First(c => c.Type == "email");

            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                return BadRequest("User not logged in or does not exist");
            }
            return Ok("User is logged in");
        }

        [HttpPost("logout")]
        public ActionResult<string> Logout()
        {
            HttpContext.Response.Cookies.Delete("token");
            return Ok("You have successfully been logged out!");
        }
    }
}
