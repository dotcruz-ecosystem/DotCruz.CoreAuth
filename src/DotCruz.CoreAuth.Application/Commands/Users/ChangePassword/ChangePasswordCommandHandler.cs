using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Users.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IRefreshTokenReadRepository _refreshTokenReadRepository;
    private readonly ILoggedUser _loggedUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IUserWriteRepository userWriteRepository,
        IRefreshTokenReadRepository refreshTokenReadRepository,
        ILoggedUser loggedUser,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork
    )
    {
        _userWriteRepository = userWriteRepository;
        _refreshTokenReadRepository = refreshTokenReadRepository;
        _loggedUser = loggedUser;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var loggedUser = await _loggedUser.User(cancellationToken);

        var user = await _userWriteRepository.GetByIdToUpdate(loggedUser.Id, cancellationToken)
            ?? throw new NotFoundException(ResourceMessagesException.USER_NOT_FOUND);

        var isPasswordValid = _passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash ?? string.Empty);
        if (!isPasswordValid)
            throw new InvalidLoginException();

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        var activeRefreshTokens = await _refreshTokenReadRepository
            .GetActiveTokensByUserIdAsync(user.Id, cancellationToken);

        foreach (var refreshToken in activeRefreshTokens ?? [])
            refreshToken.Revoke();

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
