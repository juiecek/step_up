using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using step_up.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace step_up.Areas.Identity.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileModel(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Email { get; private set; } = string.Empty; // Сделали публичным

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public List<UserSubscription> UserSubscriptions { get; set; } = new();
        public List<Registration> UserRegistrations { get; set; } = new();


        public class InputModel
        {
            [DataType(DataType.Password)]
            [Display(Name = "Новый пароль")]
            public string? NewPassword { get; set; }

            [Display(Name = "Полное имя")]
            public string? FullName { get; set; }

            [Display(Name = "Номер пользователя")]
            public string? UserNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            // Отображаем email
            Email = user.Email ?? "Нет данных";

            // Отображаем полное имя и номер
            Input.FullName = user.FullName ?? "Не указано";
            Input.UserNumber = user.CardNumber ?? "Не указан";

            // Загружаем абонементы пользователя
            UserSubscriptions = await _context.UserSubscriptions
                .Include(us => us.Subscription)
                .Where(us => us.UserId == user.Id)
                .OrderByDescending(us => us.PurchaseDate)
                .ToListAsync();

            // Загружаем регистрации пользователя
            UserRegistrations = await _context.Registration
      .Include(r => r.Schedule)
          .ThenInclude(s => s.Instructor)
      .Include(r => r.Schedule)
          .ThenInclude(s => s.ScheduleDanceStyles)
              .ThenInclude(sd => sd.DanceStyle)
      .Where(r => r.UserId == user.Id)
      //.OrderByDescending(r => r.Schedule.Date)
      .ToListAsync();

           



            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            // Обновление пароля
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                TempData["SuccessMessage"] = "Пароль успешно изменён"; // Уведомление об изменении пароля
            }

            // Обновление полного имени
            if (!string.IsNullOrEmpty(Input.FullName))
            {
                user.FullName = Input.FullName;
            }

            // Обновление номера
            if (!string.IsNullOrEmpty(Input.UserNumber))
            {
                user.CardNumber = Input.UserNumber;
            }

            // Сохранение изменений в пользователя
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            return RedirectToPage();
        }
    }
}
