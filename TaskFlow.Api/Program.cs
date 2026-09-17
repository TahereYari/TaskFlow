

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskFlow.Application.Services.Implemantation.Account;
using TaskFlow.Application.Services.Interfaces.Account;
using TaskFlow.Domain.ErrorMessages;
using TaskFlow.Domain.IRepository;
using TaskFlow.Infra.Data.Context;
using TaskFlow.Infra.Data.Repositories.Account;
using TaskFlow.Infra.IOC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region AddDBConnection
    builder.Services.AddInfrastructure(builder.Configuration);
#endregion


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
