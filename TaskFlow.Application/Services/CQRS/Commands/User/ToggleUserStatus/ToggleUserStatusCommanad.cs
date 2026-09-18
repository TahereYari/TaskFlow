using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Services.CQRS.Commands.User.ToggleUserStatus
{
    public class ToggleUserStatusCommanad
    {
        public Guid UserId { get; set; }
    }
}
