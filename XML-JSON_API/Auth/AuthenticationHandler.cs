using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using XML_JSON_API.Models;

namespace XML_JSON_API.Auth
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly BasicAuthOptions _basicAuthOptions;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IOptions<BasicAuthOptions> basicAuthOptions)
            : base(options, logger, encoder)
        {
            _basicAuthOptions = basicAuthOptions.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);

                if (!"Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));
                }

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                if (credentials.Length != 2)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header Format"));
                }

                var providedUsername = credentials[0];
                var providedPassword = credentials[1];

                if (providedUsername != _basicAuthOptions.Username ||
                    providedPassword != _basicAuthOptions.Password)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
                }

                var claims = new[] { new Claim(ClaimTypes.Name, providedUsername) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"XML_JSON_API\"";
            return base.HandleChallengeAsync(properties);
        }
    }
}