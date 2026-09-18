using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.Account;
using TaskFlow.Domain.Models.Users;

namespace TaskFlow.Application.Services.CQRS.Commands.User.ToggleUserStatus
{


    public class ToggleUserStatusCommandHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<ToggleUserStatusCommandHandler> _logger;

        public ToggleUserStatusCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAccountRepository accountRepository,
            ILogger<ToggleUserStatusCommandHandler> logger)
        {
            _userManager = userManager;
            _accountRepository = accountRepository;
            _logger = logger;
        }


        /// <summary>
        /// تغییر وضعیت کاربر از فعال به غیر فعال و برعکس
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result> Handle(
            ToggleUserStatusCommanad command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _accountRepository.BeginTransactionAsync();

                // پیدا کردن کاربر بر اساس Id
                var user = await _userManager.FindByIdAsync(
                    command.UserId.ToString());

                if (user == null)
                {
                    await _accountRepository.RollbackTransactionAsync();

                    return Result.Failure(
                        message: "کاربر یافت نشد.");
                }

                // تغییر وضعیت کاربر
                user.IsActive = !user.IsActive;

                // تغییر SecurityStamp
                //
                // این کار باعث می‌شود SecurityStamp کاربر تغییر کند.
                // اگر اعتبارسنجی SecurityStamp در پروژه فعال باشد،
                // Token/Session قبلی می‌تواند بی‌اعتبار شود.
                await _userManager.UpdateSecurityStampAsync(user);

                // اگر کاربر غیرفعال شده است،
                // تمام RefreshTokenهای فعال او را باطل می‌کنیم.
                if (!user.IsActive)
                {
                    await _accountRepository.RevokeUserRefreshTokensAsync(
                        user.Id,
                        cancellationToken);
                }

                // ذخیره تغییرات کاربر
                var userResult = await _userManager.UpdateAsync(user);

                if (!userResult.Succeeded)
                {
                    await _accountRepository.RollbackTransactionAsync();

                    var errors = userResult.Errors
                        .Select(x => x.Description)
                        .ToList();

                    return Result.Failure(
                        message: "تغییر وضعیت کاربر انجام نشد.",
                        errors: errors);
                }

                // Commit کردن تمام تغییرات
                await _accountRepository.CommitTransactionAsync();

                var status = user.IsActive
                    ? "فعال"
                    : "غیرفعال";

                return Result.Success(
                    message: $"وضعیت کاربر با موفقیت به {status} تغییر کرد.",
                    data: status);
            }
            catch (Exception ex)
            {
                await _accountRepository.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "خطا در تغییر وضعیت فعال/غیرفعال کاربر.");

                return Result.Failure(
                    message: ErrorMessages.UnknownError);
            }
        }
    }

}

