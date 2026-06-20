using DotCruz.CoreAuth.Application.Interfaces.Services;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Base;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using DotCruz.CoreAuth.Infrastructure.Data;
using DotCruz.CoreAuth.Infrastructure.Repositories.Base;
using DotCruz.CoreAuth.Infrastructure.Repositories.Tokens;
using DotCruz.CoreAuth.Infrastructure.Repositories.Users;
using DotCruz.CoreAuth.Infrastructure.Security.Password;
using DotCruz.CoreAuth.Infrastructure.Security.Tokens;
using DotCruz.CoreAuth.Infrastructure.Security.Tokens.Access.Generator;
using DotCruz.CoreAuth.Infrastructure.Security.Tokens.Access.Validator;
using DotCruz.CoreAuth.Infrastructure.Security.Tokens.Refresh;
using DotCruz.CoreAuth.Infrastructure.Services.LoggedUser;
using DotCruz.CoreAuth.Infrastructure.Services.Notification;
using DotCruz.CoreAuth.Infrastructure.Services.TenantResolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotCruz.CoreAuth.Infrastructure;

public static class DependencyInjection
{
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddRepositories(services);
            AddUnitOfWork(services);
            AddSecurity(services);
            AddServices(services, configuration);

            AddDbContext(services, configuration);

            return services;
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseReadRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IBaseWriteRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IPasswordResetTokenReadRepository, PasswordResetTokenRepository>();
            services.AddScoped<IPasswordResetTokenWriteRepository, PasswordResetTokenRepository>();

            services.AddScoped<IActivationTokenReadRepository, ActivationTokenRepository>();
            services.AddScoped<IActivationTokenWriteRepository, ActivationTokenRepository>();

            services.AddScoped<IRefreshTokenReadRepository, RefreshTokenRepository>();
            services.AddScoped<IRefreshTokenWriteRepository, RefreshTokenRepository>();

            services.AddScoped<IUserReadRepository, UserRepository>();
            services.AddScoped<IUserWriteRepository, UserRepository>();
        }

        private static void AddUnitOfWork(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void AddSecurity(IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, BCryptHasher>();
            services.AddScoped<IAccessTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddScoped<IAccessTokenValidator, JwtTokenValidator>();
            services.AddScoped<ITokenProvider, CryptographyTokenProvider>();
        }

        private static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            var notificationSettings = configuration.GetSection("Settings:Notification").Get<NotificationSettings>();

            services.AddHttpClient<IEmailService, EmailService>(client =>
            {
                client.BaseAddress = new Uri(notificationSettings!.BaseUrl);

                client.DefaultRequestHeaders.Add("X-Api-Key", notificationSettings!.ApiKey);
            });

            services.AddScoped<ILoggedUser, LoggedUser>();
            services.AddScoped<ITenantResolver, TenantResolver>();
    }

        private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgresqlConnection");

            services.AddDbContext<CoreAuthDbContext>(opt =>
            {
                opt.UseNpgsql(connectionString);
                opt.UseSnakeCaseNamingConvention();
            });
        }
    }
