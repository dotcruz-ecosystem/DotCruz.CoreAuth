using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using Moq;

namespace CommonTestUtilities.Services
{
    public class EmailServiceBuilder
    {
        public static IEmailService Build()
        {
            return new Mock<IEmailService>().Object;
        }
    }
}
