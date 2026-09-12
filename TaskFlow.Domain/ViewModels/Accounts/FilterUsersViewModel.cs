using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Paging;

namespace TaskFlow.Domain.ViewModels.Accounts
{
    public class FilterUsersViewModel :RequestPage
    {
        public string? Search { get; set; }

 
    }
}
