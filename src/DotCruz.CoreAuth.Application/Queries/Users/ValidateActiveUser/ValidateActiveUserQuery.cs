using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.Users.ValidateActiveUser;

public record ValidateActiveUserQuery(Guid UserId) : IRequest<bool>;
