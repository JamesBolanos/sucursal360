using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sucursal360.Web.Data;
using Sucursal360.Web.Models;
using Sucursal360.Web.Security;

namespace Sucursal360.Web.Controllers;

public class HomeController(UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.CorporateManager))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (User.IsInRole(AppRoles.BranchManager))
            {
                var applicationUser = await userManager.GetUserAsync(User);
                if (applicationUser?.AssignedBranchId is Guid assignedBranchId)
                {
                    return RedirectToAction("Details", "Branches", new { id = assignedBranchId });
                }
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
