using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Services.CQRS.Commands.User.CreateUser;
using TaskFlow.Application.Services.CQRS.Commands.User.Profile;
using TaskFlow.Application.Services.CQRS.Commands.User.ToggleUserStatus;
using TaskFlow.Application.Services.CQRS.Query.User.GetUser;
using TaskFlow.Application.Services.CQRS.Query.User.GetUsers;


namespace TaskFlow.Api.Controllers
{
    public class UserController : AdminBaseController
    {
        #region Constractor


        private readonly GetUserQueryHandler _getUserHandler;
        private readonly GetUsersQueryHandler _getUsersHandler;
        private readonly SaveUserCommandHandler _saveUserHandler;
        private readonly UpdateMyProfileCommandHandler _updateMyProfileHandler;
        private readonly ToggleUserStatusCommandHandler _toggleUserStatusCommandHandler;

        public UserController(
            GetUserQueryHandler getUserHandler,
            GetUsersQueryHandler getUsersHandler,
            SaveUserCommandHandler saveUserHandler,
             UpdateMyProfileCommandHandler updateMyProfileHandler,
             ToggleUserStatusCommandHandler toggleUserStatusCommandHandler
            )
        {
            _getUserHandler = getUserHandler;
            _getUsersHandler = getUsersHandler;
            _saveUserHandler = saveUserHandler;
            _updateMyProfileHandler = updateMyProfileHandler;
            _toggleUserStatusCommandHandler = toggleUserStatusCommandHandler;
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


        #region نمایش کاربر
        /// <summary>
        /// نمایش کاربر با UserId وارد شده
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query)
        {
            var result = await _getUserHandler.Handle(query);

            return Ok(result);
        }
        #endregion



        #region ذخیره کاربر
        /// <summary>
        /// ذخیره و ویرایش کاربر
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("SaveUser")]
        public async Task<IActionResult> SaveUser([FromForm] SaveUserCommand command)
        {
            var result = await _saveUserHandler.Handle(
                command);

            return Ok(result);
        }
        #endregion


        #region ویرایش پروفایل کاربر
        /// <summary>
        /// ویرایش پروفایل کاربر
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateMyProfile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMyProfile( [FromForm] UpdateMyProfileCommand command)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _updateMyProfileHandler.Handle(
                command,
                userId);

            return Ok(result);
        }

        #endregion



        #region تغییر وضعیت کاربر
        /// <summary>
        /// /تغییر وضعیت کاربر فعال /غیر فعال
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>

        [HttpPost("ToggleUserStatus")]
        public async Task<IActionResult> ToggleUserStatus([FromForm] ToggleUserStatusCommanad command)
        {
            var result = await _toggleUserStatusCommandHandler.Handle(
                command);

            return Ok(result);
        }
        #endregion

    }
}
