using DotCruz.CoreAuth.Domain.Entities.Base;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;

namespace DotCruz.CoreAuth.Domain.Entities.Tokens
{
    public class PasswordResetToken : BaseEntity
    {
        public string Token { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public Guid UserId { get; private set; }

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

        public User? User { get; private set; }

        private PasswordResetToken() { }

        public PasswordResetToken(string token, DateTimeOffset expiresAt, Guid userId)
        {
            Token = token;
            ExpiresAt = expiresAt;
            UserId = userId;
            IsUsed = false;

            Validate();
        }

        private void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Token))
                errors.Add(ResourceMessagesException.TOKEN_EMPTY);

            if (IsExpired)
                errors.Add(ResourceMessagesException.EXPIRATION_DATE_EARLIER_CURRENT_DATE);

            if (errors.Count > 0)
                throw new ErrorOnValidationException(errors);
        }

        public void MarkAsUsed()
        {
            if (IsExpired) 
                throw new ErrorOnValidationException(ResourceMessagesException.TOKEN_EXPIRED);

            IsUsed = true;
        }
    }
}
