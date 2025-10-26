using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Vendo.Identity.Api.Pages.Account
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public LogoutModel(IIdentityServerInteractionService interaction, IEventService events)
        {
            _interaction = interaction;
            _events = events;
        }

        [BindProperty]
        public string? LogoutId { get; set; }

        public async Task<IActionResult> OnGetAsync(string? logoutId)
        {
            LogoutId = logoutId;

            var showLogoutPrompt = true;

            if (User?.Identity?.IsAuthenticated != true)
            {
                // If the user is not authenticated, then just show logged out page
                showLogoutPrompt = false;
            }
            else
            {
                var context = await _interaction.GetLogoutContextAsync(LogoutId);
                if (context?.ShowSignoutPrompt == false)
                {
                    // It's safe to automatically sign-out
                    showLogoutPrompt = false;
                }
            }

            if (!showLogoutPrompt)
            {
                // If the request for a logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await OnPostAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                // Delete local authentication cookie
                await HttpContext.SignOutAsync();

                // Raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // Check if we need to trigger sign-out at an upstream identity provider
            var logoutContext = await _interaction.GetLogoutContextAsync(LogoutId);

            string? postLogoutRedirectUri = logoutContext?.PostLogoutRedirectUri;

            if (!string.IsNullOrEmpty(postLogoutRedirectUri))
            {
                return Redirect(postLogoutRedirectUri);
            }

            return RedirectToPage("/Account/LoggedOut");
        }
    }
}
