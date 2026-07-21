using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Users;
using DotCruz.CoreAuth.Application.Commands.Auth.PasswordResetTokens.RequestPasswordReset;
using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Moq;

namespace CommonTestUtilities.Commands.Users
{
    public class RequestPasswordResetCommandHandlerBuilder
    {
        private IUserReadRepository _userReadRepository = new UserReadRepositoryBuilder().Build();
        private IPasswordResetTokenWriteRepository _tokenWriteRepository = new Mock<IPasswordResetTokenWriteRepository>().Object;
        private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();
        private IEmailService _emailService = new Mock<IEmailService>().Object;
        private ITokenProvider _tokenProvider = new Mock<ITokenProvider>().Object;
        private IOptions<PasswordResetTokenSettings> _passwordResetTokenSettings = Options.Create(new PasswordResetTokenSettings { ExpirationTimeInMinutes = 60 });

        public RequestPasswordResetCommandHandlerBuilder SetUserReadRepository(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository;
            return this;
        }

        public RequestPasswordResetCommandHandlerBuilder SetEmailService(IEmailService emailService)
        {
            _emailService = emailService;
            return this;
        }

        public RequestPasswordResetCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
            return this;
        }

        public RequestPasswordResetCommandHandler Build()
        {
            return new RequestPasswordResetCommandHandler(
                _userReadRepository,
                _tokenWriteRepository,
                _unitOfWork,
                _emailService,
                _tokenProvider,
                _passwordResetTokenSettings
            );
        }
    }
}
