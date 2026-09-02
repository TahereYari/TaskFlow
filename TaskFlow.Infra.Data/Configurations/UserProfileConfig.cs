using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Users;

namespace TaskFlow.Infra.Data.Configurations
{
    public class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200);

            builder.Property(x => x.Family)
                .HasMaxLength(200);

            builder.Property(x => x.UserId);

            builder.HasIndex(x => x.UserId)
                .IsUnique();
        }
    }
}
