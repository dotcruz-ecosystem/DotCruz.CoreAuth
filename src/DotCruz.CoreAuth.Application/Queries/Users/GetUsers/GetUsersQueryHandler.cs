using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotCruz.CoreAuth.Application.Queries.Users.GetUsers;

public class GetUsersQueryHandler(IUserReadRepository userReadRepository) 
    : IRequestHandler<GetUsersQuery, PagedResultDto<ResponseUserDto>>
{
    public async Task<PagedResultDto<ResponseUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await userReadRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        var userDtos = items.Select(user => new ResponseUserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Type,
            user.Status,
            user.TenantId,
            user.CreatedAt
        )).ToList();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedResultDto<ResponseUserDto>(
            userDtos,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages
        );
    }
}
