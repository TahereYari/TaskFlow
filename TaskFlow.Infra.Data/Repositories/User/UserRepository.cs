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


        #region GetUserAsync
            public async Task<ApplicationUser?> GetUserAsync(Guid id)
            {
                return await _context.Users.FindAsync(id);
            }

        #endregion



        #region GetUsersAsync

        public async Task<Result> GetUsersAsync(FilterUsersViewModel model)
        {
            try
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


                var result = new List<object>();

                foreach (var user in paging.Entities)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    result.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,

                        Name = user.UserProfile?.Name,
                        Family = user.UserProfile?.Family,

                        ProfileImage =
                            user.UserProfile?.ProfileImage,

                        Roles = roles
                    });
                }




                return Result.Success(
                    message: "لیست کاربران با موفقیت لود شد.",
                    data: new
                    {
                        Users = result,

                        Paging = paging.GetCurrentPaging()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در دریافت کاربران.");

                return Result.Failure(
                    message: ErrorMessages.UnknownError);
            }
        }

 

        Task<ApplicationUser?> IUserRepository.GetUsersAsync(FilterUsersViewModel model)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
