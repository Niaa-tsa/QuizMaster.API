using QuizMaster.Service.Exceptions;
using System.Security.Claims;

namespace QuizMaster.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
            throw new UnauthorizedException(
                "User ID was not found in the token.");

        return userId;
    }
}