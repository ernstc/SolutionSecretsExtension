using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SolutionSecrets.Core.Encryption;


namespace SolutionSecrets.Core
{

    public class ConfigFile
    {

        private readonly string _configFilePath;
        private readonly string _uniqueFileName;
        private readonly ICipher _cipher;


        public string FullName => _configFilePath;
        public string UniqueFileName => _uniqueFileName;
        public string Content { get; set; }
        public string ProjectFileName { get; set; }


        class EncryptedContent
        {
            public string content { get; set; }
        }


        public ConfigFile(string configFilePath, string uniqueFileName, ICipher cipher)
        {
            _configFilePath = configFilePath;
            _uniqueFileName = uniqueFileName;
            _cipher = cipher;            
        }


        public ConfigFile(string configFilePath, string uniqueFileName, string content, ICipher cipher)
        {
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
                    Content = JsonConvert.SerializeObject(new EncryptedContent
                    {
                        content = encryptedContent
                    });
                    return true;
                }
            }
            return false;
        }


        public bool Decrypt()
        {
            if (_cipher != null && Content != null)
            {
                var encryptedContent = JsonConvert.DeserializeObject<EncryptedContent>(Content);
                Content = _cipher.Decrypt(_uniqueFileName, encryptedContent.content);
                return Content != null;
            }
            return false;
        }

    }
}
