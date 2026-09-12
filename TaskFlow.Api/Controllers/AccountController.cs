using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.ResultResponse;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.RefreshToken;

namespace TaskFlow.Api.Controllers
{
    [ApiController]
   
    public class AccountController : AdminBaseController
    {

        #region Constractor
            private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
            {
                _accountService = accountService;
        }
        #endregion


        #region ثبت نام کاربر
        /// <summary>
        /// ثبت نام کاربر
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
            public async Task<IActionResult> Register(RegisterModel model)
            {

                var result = await _accountService.RegisterAsync(model);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);

       
        }

        #endregion


        #region ورود کاربر
            /// <summary>
            /// ورود کاربر
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            [HttpPost("Login")]
            [AllowAnonymous]
            public async Task<IActionResult> Login(LoginModel model)
            {
                var login = await _accountService.Login(model);
                return Ok(login);
            }
        #endregion


        #region توکن Refresh
        /// <summary>
        ///  توکن Refresh
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
        {
            var refresh = await _accountService.RefreshToken(model);
            return Ok(refresh);
        }
        #endregion
       
        
        #region لیست کاربران
        /// <summary>
        /// لیست کاربران
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet("GetUsersAsync")]
            public async Task<IActionResult> GetUsersAsync([FromQuery]FilterUsersViewModel model)
            {
                var users = await _accountService.GetUsersAsync(model);
                return Ok(users);
            }

        #endregion
    }
}
