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
            //this.RequiresHttps();

            var checkForClaims = new Func<Claim, List<UserGroup>, bool>((claim, allowed) =>
            {
                return allowed.Select(x => Enum.GetName(typeof(UserGroup), x)).Contains(claim.Subject.AuthenticationType);
            });

            Post("/", async (ctx, ct) =>
            {
                this.RequiresClaims(c => checkForClaims(c, new List<UserGroup> { UserGroup.user, UserGroup.admin } ));

                try
                {
                    var token = await TokenActions.CreateNewToken(this.Request.Form.Description);
                    return token;
                }
                catch (Exception)
                {
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/{token}", async (ctx, ct) =>
            {
                this.RequiresClaims(c => checkForClaims(c, new List<UserGroup> { UserGroup.validator, UserGroup.admin }));

                if (await TokenActions.ValidTokenExists(ctx["token"]))
                    return HttpStatusCode.OK;

                return HttpStatusCode.NotFound;
            });
        }
    }
}
