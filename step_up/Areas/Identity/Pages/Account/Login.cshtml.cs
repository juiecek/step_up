// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using step_up.Models;

namespace step_up.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

       
        [BindProperty]
        public InputModel Input { get; set; }

       
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

      
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

           
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    _logger.LogWarning("Пользователь с таким Email не найден.");
                    ModelState.AddModelError("Input.Email", "Аккаунт с таким Email не найден.");
                    return Page();
                }

                if (await _signInManager.UserManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("Попытка входа в заблокированный аккаунт.");
                    ModelState.AddModelError(string.Empty, "Ваш аккаунт заблокирован.");
                    return Page();
                }

                if (!await _signInManager.UserManager.CheckPasswordAsync(user, Input.Password))
                {
                    _logger.LogWarning("Введён неверный пароль.");
                    ModelState.AddModelError("Input.Password", "Неверный пароль.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Пользователь успешно вошёл в систему.");

                    // Проверяем роли пользователя
                    var isAdmin = await _signInManager.UserManager.IsInRoleAsync(user, "Admin");
                    var isClient = await _signInManager.UserManager.IsInRoleAsync(user, "Client");

                    if (isAdmin)
                    {
                        return LocalRedirect("~/Schedules/Index"); // Администратор
                    }
                    else if (isClient)
                    {
                        return LocalRedirect("~/Identity/Account/Profile"); // Клиент
                    }

                    return LocalRedirect(returnUrl); // По умолчанию
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Аккаунт пользователя заблокирован.");
                    return RedirectToPage("./Lockout");
                }

                _logger.LogWarning("Ошибка входа.");
                ModelState.AddModelError(string.Empty, "Ошибка входа. Попробуйте ещё раз.");
            }

            return Page();
        }




    }
}
