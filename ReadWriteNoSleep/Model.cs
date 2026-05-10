using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadWriteNoSleep
{
    // Role.cs
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }

    // AppUser.cs
    public class AppUser
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;  // хранить хэш!
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool IsFrozen { get; set; }

        public Role Role { get; set; } = null!;
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ReadingList> ReadingLists { get; set; } = new List<ReadingList>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<RoleRequest> RoleRequests { get; set; } = new List<RoleRequest>();
        public ICollection<UnfreezeRequest> UnfreezeRequests { get; set; } = new List<UnfreezeRequest>();
    }

    // Genre.cs
    public class Genre
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
    }

    // Book.cs
    public class Book
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverPath { get; set; }
        public string TextContent { get; set; } = null!;
        public bool IsFrozen { get; set; }

        public AppUser Author { get; set; } = null!;
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ReadingList> ReadingLists { get; set; } = new List<ReadingList>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<UnfreezeRequest> UnfreezeRequests { get; set; } = new List<UnfreezeRequest>();
    }

    // BookGenre.cs (связующая таблица — составной PK)
    public class BookGenre
    {
        public int BookId { get; set; }
        public int GenreId { get; set; }
        public Book Book { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }

    // Review.cs
    public class Review
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public string ReviewText { get; set; } = null!;
        public int Rating { get; set; }  // 1–10
        public DateTime CreatedAt { get; set; }

        public Book Book { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }

    // ReadingList.cs (составной PK)
    public class ReadingList
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string Section { get; set; } = null!; // "Заброшено" / "В планах" / "Читаю" / "Прочитано"

        public AppUser User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }

    // Complaint.cs
    public class Complaint
    {
        public int ComplaintId { get; set; }
        public int UserId { get; set; }
        public int? TargetBookId { get; set; }    // либо книга
        public int? TargetReviewId { get; set; }  // либо отзыв (CHECK в БД)
        public string Reason { get; set; } = null!;

        public AppUser User { get; set; } = null!;
        public Book? TargetBook { get; set; }
        public Review? TargetReview { get; set; }
    }

    // RoleRequest.cs
    public class RoleRequest
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public DateTime RequestDate { get; set; }

        public AppUser User { get; set; } = null!;
    }

    // UnfreezeRequest.cs
    public class UnfreezeRequest
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public int? TargetBookId { get; set; }   // null — значит заявка на разморозку аккаунта
        public bool IsAccountUnfreeze { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime RequestDate { get; set; }

        public AppUser User { get; set; } = null!;
        public Book? TargetBook { get; set; }
    }
}
