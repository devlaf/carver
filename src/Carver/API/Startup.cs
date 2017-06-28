using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Nancy.Owin;

namespace Carver.API
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy(opt => opt.Bootstrapper = new StatelessAuthBootstrapper()));
        }
    }
}
