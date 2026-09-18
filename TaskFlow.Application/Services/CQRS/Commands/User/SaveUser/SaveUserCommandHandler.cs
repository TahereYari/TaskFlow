using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Constants;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.Account;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.Models.Users;

namespace TaskFlow.Application.Services.CQRS.Commands.User.CreateUser
{


    public class SaveUserCommandHandler
    {

        #region  #region Constractor
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SaveUserCommandHandler> _logger;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public SaveUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            ILogger<SaveUserCommandHandler> logger,
             RoleManager<ApplicationRole> roleManager
            )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
            _roleManager = roleManager;
        }
        #endregion

        #region Handle
        public async Task<Result> Handle(SaveUserCommand model,CancellationToken cancellationToken = default)
        {
            try
            {
                await _userRepository.BeginTransactionAsync();

      

                // بررسی Role
                var roleExists =
                    await _roleManager.RoleExistsAsync(model.Role);

                if (!roleExists)
                {
                    await _userRepository.RollbackTransactionAsync();

                    return Result.Failure(
                        message: $"نقش '{model.Role}' وجود ندارد.");
                }

          
                // افزودن
                

                if (!model.Id.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(model.Password))
                    {
                        await _userRepository.RollbackTransactionAsync();

                        return Result.Failure(
                            message: "رمز عبور الزامیست.");
                    }
                    var user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        IsActive = true
                    };

                    var userResult = await _userManager.CreateAsync(
                        user,
                        model.Password);

                    if (!userResult.Succeeded)
                    {
                        await _userRepository.RollbackTransactionAsync();

                        return Result.Failure(
                            "ایجاد کاربر انجام نشد.",
                            GetIdentityErrors(userResult));
                    }

                    // Role
                    var roleResult = await _userManager.AddToRoleAsync(
                        user,
                        model.Role);

                    if (!roleResult.Succeeded)
                    {
                        await _userRepository.RollbackTransactionAsync();

                        return Result.Failure(
                            "ایجاد نقش برای کاربر انجام نشد.",
                            GetIdentityErrors(roleResult));
                    }

                    // Profile
                    var profile = new UserProfile
                    {
                        UserId = user.Id,
                        Name = model.Name,
                        Family = model.Family,
                        CreateDate = DateTime.Now,
                        IsDeleted = false
                    };

                    await _userRepository.AddUserProfileAsync(
                        profile,
                        cancellationToken);
                }
                // ویرایش

                else
                {
                    var user = await _userManager.FindByIdAsync(
                        model.Id.Value.ToString());

                    if (user == null)
                    {
                        await _userRepository.RollbackTransactionAsync();

                        return Result.Failure(
                            message: "کاربر وجود ندارد.");
                    }

                    // ویرایش کاربر
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.IsActive = model.IsActive;

                    var userResult =
                        await _userManager.UpdateAsync(user);

                    if (!userResult.Succeeded)
                    {
                        await _userRepository.RollbackTransactionAsync();

                        return Result.Failure(
                            "ویرایش کاربر انجام نشد.",
                            GetIdentityErrors(userResult));
                    }

                    // Role فعلی
                    var currentRoles =
                        await _userManager.GetRolesAsync(user);

                    // تغییر Role
                    if (!currentRoles.Contains(model.Role))
                    {
                        if (currentRoles.Any())
                        {
                            var removeRoleResult =
                                await _userManager.RemoveFromRolesAsync(
                                    user,
                                    currentRoles);

                            if (!removeRoleResult.Succeeded)
                            {
                                await _userRepository.RollbackTransactionAsync();

                                return Result.Failure(
                                    "حذف نقش قبلی انجام نشد.",
                                    GetIdentityErrors(
                                        removeRoleResult));
                            }
                        }

                        var addRoleResult =
                            await _userManager.AddToRoleAsync(
                                user,
                                model.Role);

                        if (!addRoleResult.Succeeded)
                        {
                            await _userRepository.RollbackTransactionAsync();

                            return Result.Failure(
                                "افزودن نقش جدید انجام نشد.",
                                GetIdentityErrors(
                                    addRoleResult));
                        }
                    }

                    user.UserProfile.Name= model.Name;
                    user.UserProfile.Family = model.Family;
                }

                await _userRepository.SaveChangesAsync(
                    cancellationToken);

                await _userRepository.CommitTransactionAsync();

                return Result.Success(
                    message: model.Id.HasValue
                        ? "کاربر با موفقیت ویرایش شد."
                        : "کاربر با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "خطا در ایجاد یا ویرایش کاربر.");

                return Result.Failure(
                    message: ErrorMessages.UnknownError);
            }
        }

        #endregion


        #region Private Methode
        private List<string> GetIdentityErrors(IdentityResult result)
        {
            return result.Errors
                .Select(error =>
                    IdentityErrorMessages.GetMessage(
                        error.Code,
                        error.Description))
                .ToList();
        }

        #endregion
    }
}
