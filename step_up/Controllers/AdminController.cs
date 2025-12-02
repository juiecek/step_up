using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace step_up.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View(); // Страница управления пользователями
        }

        public IActionResult Bookings()
        {
            return View(); // Страница бронирований
        }
    }
}
