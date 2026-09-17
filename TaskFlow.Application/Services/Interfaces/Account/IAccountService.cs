using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.RefreshToken;

namespace TaskFlow.Application.Services.Interfaces.Account
{
    public interface IAccountService
    {
        Task<Result> RegisterAsync(RegisterModel model);

        Task<Result> Login(LoginModel model,CancellationToken cancellationToken = default);



        Task<Result> RefreshToken(RefreshTokenModel model);
    }
}
