using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Security;
using Nancy.TinyIoc;

namespace Carver.API
{
    class StatelessAuthBootstrapper : DefaultNancyBootstrapper
    {
        /// <summary>
        /// I'm tentatively using nginx to manage ssl forwarding.  The below modification to the behavior of the 
        /// application bootstrapper enables Nancy to listen for the XForwardedProto header.  See 
        /// http://stackoverflow.com/questions/29634033/nancyfx-ssl-how-to-make-this-requirehttps-work-on-linux
        /// </summary>
        protected override void RequestStartup(TinyIoCContainer requestContainer, IPipelines pipelines, NancyContext context)
        {
            var authConfiguration = new StatelessAuthenticationConfiguration(nancyContext =>
                {
                    var apiKey = (string)nancyContext.Request.Query.ApiKey.Value;
                    return SessionTokens.GetUserFromApiKey(apiKey).Result;
                });

            AllowAccessToConsumingSite(pipelines);
            SSLProxy.RewriteSchemeUsingForwardedHeaders(pipelines);
            StatelessAuthentication.Enable(pipelines, authConfiguration);
        }

        static void AllowAccessToConsumingSite(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(x =>
            {
                x.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                x.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
            });
        }
    }
}
