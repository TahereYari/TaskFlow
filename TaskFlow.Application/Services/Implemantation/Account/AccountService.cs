using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Domain.IRepository;
using TaskFlow.Domain.ResultResponse;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.RefreshToken;

namespace TaskFlow.Application.Services.Implemantation.Account
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Result> GetUsersAsync(FilterUsersViewModel model)
        {
            return await _accountRepository.GetUsersAsync(model);
        }

        public Task<Result> Login(LoginModel model)
        {
           return _accountRepository.Login(model);
        }

        public async Task<Result> RefreshToken(RefreshTokenModel model)
        {
           return await _accountRepository.RefreshToken(model);
        }

        public async Task<Result> RegisterAsync(RegisterModel model)
        {
          return await  _accountRepository.RegisterAsync(model);
        }
    }
}
