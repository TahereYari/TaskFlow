using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.Constants;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.Account;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.Paging;
using TaskFlow.Domain.ViewModels.RefreshToken;
using TaskFlow.Infra.Data.Context;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskFlow.Infra.Data.Repositories.Account
{
    public class AccountRepository : IAccountRepository
    {

        #region Constractor
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountRepository> _logger;
        private readonly JwtSettings _jwtSettings;
        public AccountRepository(ApplicationDBContext context, UserManager<ApplicationUser> userManager,
                              IConfiguration configuration, IOptions<JwtSettings> jwtSettings,ILogger<AccountRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _jwtSettings = jwtSettings.Value; 
        }

        #endregion


        #region افزودن RefreshToken
        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
            {
                await _context.RefreshTokens.AddAsync(refreshToken,cancellationToken);
            }
        #endregion


        #region جستجو RefreshToken
        public async Task<RefreshToken> FindRefreshToken(RefreshTokenModel model)
        {
              // Find refresh token
               return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == model.RefreshToken);
        }
        #endregion




        #region افزودن UserProfile
        public async Task AddUserProfileAsync(UserProfile profile)
        {
            await _context.UserProfiles.AddAsync(profile);
        }

        #endregion

        #region RevokeUserRefreshTokensAsync
        public async Task RevokeUserRefreshTokensAsync(Guid userId,CancellationToken cancellationToken = default)
        {
            var refreshTokens = await _context.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    x.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

          await SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region SaveChangesAsync
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        #endregion



        #region BeginTransactionAsync
        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }
        #endregion

        #region CommitTransactionAsync
        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }
        #endregion

        #region RollbackTransactionAsync
        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
        #endregion

    }
}
