using DotCruz.CoreAuth.Domain.Entities.Base;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;

namespace DotCruz.CoreAuth.Domain.Entities.Users;

public class User : TenantEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public UserType Type { get; private set; }
    public UserStatus Status { get; private set; }

    public IEnumerable<PasswordResetToken> PasswordResetTokens { get; private set; } = new List<PasswordResetToken>();
    public IEnumerable<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public User(string name, string email, UserType type, Guid tenantId)
    {
        Name = name;
        Email = email.ToLowerInvariant();
        Type = type;
        Status = UserStatus.PendingActivation;
        SetTenantId(tenantId);

        Validate();
    }

    public void Update(string? name, string? email, string? passwordHash, UserType? type, Guid? tenantId = null)
    {
        Name = name ?? Name;
        Email = email?.ToLowerInvariant() ?? Email;
        PasswordHash = passwordHash ?? PasswordHash;
        Type = type ?? Type;

        if (Type == UserType.SuperAdmin || Type == UserType.InternalSupport)
            SetTenantId(tenantId ?? TenantId);

        Validate();
    }

    public void Activate(string passwordHash)
    {
        if (Status != UserStatus.PendingActivation)
            throw new ErrorOnValidationException(ResourceMessagesException.TOKEN_INVALID);

        PasswordHash = passwordHash;
        Status = UserStatus.Active;

        Validate();
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Validate();
    }

    private void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(Name))
            errors.Add(ResourceMessagesException.NAME_EMPTY);

        if (Name.Length > 200)
            errors.Add(string.Format(ResourceMessagesException.NAME_MAX_LENGTH, 200));

        if (string.IsNullOrEmpty(Email))
            errors.Add(ResourceMessagesException.EMAIL_EMPTY);

        if (Email.Length > 200)
            errors.Add(string.Format(ResourceMessagesException.EMAIL_MAX_LENGTH, 200));

        if (Status != UserStatus.PendingActivation && string.IsNullOrEmpty(PasswordHash))
            errors.Add(ResourceMessagesException.PASSWORD_EMPTY);

        if ((Type == UserType.TenantAdmin || Type == UserType.TenantUser) && TenantId == Guid.Empty)
            errors.Add(ResourceMessagesException.TENANT_ID_REQUIRED);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
