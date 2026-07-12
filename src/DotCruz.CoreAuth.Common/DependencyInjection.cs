using DotCruz.CoreAuth.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotCruz.CoreAuth.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            AddJwtTokenSettings(services, configuration);
            AddPasswordResetTokenSettings(services, configuration);

            return services;
        }

        private static void AddJwtTokenSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtTokenSettings>(
                configuration.GetSection("Settings:Jwt")
            );
        }

        private static void AddPasswordResetTokenSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PasswordResetTokenSettings>(
                configuration.GetSection("Settings:PasswordResetTokenSettings")
            );
        }
    }
}
