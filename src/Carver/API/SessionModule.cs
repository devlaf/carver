using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Nancy.Security;

namespace Carver.API
{
    public class SessionModule : NancyModule
    {
        public SessionModule() : base("/sessions")
        {
            this.RequiresHttps();

            Post("/", async (ctx, ct) =>
            {
                var userName = this.Request.Form.UserName;
                var password = this.Request.Form.Password;

                if (userName == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "UserName field missing." };
                if (password == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "Password field missing." };

                var token = await SessionTokens.ValidateUser((string)userName, (string)password);
                if (token == null)
                    return HttpStatusCode.Unauthorized;

                return this.Response.AsJson(new { Token = token });
            });

            Delete("/", args =>
            {
                var apiKey = (string)this.Request.Form.ApiKey;
                SessionTokens.RemoveApiKey(apiKey);
                return new Response { StatusCode = HttpStatusCode.OK };
            });
        }
    }
}
