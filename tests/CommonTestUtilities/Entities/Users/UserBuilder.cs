using Bogus;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;

namespace CommonTestUtilities.Entities.Users;

public class UserBuilder
{
    public static User Build(
        string? name = null, 
        string? email = null, 
        string? passwordHashed = null,
        int nameLength = 10,
        int emailLength = 10,
        Guid? tenantId = null,
        UserStatus status = UserStatus.Active,
        UserType? type = null
    )
    {
        var userFaker = new Faker<User>()
            .CustomInstantiator(f =>
            {
                var resolvedType = type ?? f.PickRandom<UserType>();
                Guid resolvedTenantId;

                if (resolvedType == UserType.SuperAdmin || resolvedType == UserType.InternalSupport)
                {
                    resolvedTenantId = Guid.Empty;
                }
                else
                {
                    resolvedTenantId = tenantId ?? Guid.NewGuid();
                }

                return new User(
                    name ?? f.Person.FullName.PadRight(nameLength, 'a'),
                    email ?? f.Internet.Email(f.Person.FirstName).PadRight(emailLength, 'a'),
                    resolvedType,
                    resolvedTenantId
                );
            });

        var user = userFaker.Generate();

        // Use reflection to set private fields/properties that are not settable via constructor
        var statusProp = typeof(User).GetProperty("Status");
        statusProp?.SetValue(user, status);

        string? finalPasswordHash = passwordHashed;
        if (status != UserStatus.PendingActivation && passwordHashed == null)
        {
            finalPasswordHash = "default-password-hash";
        }
        var passwordHashProp = typeof(User).GetProperty("PasswordHash");
        passwordHashProp?.SetValue(user, finalPasswordHash);

        // Run Validate method via reflection to ensure exceptions are thrown for invalid configurations
        var validateMethod = typeof(User).GetMethod("Validate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        try
        {
            validateMethod?.Invoke(user, null);
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }

        return user;
    }
}
