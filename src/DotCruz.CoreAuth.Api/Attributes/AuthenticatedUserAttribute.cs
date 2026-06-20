using DotCruz.CoreAuth.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.CoreAuth.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthenticatedUserAttribute : TypeFilterAttribute
{
    public AuthenticatedUserAttribute() : base(typeof(AuthenticatedUserFilter)) { }
}
