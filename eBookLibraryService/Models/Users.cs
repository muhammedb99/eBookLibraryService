using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace eBookLibraryService.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public int CurrentBorrowedBooks { get; set; }
        public List<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
    }
}
