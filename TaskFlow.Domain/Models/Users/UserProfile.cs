using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Common;

namespace TaskFlow.Domain.Models.Users
{
    public class UserProfile : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public required string Name { get; set; }

        public required string Family { get; set; }

        public string? ProfileImage { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? BirthDate { get; set; }
    }
}
