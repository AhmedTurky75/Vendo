using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Vendo.Identity.Api.Pages.Account
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class LoggedOutModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
