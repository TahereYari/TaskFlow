using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Models.Permissions
{
    public class RolePermission
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public int RoleId { get; set; }

        //public ICollection<Asp> MyProperty { get; set; }


    }
}
