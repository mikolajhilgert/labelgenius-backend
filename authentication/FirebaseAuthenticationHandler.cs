using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace authentication
{
    public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly FirebaseApp _firebaseApp;

        public FirebaseAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _firebaseApp = (FirebaseApp.DefaultInstance == null) ? FirebaseApp.Create() : FirebaseApp.DefaultInstance;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token;
            bool isCookieAuth = TryRetrieveTokenFromCookie(out token);

            if (!isCookieAuth)
            {
                if (!TryRetrieveTokenFromHeader(out token))
                {
                    return AuthenticateResult.NoResult();
                }
            }

            return await AuthenticateFirebaseToken(token);
        }

        private bool TryRetrieveTokenFromCookie(out string token)
        {
            if (Context.Request.Cookies.TryGetValue("token", out token) && token != null)
            {
                return true;
            }
            return false;
        }

        private bool TryRetrieveTokenFromHeader(out string token)
        {
            token = null;
            if (!Context.Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                return false;
            }

            var bearerToken = headerValue.ToString();
            if (string.IsNullOrEmpty(bearerToken) || !bearerToken.StartsWith("Bearer "))
            {
                return false;
            }

            token = bearerToken.Substring("Bearer ".Length);
            return true;
        }

        private async Task<AuthenticateResult> AuthenticateFirebaseToken(string token)
        {
            try
            {
                var firebaseToken = await FirebaseAuth.GetAuth(_firebaseApp).VerifyIdTokenAsync(token);
                var claims = ToClaims(firebaseToken.Claims);
                var claimsIdentity = new ClaimsIdentity(claims, nameof(FirebaseAuthenticationHandler));
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authTicket = new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme);
                return AuthenticateResult.Success(authTicket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        private IEnumerable<Claim> ToClaims(IReadOnlyDictionary<string, object> claims)
        {
            return new List<Claim>
            {
                new Claim("id", claims["user_id"].ToString()),
                new Claim("email", claims["email"].ToString()),
                new Claim(ClaimTypes.Role, "User")
            };
        }
    }
}
