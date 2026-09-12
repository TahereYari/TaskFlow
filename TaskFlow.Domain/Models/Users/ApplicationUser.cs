
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace TaskFlow.Domain.Models.Users
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public UserProfile UserProfile { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
