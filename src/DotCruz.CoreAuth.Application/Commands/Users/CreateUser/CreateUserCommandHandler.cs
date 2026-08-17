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
using DotCruz.Shared.Security.Context;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Users.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IActivationTokenWriteRepository _activationTokenWriteRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IEmailService _emailService;
    private readonly ISecurityContext _securityContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserReadRepository userReadRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ITokenProvider tokenProvider,
        IActivationTokenWriteRepository activationTokenWriteRepository,
        ISecurityContext securityContext
    )
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _activationTokenWriteRepository = activationTokenWriteRepository;
        _tokenProvider = tokenProvider;
        _emailService = emailService;
        _securityContext = securityContext;
        _unitOfWork = unitOfWork;
    }


    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        request = ApplyCallerRestrictions(request);

        await ValidateEmailExists(request.Email, cancellationToken);

        var user = await CreateUser(request, cancellationToken);

        var plainToken = await CreateActivationToken(user.Id, cancellationToken);
        
        await _emailService.SendActivationEmailAsync(user.Email, user.Name, plainToken, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return user.Id;
    }

    private CreateUserCommand ApplyCallerRestrictions(CreateUserCommand request)
    {
        if (_securityContext.IsAuthenticatedService || CallerIsInRole(UserType.SuperAdmin))
            return request;

        if (!CallerIsInRole(UserType.TenantAdmin))
            throw new ForbiddenException(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE);

        if (request.Type is not (UserType.TenantAdmin or UserType.TenantUser))
            throw new ForbiddenException(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE);

        var callerTenantId = _securityContext.TenantId ?? Guid.Empty;

        if (callerTenantId == Guid.Empty)
            throw new ForbiddenException(ResourceMessagesException.TENANT_ID_REQUIRED);

        return request with { TenantId = callerTenantId };
    }

    private bool CallerIsInRole(UserType type) =>
        _securityContext.Roles.Contains(type.ToString(), StringComparer.OrdinalIgnoreCase);

    private async Task ValidateEmailExists(string email, CancellationToken cancellationToken)
    {
        if (await _userReadRepository.ExistsActiveUserWithEmailAsync(email, cancellationToken))
            throw new ErrorOnValidationException(ResourceMessagesException.EMAIL_ALREADY_EXISTS);
    }

    private async Task<User> CreateUser(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(
            request.Name,
            request.Email,
            request.Type,
            request.TenantId
        );

        await _userWriteRepository.AddAsync(user, cancellationToken);

        return user;
    }

    private async Task<string> CreateActivationToken(Guid userId, CancellationToken cancellationToken)
    {
        var plainToken = _tokenProvider.Value();
        var hashedToken = _tokenProvider.Hash(plainToken);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(1);

        var activationToken = new ActivationToken(
            hashedToken,
            expiresAt,
            userId
        );

        await _activationTokenWriteRepository.AddAsync(activationToken, cancellationToken);

        return plainToken;
    }
}
