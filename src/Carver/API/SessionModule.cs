using Nancy;

namespace Carver.API
{
    public class SessionModule : NancyModule
    {
        public SessionModule() : base("/sessions")
        {
            //this.RequiresHttps();

            Post("/", async (ctx, ct) =>
            {
                var userName = this.Request.Form.username;
                var password = this.Request.Form.password;

                if (userName == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "username field missing." };
                if (password == null)
                    return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "password field missing." };

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
