using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace step_up.Views.Home
{
    public class RecordModel : PageModel
    {

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Direction { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Message = "Заполните все поля корректно.";
                return Page();
            }
            Message = $"Спасибо, {Name}, за запись на направление {Direction}. Мы свяжемся с вами по адресу {Email}!";
            return Page();
        }
    }
}
