using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using step_up.Models;

namespace step_up.Services
{
    public class AttendanceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AttendanceBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.Now;

                    var missedRegistrations = await context.Registration
                        .Include(r => r.UserSubscription)
                        .ThenInclude(us => us.Subscription)
                        .Where(r =>
                            !r.Attended &&
                            r.Date.Date < now.Date)
                        .ToListAsync(stoppingToken);

                    foreach (var reg in missedRegistrations)
                    {
                        reg.Attended = true;

                        var userSub = reg.UserSubscription;
                        var sub = userSub.Subscription;

                        // Только если абонемент ограниченный (не безлимитный) и есть занятия
                        if (sub.NumberOfClasses > 0 && userSub.ClassesRemaining > 0)
                        {
                            userSub.ClassesRemaining--;
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }

                // Ждать до следующего запуска (например, 24 часа)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
