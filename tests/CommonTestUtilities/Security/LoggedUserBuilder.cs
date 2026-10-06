using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using Moq;

namespace CommonTestUtilities.Security;

public class LoggedUserBuilder
{
    private readonly Mock<ILoggedUser> _mock = new();

    public LoggedUserBuilder SetupUser(User user)
    {
        _mock.Setup(x => x.User(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return this;
    }

    public ILoggedUser Build() => _mock.Object;
}
