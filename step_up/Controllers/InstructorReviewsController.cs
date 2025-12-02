using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using step_up.Models;

namespace step_up.Controllers
{
    [Authorize]
    public class InstructorReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public InstructorReviewsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: InstructorReviews
        public async Task<IActionResult> Index()
        {
            var reviews = _context.InstructorReviews
                .Include(r => r.Instructor)
                .Include(r => r.User);
            return View(await reviews.ToListAsync());
        }

        // GET: InstructorReviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.InstructorReviews
                .Include(r => r.Instructor)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // GET: InstructorReviews/Create
        public IActionResult Create()
        {
            ViewData["InstructorId"] = new SelectList(_context.Instructor, "Id", "FullName");
            return View();
        }

        // POST: InstructorReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstructorId,Rating,Comment")] InstructorReview instructorReview)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Проверка на повторный отзыв
            var alreadyExists = await _context.InstructorReviews.AnyAsync(r =>
                r.UserId == currentUserId && r.InstructorId == instructorReview.InstructorId);

            if (alreadyExists)
            {
                ModelState.AddModelError("", "Вы уже оставляли отзыв для этого преподавателя.");
            }

            if (ModelState.IsValid)
            {
                instructorReview.UserId = currentUserId!;
                instructorReview.CreatedAt = DateTime.Now;

                _context.Add(instructorReview);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["InstructorId"] = new SelectList(_context.Instructor, "Id", "FullName", instructorReview.InstructorId);
            return View(instructorReview);
        }

        // GET: InstructorReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.InstructorReviews.FindAsync(id);
            if (review == null) return NotFound();

            ViewData["InstructorId"] = new SelectList(_context.Instructor, "Id", "FullName", review.InstructorId);
            return View(review);
        }

        // POST: InstructorReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InstructorId,Rating,Comment")] InstructorReview instructorReview)
        {
            if (id != instructorReview.Id) return NotFound();

            var existingReview = await _context.InstructorReviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existingReview == null) return NotFound();

            instructorReview.UserId = existingReview.UserId;
            instructorReview.CreatedAt = existingReview.CreatedAt;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructorReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorReviewExists(instructorReview.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["InstructorId"] = new SelectList(_context.Instructor, "Id", "FullName", instructorReview.InstructorId);
            return View(instructorReview);
        }

        // GET: InstructorReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.InstructorReviews
                .Include(r => r.Instructor)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // POST: InstructorReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.InstructorReviews.FindAsync(id);
            if (review != null)
            {
                _context.InstructorReviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InstructorReviewExists(int id)
        {
            return _context.InstructorReviews.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromMainPage(int instructorId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });




            var review = new InstructorReview
            {
                InstructorId = instructorId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.InstructorReviews.Add(review);
            await _context.SaveChangesAsync();

            return LocalRedirect("~/Home/Teachers"); // возврат к этой же странице
        }

    }
}
