using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.Paging;

namespace TaskFlow.Domain.IRepository.User
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserAsync(Guid id);
        Task<BasePaging<ApplicationUser>> GetUsersAsync(FilterUsersViewModel model, CancellationToken cancellationToken);
     
    }
}
