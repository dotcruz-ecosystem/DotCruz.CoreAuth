using Bogus;
using DotCruz.CoreAuth.Application.Commands.Users.CreateUser;
using DotCruz.CoreAuth.Domain.Enums.Users;
using System;

namespace CommonTestUtilities.Requests.Users;

public class CreateUserCommandBuilder
{
    public static CreateUserCommand Build()
    {
        return new Faker<CreateUserCommand>()
            .CustomInstantiator(f =>
            {
                var type = f.PickRandom<UserType>();

                return new CreateUserCommand(
                    f.Person.FullName,
                    f.Internet.Email(f.Person.FirstName),
                    type
                );
            });
    }
}
