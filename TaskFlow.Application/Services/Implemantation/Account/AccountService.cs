using Microsoft.AspNetCore.Identity;
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
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.Account;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Domain.ViewModels.Accounts;
using TaskFlow.Domain.ViewModels.RefreshToken;

namespace TaskFlow.Application.Services.Implemantation.Account
{
    public class AccountService : IAccountService
    {

        #region Constractor
        private readonly IAccountRepository _accountRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AccountService(
            IAccountRepository accountRepository,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }
        #endregion
      
        #region Login
        public async Task<Result> Login(LoginModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Result.Failure(message: "ایمیل یا رمز عبور اشتباه است.");
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password); ;
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


                await _accountRepository.AddRefreshTokenAsync(refreshToken,cancellationToken);
               await _accountRepository.SaveChangesAsync();
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

                var refreshToken = await _accountRepository.FindRefreshToken(model);

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

               await _accountRepository.AddRefreshToken(newRefreshToken);


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


        #region RegisterAsync
        public async Task<Result> RegisterAsync(RegisterModel model)
        {
             await _accountRepository.BeginTransactionAsync();

            try
            {
                //  اعتبار سنجی
                var validationErrors =
                    await ValidateUserFieldsAsync(
                        model.Email,
                        model.UserName);

                if (validationErrors.Count > 0)
                {
                    return Result.Failure(
                        message: ErrorMessages.ValidationError,
                        errors: validationErrors);
                }

                //  ایجاد کاربر
                var appUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                var userResult =
                    await _userManager.CreateAsync(
                        appUser,
                        model.Password);

                if (!userResult.Succeeded)
                {
                   await _accountRepository.RollbackTransactionAsync();

                    return Result.Failure(
                        "ایجاد کاربر انجام نشد.",
                        GetIdentityErrors(userResult));
                }

                //  ایجاد پروفایل
                var profile = new UserProfile
                {
                    UserId = appUser.Id,
                    Name = "",
                    Family = "",
                    ProfileImage = "",
                    CreateDate = DateTime.Now,
                    IsDeleted = false
                };

                await _accountRepository.AddUserProfileAsync(
                    profile);

                //  اختصاص دادن نقش
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        appUser,
                        "User");

                if (!roleResult.Succeeded)
                {
                    await _accountRepository.RollbackTransactionAsync();

                    return Result.Failure(
                        "ایجاد نقش برای کاربر انجام نشد.",
                        GetIdentityErrors(roleResult));
                }

              
                await _accountRepository.SaveChangesAsync();

              
                await _accountRepository.CommitTransactionAsync();

                return Result.Success(
                    "کاربر با موفقیت ثبت‌ نام شد.");
            }
            catch (Exception ex)
            {
                await _accountRepository.RollbackTransactionAsync();

                _logger.LogError(ex,"خطا در ثبت نام کاربر");

                return Result.Failure(
                    message: ErrorMessages.UnknownError,
                    errors: new List<string>
                    {
                ErrorMessages.ExceptionError
                    });
            }
        }

        #endregion


        #region Private Methode

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

        private List<string> GetIdentityErrors(IdentityResult result)
        {
            return result.Errors
                .Select(error =>
                    IdentityErrorMessages.GetMessage(
                        error.Code,
                        error.Description))
                .ToList();
        }

        private async Task<List<string>> ValidateUserFieldsAsync(string email, string userName, Guid? ignoreUserId = null)
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



        #endregion
    }
}
