using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Vendo.Identity.Api.Pages.Account
{
    [AllowAnonymous]
    [SecurityHeaders]
    public class ErrorModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;

        public ErrorModel(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        public ErrorViewModel? ErrorViewModel { get; set; }

        public async Task OnGetAsync(string errorId)
        {
            // Retrieve error details from IdentityServer
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                ErrorViewModel = new ErrorViewModel
                {
                    Error = message.Error,
                    ErrorDescription = message.ErrorDescription,
                    RequestId = errorId
                };
            }
        }
    }

    public class ErrorViewModel
    {
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public string? RequestId { get; set; }
    }
}
