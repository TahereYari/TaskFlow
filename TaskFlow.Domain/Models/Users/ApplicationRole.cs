
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Models.Users
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public required string DisplayName { get; set; }
    }
}
