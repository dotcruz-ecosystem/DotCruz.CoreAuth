using DotCruz.CoreAuth.Domain.Entities.Base;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;

namespace DotCruz.CoreAuth.Domain.Entities.Users;

public class User : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public UserType Type { get; private set; }
    public UserStatus Status { get; private set; }
    public Guid? DefaultWorkspaceId { get; private set; }
    public Guid WorkspaceId => DefaultWorkspaceId ?? Guid.Empty;

    public bool CanAuthenticate =>
        Status == UserStatus.Active
        && DeletedAt is null
        && !string.IsNullOrEmpty(PasswordHash);

    public IEnumerable<PasswordResetToken> PasswordResetTokens { get; private set; } = new List<PasswordResetToken>();
    public IEnumerable<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public User(string name, string email, UserType type = UserType.User, Guid? workspaceId = null)
    {
        Name = name;
        Email = email.ToLowerInvariant();
        Type = type;
        Status = UserStatus.PendingActivation;
        SetDefaultWorkspaceId(workspaceId);

        Validate();
    }

    public void SetDefaultWorkspaceId(Guid? workspaceId)
    {
        DefaultWorkspaceId = workspaceId == Guid.Empty ? null : workspaceId;
    }

    public void SetWorkspaceId(Guid workspaceId) => SetDefaultWorkspaceId(workspaceId);

    public void Update(string? name, string? email, string? passwordHash, UserType? type, Guid? workspaceId = null)
    {
        Name = name ?? Name;
        Email = email?.ToLowerInvariant() ?? Email;
        PasswordHash = passwordHash ?? PasswordHash;
        Type = type ?? Type;

        if (workspaceId.HasValue)
            SetDefaultWorkspaceId(workspaceId.Value);

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

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
