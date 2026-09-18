using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.RefreshToken;

namespace TaskFlow.Domain.IRepository.Account
{
    public interface IAccountRepository
    {
  

        Task<RefreshToken> FindRefreshToken(RefreshTokenModel model);

        Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        Task AddUserProfileAsync(UserProfile profile);
        Task RevokeUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();




    }
}
