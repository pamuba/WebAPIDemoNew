using Microsoft.AspNetCore.Identity;

namespace WebAPIDemoNew.Models
{
	public class ApplicationUser : IdentityUser
	{
        public string Name { get; set; }
    }
}
