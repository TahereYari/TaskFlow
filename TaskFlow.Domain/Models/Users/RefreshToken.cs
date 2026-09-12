using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Models.Users
{
    public class RefreshToken
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }

        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
