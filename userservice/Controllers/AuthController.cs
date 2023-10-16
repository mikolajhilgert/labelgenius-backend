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
        public async Task<ActionResult<Tuple<bool, string>>> Register(UserRegisterDto userDto)
        {
            // Check if model is valid
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                Tuple<bool, string> result = await _authService.RegisterUser(userDto);
                if (result.Item1) return Ok("Registration successful, please verify your email!");
                return BadRequest(result.Item2);
            }
            catch (FirebaseAuthException ex)
            {
                switch (ex.Reason)
                {
                    case AuthErrorReason.EmailExists:
                        return BadRequest("A  user with this email already exists. Try logging in.");
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var firebaseAuth = await _authService.LoginUser(userDto);
            if (!firebaseAuth.Item1)
            {
                return BadRequest(firebaseAuth.Item2);
            }
            else
            {
                return Ok("accessToken: " + firebaseAuth.Item3.IdToken + "\n" + "refreshToken: " + firebaseAuth.Item3.RefreshToken);
            }
        }
    }
}
