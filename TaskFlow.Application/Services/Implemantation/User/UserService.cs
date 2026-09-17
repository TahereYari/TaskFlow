using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Services.Interfaces.User;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.IRepository.Account;
using TaskFlow.Domain.IRepository.User;

using TaskFlow.Domain.ViewModels.Accounts;

namespace TaskFlow.Application.Services.Implemantation.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
          
        }


        #region GetUsersAsync
        //public async Task<Result> GetUsersAsync(FilterUsersViewModel model)
        //{
        //    return await _userRepository.GetUsersAsync(model);
        //}

        #endregion
    }
}
