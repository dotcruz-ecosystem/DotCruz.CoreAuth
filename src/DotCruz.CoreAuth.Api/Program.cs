using DotCruz.CoreAuth.Api.Filters;
using DotCruz.CoreAuth.Api.HttpContexts;
using DotCruz.CoreAuth.Api.Middlewares;
using DotCruz.CoreAuth.Application;
using DotCruz.CoreAuth.Common;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using DotCruz.CoreAuth.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCommonConfiguration(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthTokenProvider, HttpContextTokenValue>();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantValue>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<CultureMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
