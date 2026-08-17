using DotCruz.CoreAuth.Api.Controllers.Base;
using DotCruz.CoreAuth.Application.Commands.Auth.ActivateAccount;
using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Application.Commands.Auth.PasswordResetTokens.RequestPasswordReset;
using DotCruz.CoreAuth.Application.Commands.Auth.PasswordResetTokens.ResetPassword;
using DotCruz.CoreAuth.Application.Commands.Auth.RefreshTokens;
using DotCruz.CoreAuth.Application.Commands.Auth.RevokeAllUserTokens;
using DotCruz.CoreAuth.Application.DTOs.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.CoreAuth.Api.Controllers.Auth;

public class AuthController(IMediator mediator) : DotCruzCoreAuthBaseController(mediator)
{
    [HttpPost]
    [AllowAnonymous]
    [Route("login")]
    [ProducesResponseType(typeof(ResponseLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("refresh-token")]
    [ProducesResponseType(typeof(ResponseTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("password-reset/request")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("password-reset/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [Route("revoke-tokens")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeTokens(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeAllUserTokensCommand(), cancellationToken);
        return NoContent();
    }
}
