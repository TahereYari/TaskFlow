using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.Services.CQRS.Commands.User.CreateUser;
using TaskFlow.Application.Services.CQRS.Commands.User.Profile;
using TaskFlow.Application.Services.CQRS.Commands.User.ToggleUserStatus;
using TaskFlow.Application.Services.CQRS.Query.User.GetUser;
using TaskFlow.Application.Services.CQRS.Query.User.GetUsers;
using TaskFlow.Application.Services.Implemantation.Account;
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Application.Utilities.ResultResponse;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository.Account;
using TaskFlow.Domain.IRepository.User;
using TaskFlow.Domain.Models.Users;
using TaskFlow.Infra.Data.Context;
using TaskFlow.Infra.Data.Repositories.Account;
using TaskFlow.Infra.Data.Repositories.User;

namespace TaskFlow.Infra.IOC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            #region AddDBConnection
            services.AddDbContext<ApplicationDBContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            #endregion


            #region Identity
                services
              .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDBContext>()
            //.AddDefaultTokenProviders()
            ;

            #endregion

            #region Customize automatic 400 validation responses

           services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = false;

                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .SelectMany(v => v.Value?.Errors ?? Enumerable.Empty<ModelError>())
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return new BadRequestObjectResult(
                        Result.Failure(ErrorMessages.ValidationError, errors)
                    );
                };
            });

            #endregion

            #region JWT

            var jwtSettings = configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>();

            if (jwtSettings is null)
            {
                throw new InvalidOperationException(
                    "JWT configuration is missing.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Key))
            {
                throw new InvalidOperationException(
                    "JWT SecretKey is not configured.");
            }

            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,

                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),

                    ClockSkew = TimeSpan.Zero
                };
            });

            #endregion

            //==============================================================

            #region Repository
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            #endregion

            #region Service
              services.AddScoped<IAccountService, AccountService>();
  

            #endregion

            #region Handler
          
            services.AddScoped<GetUserQueryHandler>();
            services.AddScoped<GetUsersQueryHandler>();
            services.AddScoped<SaveUserCommandHandler>();
            services.AddScoped<UpdateMyProfileCommandHandler>();
            services.AddScoped<ToggleUserStatusCommandHandler>();

            #endregion


            return services;
        }
    }
}
