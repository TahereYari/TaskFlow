using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.Constants;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ResultResponse;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.Paging;
using TaskFlow.Domain.ViewModels.RefreshToken;
using TaskFlow.Infra.Data.Context;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskFlow.Infra.Data.Repositories.Account
{
    public class AccountRepository : IAccountRepository
    {

        #region Constractor
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountRepository> _logger;
        private readonly JwtSettings _jwtSettings;
        public AccountRepository(ApplicationDBContext context, UserManager<ApplicationUser> userManager,
                              IConfiguration configuration, IOptions<JwtSettings> jwtSettings,ILogger<AccountRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _jwtSettings = jwtSettings.Value; 
        }

        #endregion


        #region Login

        public async Task<Result> Login(LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Result.Failure(message: "ایمیل یا رمز عبور اشتباه است.");
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordValid)
                    return Result.Failure(message: "ایمیل یا رمز عبور اشتباه است.");
                var accessToken = GenerateAccessToken(user);

                var refreshTokenValue = GenerateRefreshToken();

                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshTokenValue,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };

                _context.RefreshTokens.Add(refreshToken);

                return Result.Success(message: "ورود موفقیت آمیز", data: new { AccessToken = accessToken, RefreshToken = refreshTokenValue });

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ورود کاربر");
                return Result.Failure(message: ErrorMessages.UnknownError);
            }
        }

        #endregion


        #region RefreshToken
        public async Task<Result> RefreshToken(RefreshTokenModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.RefreshToken))
                {
                    return Result.Failure(message: "توکن Refresh الزامیست.");
                }

                // Find refresh token
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

                if (refreshToken == null)
                {
                    return
                        Result.Failure(message: "توکن Refresh نامعتبر است.");
                }

                // Check if token has already been revoked
                if (refreshToken.RevokedAt != null)
                {
                    return
                        Result.Failure(message: "توکن Refresh باطل شده است..");
                }

                // Check token expiration
                if (refreshToken.ExpiresAt <= DateTime.UtcNow)
                {
                    return Result.Failure(message: "توکن Refresh منقضی شده است.");
                }

                // Find user
                var user = await _userManager.FindByIdAsync(
                    refreshToken.UserId.ToString()
                );

                if (user == null || !user.IsActive)
                {
                    return Result.Failure(message: "کاربر فعال نیست.");
                }

                // Revoke old refresh token
                refreshToken.RevokedAt = DateTime.UtcNow;

                // Generate new tokens
                var accessToken = GenerateAccessToken(user);
                var newRefreshTokenValue = GenerateRefreshToken();

                // Create new refresh token
                var newRefreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshTokenValue,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };

                _context.RefreshTokens.Add(newRefreshToken);

                // Save old token revocation + new token
                await _context.SaveChangesAsync();

                return Result.Success(
                        message: "Token refreshed successfully.",
                        data: new
                        {
                            AccessToken = accessToken,
                            RefreshToken = newRefreshTokenValue
                        }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در زمان توکن Refresh");
                return Result.Failure(message: ErrorMessages.UnknownError);
            }
        }
        #endregion




        #region Register
        public async Task<Result> RegisterAsync(RegisterModel model)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // username و تکراری بودن email
                var ValidationErrors = await ValidateUserFieldsAsync(model.Email, model.UserName);

                if (ValidationErrors.Count > 0)
                {
                    return (Result.Failure(message: ErrorMessages.ValidationError, errors: ValidationErrors));

                }
                // Create user
                var appUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                var userResult = await _userManager.CreateAsync(
                    appUser,
                    model.Password);

                if (!userResult.Succeeded)
                {
                   

                    await transaction.RollbackAsync();

                    return Result.Failure(
                        "ایجاد کاربر انجام نشد.",
                         GetIdentityErrors(userResult));
                }

                // Create profile
                var profile = new UserProfile
                {
                    UserId = appUser.Id,
                    Name = "",
                    Family = "",
                    ProfileImage="",
                    CreateDate=DateTime.Now,
                    IsDeleted=false,

                };

                await _context.UserProfiles.AddAsync(profile);

                // Assign default role
                var roleResult = await _userManager.AddToRoleAsync(
                    appUser,
                    "User");

                if (!roleResult.Succeeded)
                {

                    await transaction.RollbackAsync();

                    return Result.Failure(
                        "ایجاد نقش برای کاربر انجام نشد.",
                        GetIdentityErrors(roleResult));
                }

                // Save profile
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return Result.Success("کاربر با موفقیت ثبت‌ نام شد.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError("خطا در ثبت نام کاربر");
                return Result.Failure(message: ErrorMessages.UnknownError, errors: new List<string> { ErrorMessages.ExceptionError });
            }
        }
        #endregion


        #region GetAsync
        public async Task<Result> GetAsync(int id)
        {
          try {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result.Failure(message: "کاربر وجود ندارد.");
            }

            return Result.Success(message: " کاربر با موفقیت لود شد.", data: user);

            }
            catch (Exception ex)
            {
                _logger.LogError(  ex, "خطا در دریافت کاربر.");
                return Result.Failure(message: ErrorMessages.UnknownError);
            }
        }
        #endregion


        #region UpdateAsync
        public Task<Result> UpdateAsync(RegisterModel model)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region GetUsersAsync
        #region GetUsersAsync

        #region GetUsersAsync

        public async Task<Result> GetUsersAsync(FilterUsersViewModel model)
        {
            try
            {
                var usersQuery = _userManager.Users
                    .AsNoTracking()
                    .Include(x => x.UserProfile)
                    .AsQueryable();

                // Search

                if (!string.IsNullOrWhiteSpace(model.Search))
                {
                    model.Search = model.Search.Trim();

                    usersQuery = usersQuery.Where(x =>
                        (x.UserName != null &&
                         x.UserName.Contains(model.Search)) ||

                        (x.Email != null &&
                         x.Email.Contains(model.Search)) ||

                        (x.UserProfile != null &&
                         x.UserProfile.Name.Contains(model.Search)) ||

                        (x.UserProfile != null &&
                         x.UserProfile.Family.Contains(model.Search))
                    );
                }

                // Order

                usersQuery = usersQuery
                    .OrderByDescending(x => x.Id);

                // Pagination

                var paging = new BasePaging<ApplicationUser>
                {
                    PageId = model.PageId,
                    TakeEntity = model.TakeEntity
                };

                await paging.Paging(usersQuery);


                var result = new List<object>();

                foreach (var user in paging.Entities)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    result.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,

                        Name = user.UserProfile?.Name,
                        Family = user.UserProfile?.Family,

                        ProfileImage =
                            user.UserProfile?.ProfileImage,

                        Roles = roles
                    });
                }




                return Result.Success(
                    message: "لیست کاربران با موفقیت لود شد.",
                    data: new
                    {
                        Users = result,

                        Paging = paging.GetCurrentPaging()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در دریافت کاربران.");

                return Result.Failure(
                    message: ErrorMessages.UnknownError);
            }
        }

        #endregion

        #endregion
        #endregion


        #region private Methode
        private List<string> GetIdentityErrors(IdentityResult result)
        {
            return result.Errors
                .Select(error =>
                    IdentityErrorMessages.GetMessage(
                        error.Code,
                        error.Description))
                .ToList();
        }

        private async Task<List<string>> ValidateUserFieldsAsync(
           string email,
           string userName,
           Guid? ignoreUserId = null)
        {
            var errors = new List<string>();

            // Check if email already exists for another user
            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null && existingEmail.Id != ignoreUserId)
                errors.Add("این ایمیل قبلاً ثبت شده است.");

            // Check if username already exists for another user
            var existingUserName = await _userManager.FindByNameAsync(userName);
            if (existingUserName != null && existingUserName.Id != ignoreUserId)
                errors.Add("این نام کاربری قبلاً ثبت شده است.");

            return errors;
        }


        /// <summary>
        /// Generates a JWT token for the specified user with their roles
        /// </summary>
        /// <param name="user">The user for whom the token is generated</param>

        /// <returns>JWT token string</returns>
        private string GenerateAccessToken(ApplicationUser user)
        {
            // Create claims for the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject: User Id
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),       // Email claim
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // NameIdentifier claim
                       // Primary role
            };


            // Generate symmetric security key from the environment
            var keyString = Environment.GetEnvironmentVariable("JwtSettings__Key")
                ?? throw new InvalidOperationException("JWT Key is missing in the environment.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));


            // Define signing credentials using HMAC SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationMinutes = int.Parse(
                _configuration["JwtSettings:AccessTokenExpirationMinutes"]
                ?? throw new InvalidOperationException(
                    "JWT AccessTokenExpirationMinutes is missing or invalid"
                )
              );

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                 expires: DateTime.Now.AddMinutes(expirationMinutes), // Token valid for 15 Minute
                signingCredentials: creds
            );

            // Return the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate Refresh Token
        /// </summary>
        /// <returns></returns>
        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

  

        #endregion
    }
}
