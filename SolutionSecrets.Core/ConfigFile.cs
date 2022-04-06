using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolutionSecrets.Core.Encryption;


namespace SolutionSecrets.Core
{

    public class ConfigFile
    {

        private readonly string _fileName;
        private readonly string _configFilePath;
        private readonly string _uniqueFileName;
        private readonly ICipher _cipher;


        public string GroupName => _uniqueFileName;
        public string FileName => _fileName;
        public string FilePath => _configFilePath;
        public string Content { get; set; }
        public string ProjectFileName { get; set; }



        public ConfigFile(string configFilePath, string uniqueFileName, ICipher cipher)
        {
            FileInfo fileInfo = new FileInfo(configFilePath);

            _fileName = fileInfo.Name;
            _configFilePath = configFilePath;
            _uniqueFileName = uniqueFileName;
            _cipher = cipher;
        }


        public ConfigFile(string configFilePath, string uniqueFileName, string content, ICipher cipher)
        {
            FileInfo fileInfo = new FileInfo(configFilePath);

            _fileName = fileInfo.Name;
            _configFilePath = configFilePath;
            _uniqueFileName = uniqueFileName;
            Content = content;
            _cipher = cipher;
        }


        public void Load()
        {
            if (File.Exists(_configFilePath))
            {
                Content = File.ReadAllText(_configFilePath);
            }
        }


        public bool Encrypt()
        {
            if (_cipher != null && Content != null)
            {
                var encryptedContent = _cipher.Encrypt(_uniqueFileName, Content);
                if (encryptedContent != null)
                {
                    Content = encryptedContent;
                    return true;
                }
            }
            return false;
        }


        public bool Decrypt()
        {
            if (_cipher != null && Content != null)
            {
                Content = _cipher.Decrypt(_uniqueFileName, Content);
                return Content != null;
            }
            return false;
        }

    }
}
