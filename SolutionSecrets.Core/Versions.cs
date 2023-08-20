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
        public static string CommitHash { get; }
        public static Version CurrentVersion { get; }


        static Versions()
        {
            string version = typeof(Versions).Assembly?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            string[] versionParts = version?.Split('+');

            VersionString = versionParts?.Length > 0 ? versionParts[0] : "unknown";
            CurrentVersion = String.IsNullOrEmpty(version) ? new Version() : new Version(VersionString.Split('-')[0]);
            CommitHash = versionParts?.Length > 1 ? versionParts[1].Substring(0, 8) : null;
        }
    }
}
