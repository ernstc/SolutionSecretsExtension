using System;


namespace SolutionSecrets.Core
{

	public class HeaderFile
	{
        public string visualStudioSolutionSecretsVersion { get; set; }
		public DateTime lastUpload { get; set; }
        public string solutionFile { get; set; }
        public Guid? solutionGuid { get; set; }


        public bool IsVersionSupported()
        {
            try
            {
                Version headerVersion = new Version(visualStudioSolutionSecretsVersion);
                Version minVersion = new Version(Versions.MinFileFormatSupported);
                Version maxVersion = new Version(Versions.MaxFileFormatSupported);

                return minVersion.Major <= headerVersion.Major && headerVersion.Major <= maxVersion.Major;
            }
            catch
            {
                return false;
            }
        }

	}
}
