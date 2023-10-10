using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using userservice.Dto;
using userservice.Services;

namespace userservice.Controllers
{
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IRegisterService registerService;
        public RegisterController(IRegisterService registerService)
        { this.registerService = registerService; }


        [HttpPost("/api/register")]
        public async Task<ActionResult<Tuple<bool, string>>> Register(UserRegisterDto userDto)
        {
            try
            {
                Tuple<bool, string> result = await registerService.RegisterUser(userDto);
                if (result.Item1)
                {
                    return Ok("Registration successful, please verify your email!");
                }
                return BadRequest(result.Item2);
            }
            catch (FirebaseAuthException ex)
            {
                switch (ex.Reason)
                {
                    case AuthErrorReason.EmailExists:
                        return BadRequest("A user with this email already exist. Maybe you've forgotten your password?");
                    case AuthErrorReason.WeakPassword:
                        return BadRequest("Password not strong enough.");
                    case AuthErrorReason.MissingPassword:
                        return BadRequest("Password missing.");
                    case AuthErrorReason.MissingEmail:
                        return BadRequest("Email missing");
                }
                return BadRequest(ex.Message);
            }
        }
    }
}
