using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotCruz.CoreAuth.Application.Commands.Auth.PasswordResetTokens.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
    {
        private readonly IUserReadRepository _userReadRepository;
        private readonly IPasswordResetTokenWriteRepository _tokenWriteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITokenProvider _tokenProvider;
        private readonly PasswordResetTokenSettings _passwordResetTokenSettings;

        public RequestPasswordResetCommandHandler(
            IUserReadRepository userReadRepository,
            IPasswordResetTokenWriteRepository tokenWriteRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ITokenProvider tokenProvider,
            IOptions<PasswordResetTokenSettings> passwordResetTokenSettings
        )
        {
            _userReadRepository = userReadRepository;
            _tokenWriteRepository = tokenWriteRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _tokenProvider = tokenProvider;
            _passwordResetTokenSettings = passwordResetTokenSettings.Value;
        }

        public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userReadRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            if (user is null)
                return;

            var tokenString = _tokenProvider.Value();
            var hashedToken = _tokenProvider.Hash(tokenString);
            var expiresAt = DateTime.UtcNow.AddMinutes(_passwordResetTokenSettings.ExpirationTimeInMinutes);

            var resetToken = new PasswordResetToken(hashedToken, expiresAt, user.Id);

            await _tokenWriteRepository.AddAsync(resetToken, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.Name, tokenString, cancellationToken);
        }
    }
}
