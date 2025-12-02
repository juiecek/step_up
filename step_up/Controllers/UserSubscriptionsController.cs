using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using step_up.Models;

namespace step_up.Controllers
{
    public class UserSubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserSubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserSubscriptions
        public async Task<IActionResult> Index()
        {
            var userSubscriptions = _context.UserSubscriptions
                .Include(u => u.Subscription)
                .Include(u => u.User);
            return View(await userSubscriptions.ToListAsync());
        }

        // GET: UserSubscriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.UserSubscriptions
                .Include(u => u.User)
                .Include(u => u.Subscription)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }


        // GET: UserSubscriptions/Create
        //public IActionResult Create()
        //{
        //    ViewBag.Users = _context.User
        //        .Select(u => new SelectListItem
        //        {
        //            Value = u.Id,
        //            Text = u.CardNumber
        //        })
        //        .ToList();

        //    ViewBag.Subscriptions = _context.Subscriptions
        //        .Select(s => new SelectListItem
        //        {
        //            Value = s.Id.ToString(),
        //            Text = s.Name
        //        })
        //        .ToList();

        //    return View();
        //}
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_context.User, "Id", "CardNumber");
            ViewBag.Subscriptions = new SelectList(_context.Subscriptions, "Id", "Name");
            return View();
        }

        // POST: UserSubscriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string UserId, int SubscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(SubscriptionId);
            if (subscription == null || string.IsNullOrEmpty(UserId))
            {
                return BadRequest("Пользователь или абонемент не найден");
            }

            var purchaseDate = DateTime.Now;
            var expiryDate = purchaseDate.AddDays(subscription.DurationInDays);
            var classesRemaining = subscription.NumberOfClasses;

            var userSubscription = new UserSubscription
            {
                UserId = UserId,
                SubscriptionId = SubscriptionId,
                PurchaseDate = purchaseDate,
                ExpiryDate = expiryDate,
                ClassesRemaining = classesRemaining
            };

            _context.UserSubscriptions.Add(userSubscription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserSubscriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.UserSubscriptions.FindAsync(id);
            if (subscription == null)
                return NotFound();

            ViewBag.Users = new SelectList(_context.User, "Id", "CardNumber", subscription.UserId);
            ViewBag.Subscriptions = new SelectList(_context.Subscriptions, "Id", "Name", subscription.SubscriptionId);
            return View(subscription);
        }


        // POST: UserSubscriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserSubscription userSubscription)
        {
            if (id != userSubscription.Id) return NotFound();

            var subscription = await _context.Subscriptions.FindAsync(userSubscription.SubscriptionId);
            if (subscription == null)
            {
                ModelState.AddModelError("", "Абонемент не найден");
                return View(userSubscription);
            }

            userSubscription.ExpiryDate = userSubscription.PurchaseDate.AddDays(subscription.DurationInDays);
            userSubscription.ClassesRemaining = subscription.NumberOfClasses;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userSubscription);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserSubscriptions.Any(e => e.Id == userSubscription.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Subscriptions = new SelectList(_context.Subscriptions, "Id", "Name", userSubscription.SubscriptionId);
            ViewBag.Users = new SelectList(_context.User, "Id", "CardNumber", userSubscription.UserId);
            return View(userSubscription);
        }


        // GET: UserSubscriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userSubscription = await _context.UserSubscriptions
                .Include(us => us.User)
                .Include(us => us.Subscription)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSubscription == null) return NotFound();

            return View(userSubscription);
        }

        // POST: UserSubscriptions/Delete/5
         [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userSub = await _context.UserSubscriptions.FindAsync(id);
            if (userSub != null)
            {
                _context.UserSubscriptions.Remove(userSub);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserSubscriptionExists(int id)
        {
            return _context.UserSubscriptions.Any(e => e.Id == id);
        }
    }
}
