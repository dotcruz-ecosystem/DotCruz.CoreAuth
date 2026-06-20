using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.Users.ValidateActiveUser;

public class ValidateActiveUserQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<ValidateActiveUserQuery, bool>
{

    public async Task<bool> Handle(ValidateActiveUserQuery request, CancellationToken cancellationToken)
    {
        return await userReadRepository.IsUserActive(request.UserId, cancellationToken);
    }
}
