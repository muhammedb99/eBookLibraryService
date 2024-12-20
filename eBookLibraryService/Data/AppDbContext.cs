using eBookLibraryService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eBookLibraryService.Data
{
    public class AppDbContext : IdentityDbContext<Users> // Use the IdentityDbContext for Users
    {
        // Ensure the DbContextOptions is for AppDbContext specifically
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
