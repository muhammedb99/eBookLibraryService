using Microsoft.AspNetCore.Identity;

namespace eBookLibraryService.Models
{
    public class Users : IdentityUser
    {
        public string FullName {  get; set; }   
    }
}
