using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.ResultResponse;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.RefreshToken;

namespace TaskFlow.Domain.IRepository
{
    public interface IAccountRepository
    {
        Task<Result> RegisterAsync(RegisterModel model);
        Task<Result> Login(LoginModel model);
        Task<Result> UpdateAsync(RegisterModel model);

      
        Task<Result> GetAsync(int id);
        Task<Result> GetUsersAsync(FilterUsersViewModel model);
        Task<Result> RefreshToken(RefreshTokenModel model);
       

    }
}
