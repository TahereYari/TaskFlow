using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Services.CQRS.Query.User
{
    public class GetUserQuery
    {
        public Guid Id { get; set; }
    }
}
