using CommonTestUtilities.Entities.Tokens;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Services;
using DotCruz.CoreAuth.Application.Interfaces.Services;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace WebApi.Test;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private User _user = default!;
    private string _password = string.Empty;
    private RefreshToken _refreshToken = default!;
    private string _refreshTokenValue = string.Empty;

    private User _pendingUser = default!;
    private ActivationToken _activationToken = default!;
    private string _activationTokenValue = string.Empty;

    private User _superAdminUser = default!;
    private string _superAdminPassword = string.Empty;

    private PasswordResetToken _passwordResetToken = default!;
    private string _passwordResetTokenValue = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();

        Environment.SetEnvironmentVariable("Settings__Jwt__Issuer", "test-issuer");
        Environment.SetEnvironmentVariable("Settings__Jwt__Audience", "test-audience");
        Environment.SetEnvironmentVariable("Settings__Jwt__JwksUrl", "https://localhost:8080/.well-known/jwks.json");
        Environment.SetEnvironmentVariable("Settings__Jwt__Kid", "test-kid");
        Environment.SetEnvironmentVariable("Settings__Jwt__PrivateKeyPem", privateKeyPem);
        Environment.SetEnvironmentVariable("Settings__Jwt__ExpirationTimeMinutes", "60");
        Environment.SetEnvironmentVariable("Settings__Jwt__RefreshTokenExpirationTimeDays", "7");
        Environment.SetEnvironmentVariable("Settings__PasswordResetTokenSettings__ExpirationTimeInMinutes", "60");
        Environment.SetEnvironmentVariable("Settings__Notification__BaseUrl", "https://localhost:5001");
        Environment.SetEnvironmentVariable("Settings__Notification__ApiKey", "test-api-key");
        Environment.SetEnvironmentVariable("Settings__ServiceAuth__Self__Name", "test-service");
        Environment.SetEnvironmentVariable("Settings__ServiceAuth__Self__Key", "test-service-key");

        builder.UseEnvironment("Test")
            .ConfigureServices(services =>
            {
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CoreAuthDbContext>));
                if (dbDescriptor is not null)
                    services.Remove(dbDescriptor);

                // Remove invalid abstract BaseRepository open generic registrations
                var baseReadDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DotCruz.CoreAuth.Domain.Interfaces.Repositories.Base.IBaseReadRepository<>));
                if (baseReadDescriptor is not null)
                    services.Remove(baseReadDescriptor);

                var baseWriteDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DotCruz.CoreAuth.Domain.Interfaces.Repositories.Base.IBaseWriteRepository<>));
                if (baseWriteDescriptor is not null)
                    services.Remove(baseWriteDescriptor);

                var dbServiceProvider = services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

                services.AddDbContext<CoreAuthDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(dbServiceProvider);
                });

                services.AddHttpContextAccessor();

                var tenantDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITenantProvider));
                if (tenantDescriptor is not null)
                    services.Remove(tenantDescriptor);

                services.AddScoped<ITenantProvider>(sp =>
                {
                    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                    var httpContext = httpContextAccessor.HttpContext;
                    Guid? tenantId = null;
                    
                    if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var values) && Guid.TryParse(values, out var parsedGuid))
                        tenantId = parsedGuid;
                    
                    var mock = new Mock<ITenantProvider>();
                    mock.Setup(t => t.TenantId()).Returns(tenantId);
                    return mock.Object;
                });

                var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (emailDescriptor is not null)
                    services.Remove(emailDescriptor);

                services.AddScoped(_ => EmailServiceBuilder.Build());

                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CoreAuthDbContext>();

                dbContext.Database.EnsureDeleted();
                StartDatabase(dbContext);
            });
    }

    public string GetEmail() => _user.Email;
    public string GetPassword() => _password;
    public string GetName() => _user.Name;
    public Guid GetUserId() => _user.Id;
    public Guid? GetTenantId() => _user.TenantId;
    public string GetRefreshToken() => _refreshTokenValue;

    public string GenerateAccessToken(User user)
    {
        using var scope = Services.CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens.IAccessTokenGenerator>();
        return generator.Generate(user);
    }

    public string GetAccessToken()
    {
        return GenerateAccessToken(_user);
    }

    public string GetPendingUserEmail() => _pendingUser.Email;
    public string GetActivationToken() => _activationTokenValue;

    public string GetPasswordResetToken() => _passwordResetTokenValue;

    public string GetSuperAdminEmail() => _superAdminUser.Email;
    public string GetSuperAdminPassword() => _superAdminPassword;

    private void StartDatabase(CoreAuthDbContext dbContext)
    {
        try
        {
            var faker = new Bogus.Faker();
            var hasher = new DotCruz.CoreAuth.Infrastructure.Security.Password.BCryptHasher();
            var tokenProvider = new DotCruz.CoreAuth.Infrastructure.Security.Tokens.CryptographyTokenProvider();

            string GenerateStrongPassword() => faker.Internet.Password(12, false, @"[a-zA-Z0-9]") + "1!Aa";

            var tenantId = Guid.NewGuid();

            _password = GenerateStrongPassword();
            var activeUserHash = hasher.HashPassword(_password);
            _user = UserBuilder.Build(
                name: faker.Person.FullName,
                email: faker.Internet.Email(),
                passwordHashed: activeUserHash,
                tenantId: tenantId,
                status: UserStatus.Active,
                type: UserType.TenantUser
            );
        
            dbContext.Users.Add(_user);

            _refreshTokenValue = tokenProvider.Value();
            var hashedRefreshToken = tokenProvider.Hash(_refreshTokenValue);
            _refreshToken = RefreshTokenBuilder.Build(
                token: hashedRefreshToken,
                expiresAt: DateTimeOffset.UtcNow.AddDays(7),
                userId: _user.Id
            );
            dbContext.RefreshTokens.Add(_refreshToken);

            _passwordResetTokenValue = Guid.NewGuid().ToString();
            var hashedResetToken = tokenProvider.Hash(_passwordResetTokenValue);
            _passwordResetToken = PasswordResetTokenBuilder.Build(
                token: hashedResetToken,
                expiresAt: DateTimeOffset.UtcNow.AddHours(1),
                userId: _user.Id
            );
            dbContext.PasswordResetTokens.Add(_passwordResetToken);

            _pendingUser = UserBuilder.Build(
                name: faker.Person.FullName,
                email: faker.Internet.Email(),
                passwordHashed: null,
                tenantId: tenantId,
                status: UserStatus.PendingActivation,
                type: UserType.TenantUser
            );

            dbContext.Users.Add(_pendingUser);

            _activationTokenValue = Guid.NewGuid().ToString();
            var hashedActivationToken = tokenProvider.Hash(_activationTokenValue);
            _activationToken = ActivationTokenBuilder.Build(
                token: hashedActivationToken,
                expiresAt: DateTimeOffset.UtcNow.AddDays(1),
                userId: _pendingUser.Id
            );
            dbContext.ActivationTokens.Add(_activationToken);

            _superAdminPassword = GenerateStrongPassword();
            var superAdminHash = hasher.HashPassword(_superAdminPassword);
            _superAdminUser = UserBuilder.Build(
                name: faker.Person.FullName,
                email: faker.Internet.Email(),
                passwordHashed: superAdminHash,
                tenantId: null,
                status: UserStatus.Active
            );
        
            _superAdminUser.Update(name: null, email: null, passwordHash: null, type: UserType.SuperAdmin, tenantId: null);

            dbContext.Users.Add(_superAdminUser);

            dbContext.SaveChanges();
        }
        catch (DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions.ErrorOnValidationException valEx)
        {
            var errs = string.Join(", ", valEx.GetErrorsMessages());
            throw new Exception($"Validation Exception during seeding: {errs}", valEx);
        }
    }
}
