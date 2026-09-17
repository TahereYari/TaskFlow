using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Services.CQRS.Query.User;
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Application.Services.Interfaces.User;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.ViewModels.Accounts;

namespace TaskFlow.Api.Controllers
{
    public class UserController : AdminBaseController
    {
        #region Constractor


        private readonly GetUserQueryHandler _getUserHandler;
        //private readonly GetUsersQueryHandler _getUsersHandler;

        public UserController(
            GetUserQueryHandler getUserHandler
            //GetUsersQueryHandler getUsersHandler
            )
        {
            _getUserHandler = getUserHandler;
            //_getUsersHandler = getUsersHandler;
        }
        #endregion



        #region لیست کاربران
        /// <summary>
        /// لیست کاربران
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        //[HttpGet("GetUsersAsync")]
        //public async Task<IActionResult> GetUsersAsync([FromQuery] FilterUsersViewModel model)
        //{
        //    var users = await _userService.GetUsersAsync(model);
        //    return Ok(users);
        //}

        #endregion


        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query)
        {
            var result = await _getUserHandler.Handle(query);

            return Ok(result);
        }

    }
}
