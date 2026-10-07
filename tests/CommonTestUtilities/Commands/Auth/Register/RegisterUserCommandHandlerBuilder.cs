using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Security;
using DotCruz.CoreAuth.Application.Commands.Auth.Register;
using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging;
using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Moq;

namespace CommonTestUtilities.Commands.Auth.Register;

public class RegisterUserCommandHandlerBuilder
{
    private IUserWriteRepository _userWriteRepository = UserWriteRepositoryBuilder.BuildStatic();
    private IUserReadRepository _userReadRepository = new UserReadRepositoryBuilder().Build();
    private IActivationTokenWriteRepository _activationTokenWriteRepository = new Mock<IActivationTokenWriteRepository>().Object;
    private ITokenProvider _tokenProvider = new TestTokenProvider();
    private IEmailService _emailService = new Mock<IEmailService>().Object;
    private IWorkspaceProvisioningPublisher _workspaceProvisioningPublisher = new Mock<IWorkspaceProvisioningPublisher>().Object;
    private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();

    public RegisterUserCommandHandlerBuilder SetUserWriteRepository(IUserWriteRepository userWriteRepository)
    {
        _userWriteRepository = userWriteRepository;
        return this;
    }

    public RegisterUserCommandHandlerBuilder SetUserReadRepository(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
        return this;
    }

    public RegisterUserCommandHandlerBuilder SetActivationTokenWriteRepository(IActivationTokenWriteRepository writeRepository)
    {
        _activationTokenWriteRepository = writeRepository;
        return this;
    }

    public RegisterUserCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        return this;
    }

    public RegisterUserCommandHandlerBuilder SetEmailService(IEmailService emailService)
    {
        _emailService = emailService;
        return this;
    }

    public RegisterUserCommandHandlerBuilder SetWorkspaceProvisioningPublisher(IWorkspaceProvisioningPublisher publisher)
    {
        _workspaceProvisioningPublisher = publisher;
        return this;
    }

    public RegisterUserCommandHandlerBuilder SetUnitOfWork(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        return this;
    }

    public RegisterUserCommandHandler Build()
    {
        return new RegisterUserCommandHandler(
            _userWriteRepository,
            _userReadRepository,
            _activationTokenWriteRepository,
            _tokenProvider,
            _emailService,
            _workspaceProvisioningPublisher,
            _unitOfWork
        );
    }
}
