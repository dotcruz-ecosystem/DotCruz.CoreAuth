using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.RevokeAllUserTokens;

public class RevokeAllUserTokensCommandHandler : IRequestHandler<RevokeAllUserTokensCommand>
{
    private readonly IRefreshTokenReadRepository _refreshTokenReadRepository;
    private readonly ILoggedUser _loggedUser;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeAllUserTokensCommandHandler(
        IRefreshTokenReadRepository refreshTokenReadRepository,
        ILoggedUser loggedUser,
        IUnitOfWork unitOfWork
    )
    {
        _refreshTokenReadRepository = refreshTokenReadRepository;
        _loggedUser = loggedUser;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RevokeAllUserTokensCommand request, CancellationToken cancellationToken)
    {
        var user = await _loggedUser.User(cancellationToken);

        var refreshTokens = await _refreshTokenReadRepository.GetActiveTokensByUserIdAsync(user.Id, cancellationToken);

        if (refreshTokens == null || !refreshTokens.Any())
            return;

        refreshTokens.ToList().ForEach(rt => rt.Revoke());

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
