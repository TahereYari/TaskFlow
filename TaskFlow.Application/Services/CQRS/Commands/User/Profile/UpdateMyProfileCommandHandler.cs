using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Constants;
using TaskFlow.Application.Utilities;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Application.Utilities.TaskFlow.Application.Utilities.Extensions;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.Models.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskFlow.Application.Services.CQRS.Commands.User.Profile
{
    public class UpdateMyProfileCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateMyProfileCommandHandler> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateMyProfileCommandHandler(
            IUserRepository userRepository,
            ILogger<UpdateMyProfileCommandHandler> logger,
            UserManager<ApplicationUser> userManager
            )
        {
            _userRepository = userRepository;
            _logger = logger;
            _userManager = userManager;
        }


        /// <summary>
        /// ویرایش پروفایل کاربر
        /// </summary>
        /// <param name="command"></param>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result> Handle(UpdateMyProfileCommand command, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
             
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return Result.Failure(
                        message: "شناسه کاربر معتبر نیست.");
                }

                var user =
                    await _userRepository.GetUserAsync(
                        userGuid,
                        cancellationToken);

                if (user == null)
                {
                    return Result.Failure(
                        message: " کاربر وجود ندارد.");
                }
                user.PhoneNumber = command.PhoneNumber;
                user.UserProfile.Name = command.Name;
                user.UserProfile.Family = command.Family;
           
                user.UserProfile.BirthDate = command.BirthDate.ToGregorianDate();
                if (command.ProfileImage != null)
                {
                    if (!string.IsNullOrWhiteSpace(user.UserProfile.ProfileImage))
                    {
                        FileUploader.DeleteFile(
                             user.UserProfile.ProfileImage,
                            "Profiles");
                    }

                    var fileName = await FileUploader.SaveFileAsync(
                        command.ProfileImage,
                        "Profiles");

                    user.UserProfile.ProfileImage = fileName;
                }
                var userResult =
                       await _userManager.UpdateAsync(user);

                if (!userResult.Succeeded)
                {
                    await _userRepository.RollbackTransactionAsync();

                    return Result.Failure(
                        "ویرایش کاربر انجام نشد.",
                        GetIdentityErrors(userResult));
                }
                await _userRepository.SaveChangesAsync(
                    cancellationToken);

                return Result.Success(
                    message: "پروفایل با موفقیت ویرایش شد.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در ویرایش پروفایل کاربر.");

                return Result.Failure(
                    message: ErrorMessages.UnknownError);
            }
        }

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
