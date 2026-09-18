using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.Models.Users;

using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.Paging;
using TaskFlow.Infra.Data.Context;
using TaskFlow.Infra.Data.Repositories.Account;

namespace TaskFlow.Infra.Data.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        #region Constractor
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;
        private readonly JwtSettings _jwtSettings;
        public UserRepository(ApplicationDBContext context, UserManager<ApplicationUser> userManager,
                              IConfiguration configuration, IOptions<JwtSettings> jwtSettings, ILogger<UserRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        #endregion


        #region نمایش کاربر
       
            public async Task<ApplicationUser?> GetUserAsync(Guid id, CancellationToken cancellationToken)
            {
                return await _context.Users
                                .Include(u => u.UserProfile)
                                .FirstOrDefaultAsync(
                                    u => u.Id == id,
                                    cancellationToken); 
            }

        #endregion


        #region لیست کاربران
          public async Task<BasePaging<ApplicationUser>> GetUsersAsync(FilterUsersViewModel model, CancellationToken cancellationToken = default)
        {
            var usersQuery = _userManager.Users
                .AsNoTracking()
                .Include(x => x.UserProfile)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                model.Search = model.Search.Trim();

                usersQuery = usersQuery.Where(x =>
                    (x.UserName != null &&
                     x.UserName.Contains(model.Search)) ||

                    (x.Email != null &&
                     x.Email.Contains(model.Search)) ||

                    (x.UserProfile != null &&
                     x.UserProfile.Name.Contains(model.Search)) ||

                    (x.UserProfile != null &&
                     x.UserProfile.Family.Contains(model.Search))
                );
            }

            // Order
            usersQuery = usersQuery
                .OrderByDescending(x => x.Id);

            // Pagination
            var paging = new BasePaging<ApplicationUser>
            {
                PageId = model.PageId,
                TakeEntity = model.TakeEntity
            };

            await paging.Paging(usersQuery);

            return paging;
        }

        #endregion


        #region افزودن کاربر جدید
        public async Task AddUserProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
        {
            await _context.UserProfiles.AddAsync( profile,cancellationToken);
        }
        #endregion

        #region SaveChangesAsync
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        #endregion



        #region قعال/ غیر فعال کردن کاربر
        public Task ToggleActiveUser()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ویرایش پروفایل کاربر


        //public Task UpdateUserProfile()
        //{
        //    throw new NotImplementedException();
        //}
        #endregion


        #region اطلاعات پروفایل کاربر
        public async Task<UserProfile?> GetUserProfileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                        .FirstOrDefaultAsync(x => x.UserId == id,cancellationToken);
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
