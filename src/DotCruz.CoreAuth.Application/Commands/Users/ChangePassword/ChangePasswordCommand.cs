using MediatR;
using System;

namespace DotCruz.CoreAuth.Application.Commands.Users.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;
