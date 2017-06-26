using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Config;

namespace Carver
{
    /// <summary>
    /// Setup Log4net logging using log4net.config
    /// </summary>
    internal static class Logging
    {
        /// <summary> Setup log4net preferences and file info. </summary>
        /// <remarks> Log4net requires manual config setup in .net core, as there is no app.config or assemblyinfo</remarks>
        internal static void InitLog4Net()
        {
            // Using this property, we can pass the directory of the executing assembly to the log4net.config 
            // file.  We use this as the base directory for the log file folder.
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
