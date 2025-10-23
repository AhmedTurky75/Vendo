using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Vendo.Identity.Domain.Repositories;
using Vendo.Identity.Application.Common.Interfaces;

namespace Vendo.Identity.Api.Pages.Account
{
    [AllowAnonymous]
    [SecurityHeaders]
    public class LoginModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IIdentityProviderStore _identityProviderStore;

        public LoginModel(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IIdentityServerInteractionService interaction,
            IEventService events,
            IAuthenticationSchemeProvider schemeProvider,
            IIdentityProviderStore identityProviderStore)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _interaction = interaction;
            _events = events;
            _schemeProvider = schemeProvider;
            _identityProviderStore = identityProviderStore;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberLogin { get; set; }

        [BindProperty]
        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(ReturnUrl);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(ReturnUrl);

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please provide valid credentials";
                return Page();
            }

            // Validate credentials
            var user = await _userRepository.GetByEmailAsync(Username);
            if (user == null)
            {
                // Try by username if email lookup failed
                user = await _userRepository.GetByUsernameAsync(Username);
            }

            if (user == null || !_passwordHasher.VerifyPassword(Password, user.PasswordHash))
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(Username, "invalid credentials"));
                ErrorMessage = "Invalid username or password";
                return Page();
            }

            if (!user.IsActive)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(Username, "user is inactive"));
                ErrorMessage = "Your account has been deactivated. Please contact support.";
                return Page();
            }

            // Issue authentication cookie with claims
            var claims = new List<Claim>
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("email", user.Email.Value),
                new Claim("given_name", user.FirstName ?? string.Empty),
                new Claim("family_name", user.LastName ?? string.Empty)
            };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim("role", role));
            }

            // Add tenant claim if available
            if (user.TenantId.HasValue)
            {
                claims.Add(new Claim("tenant_id", user.TenantId.Value.ToString()));
            }

            var isuser = new IdentityServerUser(user.Id.ToString())
            {
                DisplayName = user.Username,
                AdditionalClaims = claims
            };

            await HttpContext.SignInAsync(isuser, new AuthenticationProperties
            {
                IsPersistent = RememberLogin,
                ExpiresUtc = RememberLogin ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
            });

            await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.Id.ToString(), user.Username));

            if (context != null)
            {
                // We can trust ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(ReturnUrl!);
            }

            // Request for a local page
            if (Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            else if (string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect("~/");
            }
            else
            {
                // User might have clicked on a malicious link - should be logged
                throw new InvalidOperationException("Invalid return URL");
            }
        }
    }

    /// <summary>
    /// This attribute sets security headers for the page
    /// </summary>
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var result = context.Result;
            if (result is ViewResult)
            {
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                var csp = "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'";
                if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("Content-Security-Policy", csp);
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
                {
                    context.HttpContext.Response.Headers.Add("Referrer-Policy", "no-referrer");
                }
            }
        }
    }
}
