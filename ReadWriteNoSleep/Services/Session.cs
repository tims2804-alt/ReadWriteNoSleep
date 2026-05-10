using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Services/Session.cs
namespace ReadWriteNoSleep.Services
{
    public static class Session
    {
        public static AppUser? CurrentUser { get; set; }

        public static bool IsAdmin =>
            CurrentUser?.Role?.RoleName == "Администратор";

        public static bool IsAuthor =>
            CurrentUser?.Role?.RoleName == "Автор";

        public static bool IsFrozen =>
            CurrentUser?.IsFrozen == true;
    }
}
