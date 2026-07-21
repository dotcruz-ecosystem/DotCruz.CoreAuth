using DotCruz.CoreAuth.Application.Commands.Users.CreateUser;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Services;
using Moq;
using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;

namespace CommonTestUtilities.Commands.Users;

public class CreateUserCommandHandlerBuilder
{
    private IUserWriteRepository _userWriteRepository = UserWriteRepositoryBuilder.BuildStatic();
    private IUserReadRepository _userReadRepository = new UserReadRepositoryBuilder().Build();
    private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();
    private IEmailService _emailService = EmailServiceBuilder.Build();
    private ITokenProvider _tokenProvider = new Mock<ITokenProvider>().Object;
    private IActivationTokenWriteRepository _activationTokenWriteRepository = new Mock<IActivationTokenWriteRepository>().Object;

    public CreateUserCommandHandlerBuilder SetUserReadRepository(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
        return this;
    }

    public CreateUserCommandHandlerBuilder SetUserWriteRepository(IUserWriteRepository userWriteRepository)
    {
        _userWriteRepository = userWriteRepository;
        return this;
    }

    public CreateUserCommandHandlerBuilder SetEmailService(IEmailService emailService)
    {
        _emailService = emailService;
        return this;
    }

    public CreateUserCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        return this;
    }

    public CreateUserCommandHandlerBuilder SetActivationTokenWriteRepository(IActivationTokenWriteRepository activationTokenWriteRepository)
    {
        _activationTokenWriteRepository = activationTokenWriteRepository;
        return this;
    }

    public CreateUserCommandHandler Build()
    {
        return new CreateUserCommandHandler(
            _userWriteRepository,
            _userReadRepository,
            _unitOfWork,
            _emailService,
            _tokenProvider,
            _activationTokenWriteRepository
        );
    }
}
