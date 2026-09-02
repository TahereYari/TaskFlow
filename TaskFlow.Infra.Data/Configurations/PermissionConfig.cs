using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Permissions;
using TaskFlow.Domain.Models.Users;

namespace TaskFlow.Infra.Data.Configurations
{
    public class PermissionConfig : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UniqueName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        }
    }
}
