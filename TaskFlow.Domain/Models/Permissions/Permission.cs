using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Models.Permissions
{
    public class Permission
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public required string UniqueName { get; set; }
        public required string DisplayName { get; set; }

        public Permission? Parent { get; set; }
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
