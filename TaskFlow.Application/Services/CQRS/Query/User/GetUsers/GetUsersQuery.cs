using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.ViewModels.Paging;

namespace TaskFlow.Application.Services.CQRS.Query.User.GetUsers
{
    public class GetUsersQuery : RequestPage
    {
        public string? Search { get; set; }
    }
}
