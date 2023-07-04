using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders.Physical;
using Pustok.Areas.Manage.ViewModels;
using Pustok.Models;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHubContext<PustokHub> _hubContext;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,RoleManager<IdentityRole> roleManager, IHubContext<PustokHub> hubContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _hubContext = hubContext;
        }
        //public async Task<IActionResult> CreateRoles()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));

        //    return Content("created");
        //}
        //public async Task<IActionResult> CreateAdmin()
        //{
        //    AppUser admin = new AppUser
        //    {
        //        FullName = "Super Admin",
        //        UserName = "SuperAdmin",
        //    };

        //    var result = await _userManager.CreateAsync(admin, "Admin123");

        //    await _userManager.AddToRoleAsync(admin, "SuperAdmin");


        //    return Content("yaradildi");
        //}

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel adminVM,string returnUrl=null)
        {
            AppUser admin = await _userManager.FindByNameAsync(adminVM.UserName);

            if(admin == null || !admin.IsAdmin)
            {
                ModelState.AddModelError("", "UserName or Password is incorrect");
                return View();
            }


            var result = await _signInManager.PasswordSignInAsync(admin, adminVM.Password, adminVM.RememberMe, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "UserName or Password is incorrect");
                return View();
            }

            await _hubContext.Clients.All.SendAsync("LoginInfo");


            return returnUrl == null ? RedirectToAction("index", "dashboard") : Redirect(returnUrl);
        }
    }
}
