using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace SolutionSecrets.Core
{
    public static class AppData
    {

        private static JsonSerializerOptions jsonSeriazerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };


        public static T LoadData<T>(string fileName) where T : class, new()
        {
            string filePath = Path.Combine(Context.Current.IO.GetApplicationDataFolderPath(), fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<T>(json, jsonSeriazerOptions);
                }
                catch
                { }
            }
            return null;
        }


        public static void SaveData<T>(string fileName, T data) where T : class, new()
        {
            string folderPath = Context.Current.IO.GetApplicationDataFolderPath();
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            try
            {
                string json = JsonSerializer.Serialize<T>(data, jsonSeriazerOptions);
                File.WriteAllText(filePath, json);
            }
            catch
            { }
        }

    }
}
