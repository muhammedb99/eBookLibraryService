using Microsoft.EntityFrameworkCore;
using eBookLibraryService.Models;

namespace eBookLibraryService.Data
{
    public class eBookLibraryServiceContext : DbContext
    {
        public eBookLibraryServiceContext(DbContextOptions<eBookLibraryServiceContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<WaitingListEntry> WaitingListEntries { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks { get; set; }

        public DbSet<Cart> Carts { get; set; }

    }
}
