﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System.Security.Claims;

namespace Pustok.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly PustokDbContext _context;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,PustokDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(MemberRegisterViewModel memberVM)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = new AppUser
            {
                FullName = memberVM.FullName,
                Email = memberVM.Email,
                UserName = memberVM.UserName,
                PhoneNumber = memberVM.Phone,
                IsAdmin = false
            };

            var result = await _userManager.CreateAsync(user, memberVM.Password); 

            if(!result.Succeeded)
            {
                foreach (var item in result.Errors)
                    ModelState.AddModelError("", item.Description);

                return View();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(MemberLoginViewModel memberVM, string returnUrl=null)
        {
            if(!ModelState.IsValid) return View();

            AppUser member = await _userManager.FindByNameAsync(memberVM.UserName);

            if (member == null || member.IsAdmin)
            {
                ModelState.AddModelError("", "UserName or Password incorrect!");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(member, memberVM.Password,false,true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "USer is locked out for 15 seconds");
                return View();
            }
            else if (!result.Succeeded)
            {
                ModelState.AddModelError("", "UserName or Password incorrect!");
                return View();
            }

            return returnUrl==null?RedirectToAction("index", "home"):Redirect(returnUrl);
        }

        [Authorize(Roles="Member")]
        public async Task<IActionResult> Profile(string tab="Profile")
        {
            ViewBag.Tab = tab;
            AppUser member = await _userManager.FindByNameAsync(User.Identity.Name);

            ProfileViewModel vm = new ProfileViewModel
            {
                Member = new MemberUpdateViewModel
                {
                    FullName = member.FullName,
                    Email = member.Email,
                    Phone = member.PhoneNumber,
                    UserName= member.UserName,
                },
                Orders = _context.Orders.Include(x=>x.OrderItems).Where(x=>x.AppUserId == member.Id).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles ="Member")]
        public async Task<IActionResult> MemberUpdate(MemberUpdateViewModel memberVM)
        {
            if(!ModelState.IsValid)
            {
                ProfileViewModel vm = new ProfileViewModel { Member = memberVM };
                return View("Profile",vm);
            }

            AppUser member = await _userManager.FindByNameAsync(User.Identity.Name);

            member.FullName= memberVM.FullName;
            member.Email= memberVM.Email;
            member.PhoneNumber = memberVM.Phone;
            member.UserName = memberVM.UserName;

            var result = await _userManager.UpdateAsync(member);

            if(!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
                ProfileViewModel vm = new ProfileViewModel { Member = memberVM };
                return View("Profile", vm);
            }

            //password update

            await _signInManager.SignInAsync(member, false);

            return RedirectToAction("profile");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{
            AppUser user = await _userManager.FindByEmailAsync(email);

            if (user == null) return View("error");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = Url.Action("verifytoken","account", new { email = email,token=token }, Request.Scheme);

            return Json(new
            {
                url = url
            });
		}

        public async Task<IActionResult> VerifyToken(string email,string token)
        {
			AppUser user = await _userManager.FindByEmailAsync(email);

			if(await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token))
            {
                TempData["Email"] = email;
                TempData["Token"] = token;
                return RedirectToAction("ResetPassword");
            }

            return View("error");
        }

        public async Task<IActionResult> ResetPassword()
        {
			return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
			AppUser user = await _userManager.FindByEmailAsync(resetPassword.Email);

            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);

            if (!result.Succeeded)
                return View("error");


			return RedirectToAction("login");
        }

	}
}
