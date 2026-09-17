using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.User;

namespace TaskFlow.Application.Services.CQRS.Query.User.GetUser
{
    public class GetUserQueryHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserQueryHandler> _logger;

        public GetUserQueryHandler(IUserRepository userRepository, ILogger<GetUserQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
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

                return Result.Success(message: " کاربر با موفقیت لود شد.", data: user);

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
