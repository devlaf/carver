using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Carver.Logging
{
    internal static class Logger
    {
        /// <summary> Setup log4net preferences and file info. </summary>
        /// <remarks> Log4net requires manual config setup in .net core, as there is no app.config or assemblyinfo</remarks>
        internal static void InitLog4Net()
        {
            log4net.GlobalContext.Properties["ExecutingAssemblyDir"] = GetDirectoryOfExecutingAssembly();

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        private static string GetDirectoryOfExecutingAssembly()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetEntryAssembly().CodeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
