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
    }

}
