using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using Nancy;
using Nancy.Conventions;

namespace Carver.API
{
    /// <summary>
    /// Establish the static content server (default directory: {executing_assembly_dir}/static_content/).
    /// Static content must exist in a subdirectory of the applicaiton executable.  This is enforced 
    /// by the Nancy framework.
    /// </summary>
    partial class ApplicationBootstrapper : DefaultNancyBootstrapper
    {
        internal static string StaticContentRelativePath => Configuration.GetValue<string>("static_content_subdirectory", @"static_content");

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            // Note that changing the static content directory via configuration will require a application restart.
            var subdir = StaticContentRelativePath;

            if (Directory.Exists(subdir))
                Log.Info($"The static content directory was established at [{subdir}].");
            else
                Log.Error($"The static content directory, specified at {subdir}] by configuration, does not exist.  Static Content requests will likely fail.");

            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("static_content", subdir));
            base.ConfigureConventions(nancyConventions);
        }
    }

    /// <summary>
    /// We have to handle the index.html file seperately from the rest of static content, according to nancy conventions.
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
