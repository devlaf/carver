using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Carver.DataStore;
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
            // This module deliberately does not require SSL, as the expectation is that any users authorized with 
            // validate credentials is on the same physical machine.

            var checkForClaims = new Func<Claim, List<UserGroup>, bool>((claim, allowed) =>
            {
                return allowed.Select(x => Enum.GetName(typeof(UserGroup), x)).Contains(claim.Subject.AuthenticationType);
            });

            Post("/", async (ctx, ct) =>
            {
                this.RequiresClaims(c => checkForClaims(c, new List<UserGroup> { UserGroup.user, UserGroup.admin } ));

                if (this.Request.Form.description == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "description field missing." };

                try
                {
                    var token = await TokenActions.CreateNewToken(DataStoreFactory.TokenDataStore, this.Request.Form.Description);
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

                if (await TokenActions.ValidTokenExists(DataStoreFactory.TokenDataStore, ctx["token"]))
                    return HttpStatusCode.OK;

                return HttpStatusCode.NotFound;
            });
        }
    }
}
