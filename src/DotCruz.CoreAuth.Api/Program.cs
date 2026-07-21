using DotCruz.CoreAuth.Api.Configurations;
using DotCruz.CoreAuth.Api.Filters;
using DotCruz.CoreAuth.Api.HttpContexts;
using DotCruz.CoreAuth.Api.Middlewares;
using DotCruz.CoreAuth.Application;
using DotCruz.CoreAuth.Common;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using DotCruz.CoreAuth.Infrastructure;
using DotCruz.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCommonConfiguration(builder.Configuration);
builder.Services.AddSharedSecurity(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthTokenProvider, HttpContextTokenValue>();

builder.Services.AddApiConventions();
builder.Services.AddOpenApiDocumentation();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApiDocumentation();

app.UseMiddleware<CultureMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
