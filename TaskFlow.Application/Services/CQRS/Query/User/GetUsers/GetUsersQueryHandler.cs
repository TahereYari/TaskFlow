using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Services.CQRS.Query.User.GetUser;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Accounts;

namespace TaskFlow.Application.Services.CQRS.Query.User.GetUsers
{
    public class GetUsersQueryHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUsersQueryHandler> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public GetUsersQueryHandler(IUserRepository userRepository, 
                                    ILogger<GetUsersQueryHandler> logger,
                                     UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<Result> Handle(
       GetUsersQuery query,
       CancellationToken cancellationToken = default)
        {
            try
            {
                var filter = new FilterUsersViewModel
                {
                    Search = query.Search,
                    PageId = query.PageId,
                    TakeEntity = query.TakeEntity
                };
                var paging =
                    await _userRepository.GetUsersAsync(filter, cancellationToken);

                var result = new List<object>();

                foreach (var user in paging.Entities)
                {
                    var roles =
                        await _userManager.GetRolesAsync(user);

                    result.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,

                        Name = user.UserProfile?.Name,

                        Family = user.UserProfile?.Family,

                        ProfileImage =
                            user.UserProfile?.ProfileImage,
                            user.IsActive,

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
    }
}
