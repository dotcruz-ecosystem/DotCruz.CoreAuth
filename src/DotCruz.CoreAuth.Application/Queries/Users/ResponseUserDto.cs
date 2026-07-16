using DotCruz.CoreAuth.Domain.Enums.Users;
using System;

namespace DotCruz.CoreAuth.Application.Queries.Users;

public record ResponseUserDto(
    Guid Id,
    string Name,
    string Email,
    UserType Type,
    UserStatus Status,
    Guid? TenantId,
    DateTimeOffset CreatedAt
);
