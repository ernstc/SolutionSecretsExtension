using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SolutionSecrets.Core
{
    public static class AppData
    {

        private const string APP_DATA_FOLDER = @"Visual Studio Solution Secrets";


        public static T LoadData<T>(string fileName) where T: class, new()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_DATA_FOLDER, fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                }
                catch
                {
                }
            }
            return null;
        }


        public static void SaveData<T>(string fileName, T data) where T : class, new()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_DATA_FOLDER);
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            try
            {
                string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings 
                {
                    Formatting = Formatting.Indented
                });
                File.WriteAllText(filePath, JsonConvert.SerializeObject(data));
            }
            catch
            {
            }
        }

    }
}
