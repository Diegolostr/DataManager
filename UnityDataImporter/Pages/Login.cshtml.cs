using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnityDataImporter.Data;

namespace UnityDataImporter.Pages;

public class LoginModel(AppDbContext db) : PageModel
{
    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == Username);

        if (user is null || user.Password is null || !BCrypt.Net.BCrypt.Verify(Password, user.Password))
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, user.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToPage("/Index");
    }
}
