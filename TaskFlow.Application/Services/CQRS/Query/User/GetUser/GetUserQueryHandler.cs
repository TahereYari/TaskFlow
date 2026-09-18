using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.Models.Users;

namespace TaskFlow.Application.Services.CQRS.Query.User.GetUser
{
    public class GetUserQueryHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserQueryHandler> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserQueryHandler(IUserRepository userRepository,
                                   ILogger<GetUserQueryHandler> logger,
                                   UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userManager = userManager;
        }
        #region GetAsync
        public async Task<Result> Handle(GetUserQuery query)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(query.Id);
                if (user == null)
                {
                    return Result.Failure(message: "کاربر وجود ندارد.");
                }
                var roles = await _userManager.GetRolesAsync(user);
                var result = new 
                {
                    user.Id,
                        user.UserName,
                        user.Email,

                        Name = user.UserProfile?.Name,
                        user.IsActive,
                        Family = user.UserProfile?.Family,

                        ProfileImage =
                            user.UserProfile?.ProfileImage,

                        Roles = roles
                };

                return Result.Success(message: " کاربر با موفقیت لود شد.", data: result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت کاربر.");
                return Result.Failure(message: ErrorMessages.UnknownError);
            }
        }
        #endregion
    }
}
