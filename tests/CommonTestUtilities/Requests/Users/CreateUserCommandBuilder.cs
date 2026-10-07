using Bogus;
using DotCruz.CoreAuth.Application.Commands.Users.CreateUser;
using DotCruz.CoreAuth.Domain.Enums.Users;
using System;

namespace CommonTestUtilities.Requests.Users;

public class CreateUserCommandBuilder
{
    public static CreateUserCommand Build(Guid? workspaceId = null)
    {
        return new Faker<CreateUserCommand>()
            .CustomInstantiator(f =>
            {
                var type = f.PickRandom<UserType>();
                Guid resolvedWorkspaceId;

                if (type == UserType.SuperAdmin || type == UserType.InternalSupport)
                {
                    resolvedWorkspaceId = Guid.Empty;
                }
                else
                {
                    resolvedWorkspaceId = workspaceId ?? Guid.NewGuid();
                }

                return new CreateUserCommand(
                    f.Person.FullName,
                    f.Internet.Email(f.Person.FirstName),
                    type,
                    resolvedWorkspaceId
                );
            });
    }
}
