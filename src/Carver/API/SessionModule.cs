using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using Carver.Data.Models;

namespace Carver.API
{
    public class SessionModule : NancyModule
    {
        private readonly SessionTokens SessionTokenManager = new SessionTokens();

        public SessionModule() : base("/sessions")
        {
            this.RequiresHttps();

            Func<Claim, List<Permission>, bool> VerifyClaims = new Func<Claim, List<Permission>, bool>((_claim, _allowed) =>
            {
                return _allowed.Select(permission => Enum.GetName(typeof(Permission), permission)).Contains(_claim.Value);
            });

            Post("/", async (ctx, ct) =>
            {
                SessionLogin body = this.Bind<SessionLogin>();

                if (body.username == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "username field missing." };
                if (body.password == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "password field missing." };

                var token = await SessionTokenManager.ValidateUser(body.username, body.password);
                if (token == null)
                    return HttpStatusCode.Unauthorized;

                return this.Response.AsJson(new { session_token = token });
            });

            Delete("/", args =>
            {
                this.RequiresAuthentication();
                this.RequiresClaims(c => VerifyClaims(c, new List<Permission> { Permission.ManageSelfUser }));

                var sessionToken = this.Request.Query.session_token?.Value;;

                SessionTokenManager.InvalidateToken(sessionToken);
                return new Response { StatusCode = HttpStatusCode.OK };
            });
        }

        private class SessionLogin
        {
            public string username { get; set; }
            public string password { get; set; }
        }
    }
}
