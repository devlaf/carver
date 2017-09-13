using System.IO;
using log4net;
using Nancy;
using Nancy.Conventions;
using Carver.Config;

namespace Carver.API
{
    /// <summary>
    /// Establish the static content server (default directory: {executing_assembly_dir}/static_content/).
    /// Any static content must exist in a subdirectory of the application executable.  This is enforced 
    /// by the Nancy framework.
    /// </summary>
    partial class ApplicationBootstrapper : DefaultNancyBootstrapper
    {
        internal static string StaticContentRelativePath => Configuration.GetValue<string>("static_content_subdirectory", @"static_content");

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            var subdir = StaticContentRelativePath;

            if (Directory.Exists(subdir))
                Log.Info($"Static content directory was established at [{subdir}].");
            else
                Log.Error($"Static content directory, specified at [{subdir}] by configuration, does not exist.");

            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("static_content", subdir));
            base.ConfigureConventions(nancyConventions);
        }
    }

    /// <summary>
    /// Need to handle the index.html file seperately from the rest of static content, according to nancy conventions.
    /// </summary>
    public class IndexModule : NancyModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IndexModule));

        private readonly string IndexFileLocation;

        public IndexModule() : base("")
        {
            IndexFileLocation = Path.Combine(ApplicationBootstrapper.StaticContentRelativePath, "index.html");

            if (!File.Exists(IndexFileLocation))
                Log.Error($"The specified index.html file, at [{IndexFileLocation}], does not exist.");

            Get("/", parameters =>
            {
                return Response.AsFile(IndexFileLocation, "text/html");
            });
        }
    }
}
