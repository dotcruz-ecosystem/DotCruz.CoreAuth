using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using DotCruz.CoreAuth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotCruz.CoreAuth.Infrastructure.Services.LoggedUser;

public class LoggedUser : ILoggedUser
{
    private readonly CoreAuthDbContext _context;
    private readonly IAuthTokenProvider _tokenProvider;

    public LoggedUser(
        CoreAuthDbContext context,
        IAuthTokenProvider tokenProvider
    )
    {
        _context = context;
        _tokenProvider = tokenProvider;
    }

    public async Task<User> User()
    {
        var token = _tokenProvider.Value();

        var tokenHandler = new JwtSecurityTokenHandler();

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

        var userIdClaim = jwtSecurityToken.Claims.First(c => c.Type == ClaimTypes.Sid).Value;

        var userId = Guid.Parse(userIdClaim);

        return await _context
            .Users
            .AsNoTracking()
            .FirstAsync(user => user.Id == userId && user.DeletedAt == null);
    }
}
