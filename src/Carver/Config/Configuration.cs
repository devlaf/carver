using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Carver.Config
{
    /// <summary>
    /// Configuration info, managed via the appsettings.json file.
    /// </summary>
    internal class Configuration
    {
        private static readonly Lazy<IConfigurationRoot> lazyConfig = new Lazy<IConfigurationRoot>(() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true).Build()
        );

        private static IConfigurationRoot Config => lazyConfig.Value;

        static Configuration() { }

        /// <summary>Get the config value associated with the given key.</summary>
        /// <returns>Value of type T associated with the given KEY</returns>
        /// <exception cref="KeyNotFoundException">Specified key does not exist.</exception>
        /// <exception cref="FileNotFoundException">appsettings.json does not exist.</exception>
        /// <exception cref="FormatException">appsettings.json can not be parsed.</exception>
        /// <exception cref="InvalidCastException">Specified type T is not compatible with the returned value.</exception>
        /// <exception cref="OverflowException">Returned value is out of range of the specified type T.</exception>
        public static T GetValue<T>(string KEY)
        {
            if (Config[KEY] == null)
                throw new KeyNotFoundException($"Key [{KEY}] does not exist in configuration");

            return (T)Convert.ChangeType(Config[KEY], typeof(T));
        }

        /// <summary>Get the config value associated with the given key.</summary>
        /// <param name="defaultValue">The value to return if the KEY does not exist.</param>
        /// <returns>Value of type T associated with the given KEY</returns>
        /// <exception cref="FileNotFoundException">appsettings.json does not exist.</exception>
        /// <exception cref="FormatException">appsettings.json can not be parsed.</exception>
        /// <exception cref="InvalidCastException">Specified type T is not compatible with the returned value.</exception>
        /// <exception cref="OverflowException">Returned value is out of range of the specified type T.</exception>
        public static T GetValue<T>(string KEY, T defaultValue)
        {
            if (Config[KEY] == null)
                return defaultValue;

            return GetValue<T>(KEY);
        }
    }
}