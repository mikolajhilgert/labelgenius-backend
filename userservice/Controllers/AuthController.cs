using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using userservice.Dto;
using userservice.Services;

namespace userservice.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        { this._authService = authService; }

        [HttpPost("/api/auth/register")]
        public async Task<ActionResult<(bool, string)>> Register(UserRegisterDto userDto)
        {
            // Check if model is valid
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterUser(userDto);
                if (result.Result) return Ok("Registration successful, please verify your email!");
                return BadRequest(result.Message);
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

        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            // Check if model is valid
            // if (!ModelState.IsValid) return BadRequest(ModelState);

            var firebaseAuth = await _authService.LoginUser(userDto);
            if (!firebaseAuth.Result)
            {
                return BadRequest(firebaseAuth.Message);
            }
            else
            {
                var credential = firebaseAuth.Credential;
                return Ok("accessToken: " + credential.IdToken + "\n refreshToken: " + credential.RefreshToken);
            }
        }
    }
}
