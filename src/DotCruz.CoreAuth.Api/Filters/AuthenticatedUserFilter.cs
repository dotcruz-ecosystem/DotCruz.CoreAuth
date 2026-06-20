using DotCruz.CoreAuth.Application.DTOs.Base;
using DotCruz.CoreAuth.Application.Queries.Users.ValidateActiveUser;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace DotCruz.CoreAuth.Api.Filters;

public class AuthenticatedUserFilter : IAsyncAuthorizationFilter
{
    private readonly IMediator _mediator;
    private readonly IAccessTokenValidator _accessTokenValidator;

    public AuthenticatedUserFilter(IMediator mediator, IAccessTokenValidator accessTokenValidator)
    {
        _mediator = mediator;
        _accessTokenValidator = accessTokenValidator;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var token = TokenOnRequest(context);

            var userIdentifier = _accessTokenValidator.ValidateAndGetUserIdentifier(token);

            var exist = await _mediator.Send(new ValidateActiveUserQuery(userIdentifier), context.HttpContext.RequestAborted);
            
            if (!exist)
                throw new UnauthorizedException(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE);
        }
        catch (SecurityTokenExpiredException)
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponseDto(ResourceMessagesException.TOKEN_EXPIRED));
        }
        catch (CoreAuthException coreAuthException)
        {
            context.HttpContext.Response.StatusCode = (int)coreAuthException.GetStatusCode();
            context.Result = new ObjectResult(new ErrorResponseDto(coreAuthException.GetErrorsMessages()));
        }
        catch
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponseDto(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE));
        }
    }

    private static string TokenOnRequest(AuthorizationFilterContext context)
    {
        var authentication = context.HttpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authentication) || !authentication.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedException(ResourceMessagesException.NO_TOKEN);

        return authentication["Bearer ".Length..].Trim();
    }
}
