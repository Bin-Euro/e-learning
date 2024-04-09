using Cursus.Constants;
using Cursus.Data;
using Cursus.DTO;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cursus.Authentication;

public class ApplicationJwtBearEvents : JwtBearerEvents
{
    public ApplicationJwtBearEvents()
    {
        OnChallenge = _onChallenge;
        OnTokenValidated = _onTokenValidated;
        OnForbidden = _onForbidden;
    }

    private readonly Func<JwtBearerChallengeContext, Task> _onChallenge =
        async delegate(JwtBearerChallengeContext context)
        {
            var ex = context.AuthenticateFailure;

            await Task.Run(() => context.Response.OnStarting(async () =>
            {
                var message = "";
                if (ex is not null)
                {
                    message = "Invalid token";
                    if (ex.GetType() == typeof(SecurityTokenExpiredException))
                        message = "Expired token";
                }

                await context.Response.WriteAsJsonAsync(ResultDTO<string>.Fail(message, context.Response.StatusCode));
            }));
        };

    private readonly Func<TokenValidatedContext, Task> _onTokenValidated = async delegate(TokenValidatedContext context)
    {
        var userIdClaim = context.Principal?.Claims.FirstOrDefault(claim => claim.Type == "Id");
        if (userIdClaim is not null)
        {
            var redisService = context.HttpContext.RequestServices.GetService<IRedisService>();
            var key = CacheKeyPatterns.User + userIdClaim.Value;

            var user = redisService is not null ? await redisService.GetDataAsync<User>(key) : null;

            if (user is null)
            {
                var dbContext = context.HttpContext.RequestServices.GetService<MyDbContext>();
                try
                {
                    user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdClaim.Value);
                    if (user is null)
                    {
                        context.Fail("Invalid token");
                        return;
                    }

                    if (redisService is not null)
                        await redisService.SetDataAsync(key, user, TimeSpan.FromDays(1));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (user is not null && user.Status != Enum.GetName(UserStatus.Disable))
            {
                context.Success();
                return;
            }
        }

        context.Fail("Invalid token");
    };

    private readonly Func<ForbiddenContext, Task> _onForbidden = async delegate(ForbiddenContext context)
    {
        await Task.Run(() => context.Response.OnStarting(async Task() =>
        {
            await context.Response.WriteAsJsonAsync(ResultDTO<string>.Fail(
                    "You are not allow to access this resource", context.Response.StatusCode
                )
            );
        }));
    };
}