using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using step_up.Controllers;
using step_up.Models;

namespace step_up.Views.Home
{
    public class HistoryModel : PageModel
    {
        private readonly ILogger<HistoryModel> _logger;

        public HistoryModel(ILogger<HistoryModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
        }
        public IActionResult Index()
        {
            return View();
        }

        private IActionResult View()
        {
            throw new NotImplementedException();
        }
    }
}
