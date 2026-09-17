using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Accounts;

namespace TaskFlow.Domain.IRepository.User
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserAsync(Guid id);
        Task<ApplicationUser?> GetUsersAsync(FilterUsersViewModel model);
     
    }
}
