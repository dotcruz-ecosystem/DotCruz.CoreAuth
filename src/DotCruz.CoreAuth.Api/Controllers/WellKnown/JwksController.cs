using DotCruz.CoreAuth.Api.Controllers.Base;
using DotCruz.CoreAuth.Application.Queries.WellKnown.GetJwks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.CoreAuth.Api.Controllers.WellKnown;

[Route(".well-known")]
[ApiController]
[AllowAnonymous]
public class JwksController(IMediator mediator) : DotCruzCoreAuthBaseController(mediator)
{
    [HttpGet("jwks.json")]
    [ProducesResponseType(typeof(JwksResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJwks(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetJwksQuery(), cancellationToken));
    }
}
