using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.ActivateAccount;

public record ActivateAccountCommand(string Token, string Password) : IRequest<ResponseLoginDto>;
