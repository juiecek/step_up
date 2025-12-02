using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using step_up.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterModel(
        UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = string.Empty;

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Пароль должен содержать от {2} до {1} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/Identity/Account/Profile"); // Перенаправляем клиента в личный кабинет
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Этот Email уже используется.");
                return Page();
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            // Генерация случайного номера карты
            user.CardNumber = GenerateCardNumber();

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Новый пользователь зарегистрирован.");

                // Проверяем, есть ли роль "Client", и создаем её, если нет
                if (!await _roleManager.RoleExistsAsync("Client"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Client"));
                }

                // Назначаем новому пользователю роль "Client"
                await _userManager.AddToRoleAsync(user, "Client");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Ошибка при регистрации: {Error}", error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }

    private User CreateUser()
    {
        try
        {
            return Activator.CreateInstance<User>();
        }
        catch
        {
            throw new InvalidOperationException($"Не удалось создать экземпляр '{nameof(User)}'. " +
                $"Убедитесь, что '{nameof(User)}' не является абстрактным классом и имеет конструктор без параметров.");
        }
    }

    // Метод для генерации случайного номера карты из 5 цифр
    private string GenerateCardNumber()
    {
        var random = new Random();
        var cardNumber = random.Next(10000, 99999);  // Генерация случайного числа от 10000 до 99999
        return cardNumber.ToString();  // Возвращаем строку для CardNumber
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("Необходимо хранилище пользователей с поддержкой Email.");
        }
        return (IUserEmailStore<User>)_userStore;
    }
}
