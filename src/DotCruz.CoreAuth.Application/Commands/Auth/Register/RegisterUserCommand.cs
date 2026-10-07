using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Register;

public sealed record RegisterUserCommand(string Name, string Email) : IRequest<Guid>;
