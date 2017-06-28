using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Carver.Tokens;
using Carver.Users;
using log4net;
using Nancy;
using Nancy.Security;

namespace Carver.API
{
    public class TokenModule : NancyModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TokenModule));

        public TokenModule() : base("/tokens")
        {
            this.RequiresHttps();

            var checkForClaims = new Func<Claim, List<UserGroup>, bool>((claim, allowed) =>
            {
                if (claim.Type != ClaimTypes.Role)
                    return false;

                return allowed.Select(x => Enum.GetName(typeof(UserGroup), x)).Contains(claim.Value);
            });

            Post("/", async (ctx, ct) =>
            {
                this.RequiresClaims(c => checkForClaims(c, new List<UserGroup> { UserGroup.user, UserGroup.admin } ));

                try
                {
                    await TokenActions.CreateNewToken(this.Request.Form.Description);
                }
                catch (Exception)
                {
                    return HttpStatusCode.InternalServerError;
                }

                return HttpStatusCode.OK;
            });

            Get("/{token}", async (ctx, ct) =>
            {
                this.RequiresClaims(c => checkForClaims(c, new List<UserGroup> { UserGroup.validator, UserGroup.admin }));

                if (await TokenActions.ValidTokenExists(ctx.username))
                    return HttpStatusCode.OK;

                return HttpStatusCode.NotFound;
            });
        }
    }
}
