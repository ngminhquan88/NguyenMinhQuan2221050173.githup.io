using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Controllers;

public class CultureController : Controller
{
    private static readonly HashSet<string> SupportedCultures = ["vi-VN", "en-US"];

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (!SupportedCultures.Contains(culture))
            return BadRequest();

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Students");
    }
}