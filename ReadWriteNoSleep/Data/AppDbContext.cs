using Microsoft.EntityFrameworkCore;
using ReadWriteNoSleep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AppDbContext : DbContext
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookGenre> BookGenres { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReadingList> ReadingLists { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<RoleRequest> RoleRequests { get; set; }
    public DbSet<UnfreezeRequest> UnfreezeRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=.\SQLEXPRESS;Database=ReadWriteNoSleep;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().ToTable("AppUser");
        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<Book>().ToTable("Book");
        modelBuilder.Entity<Genre>().ToTable("Genre");
        modelBuilder.Entity<BookGenre>().ToTable("BookGenre");
        modelBuilder.Entity<Review>().ToTable("Review");
        modelBuilder.Entity<ReadingList>().ToTable("ReadingList");
        modelBuilder.Entity<Complaint>().ToTable("Complaint");
        modelBuilder.Entity<RoleRequest>().ToTable("RoleRequest");
        modelBuilder.Entity<UnfreezeRequest>().ToTable("UnfreezeRequest");

        // Явно указываем Primary Keys
        modelBuilder.Entity<AppUser>().HasKey(u => u.UserId);
        modelBuilder.Entity<Role>().HasKey(r => r.RoleId);
        modelBuilder.Entity<Book>().HasKey(b => b.BookId);
        modelBuilder.Entity<Genre>().HasKey(g => g.GenreId);
        modelBuilder.Entity<Review>().HasKey(r => r.ReviewId);
        modelBuilder.Entity<Complaint>().HasKey(c => c.ComplaintId);
        modelBuilder.Entity<RoleRequest>().HasKey(r => r.RequestId);
        modelBuilder.Entity<UnfreezeRequest>().HasKey(u => u.RequestId);

        // Составные PK
        modelBuilder.Entity<BookGenre>()
            .HasKey(bg => new { bg.BookId, bg.GenreId });

        modelBuilder.Entity<ReadingList>()
            .HasKey(rl => new { rl.UserId, rl.BookId });

        // CHECK ограничения
        modelBuilder.Entity<ReadingList>()
            .HasCheckConstraint("CK_ReadingList_Section",
                "[Section] IN (N'Прочитано', N'Читаю', N'В планах', N'Заброшено')");

        modelBuilder.Entity<Review>()
            .HasCheckConstraint("CK_Review_Rating",
                "[Rating] >= 1 AND [Rating] <= 10");

        modelBuilder.Entity<Complaint>()
            .HasCheckConstraint("CK_Complaint_Target",
                "([TargetBookId] IS NOT NULL AND [TargetReviewId] IS NULL) OR " +
                "([TargetBookId] IS NULL AND [TargetReviewId] IS NOT NULL)");
    }
}
