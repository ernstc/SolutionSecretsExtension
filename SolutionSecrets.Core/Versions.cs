using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core
{
    public static class Versions
    {

		public const string MinFileFormatSupported = "1.0.0";
        public const string MaxFileFormatSupported = "2.0.0";


        public static string VersionString { get; }
        public static Version CurrentVersion { get; }


        static Versions()
        {
            string version = typeof(Versions).Assembly?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            VersionString = version ?? "unknown";
            CurrentVersion = String.IsNullOrEmpty(version) ? new Version() : new Version(version.Split('-')[0]);
        }
    }
}
