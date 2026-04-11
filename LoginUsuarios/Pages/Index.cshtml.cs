using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginUsuarios.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToPage("/Account/Login");
            return Page();
        }
    }
}
