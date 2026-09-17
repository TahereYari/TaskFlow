using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Services.CQRS.Query.User.GetUser;
using TaskFlow.Application.Services.CQRS.Query.User.GetUsers;
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.ViewModels.Accounts;

namespace TaskFlow.Api.Controllers
{
    public class UserController : AdminBaseController
    {
        #region Constractor


        private readonly GetUserQueryHandler _getUserHandler;
        private readonly GetUsersQueryHandler _getUsersHandler;

        public UserController(
            GetUserQueryHandler getUserHandler,
            GetUsersQueryHandler getUsersHandler
            )
        {
            _getUserHandler = getUserHandler;
            _getUsersHandler = getUsersHandler;
        }
        #endregion



        #region لیست کاربران
        /// <summary>
        /// لیست کاربران
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            var users = await _getUsersHandler.Handle(query);
            return Ok(users);
        }

        #endregion


        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query)
        {
            var result = await _getUserHandler.Handle(query);

            return Ok(result);
        }

    }
}
