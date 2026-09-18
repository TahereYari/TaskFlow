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
        Task<ApplicationUser?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BasePaging<ApplicationUser>> GetUsersAsync(FilterUsersViewModel model, CancellationToken cancellationToken);
        Task AddUserProfileAsync(UserProfile profile,CancellationToken cancellationToken = default);
        //Task UpdateUserProfileAsync(UserProfile profile,CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ToggleActiveUser();
        Task<UserProfile?> GetUserProfileAsync(Guid id, CancellationToken cancellationToken = default);

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}
