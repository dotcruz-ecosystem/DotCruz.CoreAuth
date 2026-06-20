using DotCruz.CoreAuth.Domain.Enums.Users;
using MediatR;
using System;

namespace DotCruz.CoreAuth.Application.Commands.Users.CreateUser;

public record CreateUserCommand(
    string Name,
    string Email,
    UserType Type,
    Guid TenantId) : IRequest<Guid>;