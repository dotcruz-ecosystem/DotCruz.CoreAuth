using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging;
using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging.Messages;
using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IActivationTokenWriteRepository _activationTokenWriteRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IEmailService _emailService;
    private readonly IWorkspaceProvisioningPublisher _workspaceProvisioningPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserReadRepository userReadRepository,
        IActivationTokenWriteRepository activationTokenWriteRepository,
        ITokenProvider tokenProvider,
        IEmailService emailService,
        IWorkspaceProvisioningPublisher workspaceProvisioningPublisher,
        IUnitOfWork unitOfWork)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _activationTokenWriteRepository = activationTokenWriteRepository;
        _tokenProvider = tokenProvider;
        _emailService = emailService;
        _workspaceProvisioningPublisher = workspaceProvisioningPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userReadRepository.ExistsActiveUserWithEmailAsync(email, cancellationToken))
            throw new ErrorOnValidationException(ResourceMessagesException.EMAIL_ALREADY_EXISTS);

        var user = new User(request.Name.Trim(), email, UserType.TenantUser);
        await _userWriteRepository.AddAsync(user, cancellationToken);

        var plainToken = _tokenProvider.Value();
        var hashedToken = _tokenProvider.Hash(plainToken);
        var activationToken = new ActivationToken(
            hashedToken,
            DateTimeOffset.UtcNow.AddDays(1),
            user.Id
        );
        await _activationTokenWriteRepository.AddAsync(activationToken, cancellationToken);

        await _emailService.SendActivationEmailAsync(user.Email, user.Name, plainToken, cancellationToken);

        await _workspaceProvisioningPublisher.PublishUserRegisteredAsync(
            new UserRegisteredIntegrationEvent(user.Id, user.Name, user.Email, DateTime.UtcNow),
            cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);

        return user.Id;
    }
}
