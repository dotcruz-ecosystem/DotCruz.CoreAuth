using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace DotCruz.CoreAuth.Application.Queries.Users.GetUserById;

public class GetUserByIdQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetUserByIdQuery, ResponseUserDto>
{
    public async Task<ResponseUserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            throw new NotFoundException(ResourceMessagesException.USER_NOT_FOUND);

        return new ResponseUserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Type,
            user.Status,
            user.TenantId,
            user.CreatedAt
);
    }
}
