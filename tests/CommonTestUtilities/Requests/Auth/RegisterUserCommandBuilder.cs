using Bogus;
using DotCruz.CoreAuth.Application.Commands.Auth.Register;

namespace CommonTestUtilities.Requests.Auth;

public class RegisterUserCommandBuilder
{
    public static RegisterUserCommand Build(string? name = null, string? email = null)
    {
        return new Faker<RegisterUserCommand>()
            .CustomInstantiator(f => new RegisterUserCommand(
                name ?? f.Person.FullName,
                email ?? f.Internet.Email(f.Person.FirstName)
            ));
    }
}
