using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using step_up.Models;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Halls> Hall { get; set; }
    public DbSet<Bookings> Booking { get; set; }
    public DbSet<Instructors> Instructor { get; set; }  // Переименовано в Instructor
    public DbSet<Schedules> Schedule { get; set; }
    public DbSet<Registration> Registration { get; set; }
    public DbSet<DanceStyle> DanceStyles { get; set; }
    public DbSet<ScheduleDanceStyle> ScheduleDanceStyles { get; set; } // Промежуточная таблица для связи
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<DanceStyleReview> DanceStyleReviews { get; set; }
    public DbSet<InstructorReview> InstructorReviews { get; set; }
    public DbSet<Level> Levels { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Instructors>()
     .HasOne(i => i.DanceStyle)
     .WithMany(ds => ds.Instructors)
     .HasForeignKey(i => i.DanceStyleId)
     .OnDelete(DeleteBehavior.Restrict);

        // Для ScheduleDanceStyle
        modelBuilder.Entity<ScheduleDanceStyle>()
            .HasKey(sds => sds.Id);  // Составной ключ

        // Связи для ScheduleDanceStyle
        modelBuilder.Entity<ScheduleDanceStyle>()
            .HasOne(sds => sds.Schedule)
            .WithMany(s => s.ScheduleDanceStyles)
            .HasForeignKey(sds => sds.ScheduleId);

        modelBuilder.Entity<ScheduleDanceStyle>()
            .HasOne(sds => sds.DanceStyle)
            .WithMany(ds => ds.ScheduleDanceStyles)
            .HasForeignKey(sds => sds.DanceStyleId);

        modelBuilder.Entity<ScheduleDanceStyle>()
      .HasOne(sds => sds.Level)
      .WithMany() // если ты не добавляешь навигационное свойство в Level (например: public ICollection<ScheduleDanceStyle> ScheduleDanceStyles)
      .HasForeignKey(sds => sds.LevelId);

        modelBuilder.Entity<ScheduleDanceStyle>()
    .HasOne(sds => sds.Level)
    .WithMany(l => l.ScheduleDanceStyles)
    .HasForeignKey(sds => sds.LevelId);


        // Связи и настройки для других сущностей
        modelBuilder.Entity<Bookings>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId);

        modelBuilder.Entity<Bookings>()
            .HasOne(b => b.Hall)
            .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HallId);

        modelBuilder.Entity<Schedules>()
            .HasOne(s => s.Instructor)
            .WithMany(i => i.Schedules)
            .HasForeignKey(s => s.InstructorId);

        modelBuilder.Entity<Schedules>()
            .HasOne(s => s.Hall)
            .WithMany(h => h.Schedules)
            .HasForeignKey(s => s.HallId);

        modelBuilder.Entity<Registration>()
            .HasOne(r => r.User)
            .WithMany(u => u.Registrations)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Registration>()
            .HasOne(r => r.Schedule)
            .WithMany(s => s.Registrations)
            .HasForeignKey(r => r.ScheduleId);

        modelBuilder.Entity<Bookings>()
            .Property(b => b.TotalPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Halls>()
            .Property(h => h.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Subscription>()
            .Property(s => s.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserSubscriptions)
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.Subscription)
            .WithMany(s => s.UserSubscriptions)
            .HasForeignKey(us => us.SubscriptionId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.CardNumber)
            .IsUnique();
    }

}
