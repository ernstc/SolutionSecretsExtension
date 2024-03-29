﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SolutionSecrets.Core.Repository;


namespace SolutionSecrets.Core
{

    public class SolutionSynchronizationSettings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RepositoryType Repository { get; set; }
        public string AzureKeyVaultName { get; set; }
    }


    public class SyncConfiguration : Dictionary<string, SolutionSynchronizationSettings>
    {

        const string APP_DATA_FILENAME = "configuration.json";


        private SyncConfiguration() { }



        public static readonly SolutionSynchronizationSettings DefaultSettings = new SolutionSynchronizationSettings
        {
            Repository = RepositoryType.GitHub
        };


        [JsonIgnore]
        public static SolutionSynchronizationSettings Default
        {
            get
            {
                if (Current.TryGetValue("default", out var settings))
                {
                    return settings;
                }
                else
                {
                    return DefaultSettings;
                }
            }
            set
            {
                if (value != null)
                {
                    Current["default"] = value;
                }
            }
        }


        public static SolutionSynchronizationSettings GetCustomSynchronizationSettings(Guid solutionGuid)
        {
            Refresh();
            string key = solutionGuid.ToString();
            if (Current.TryGetValue(key, out SolutionSynchronizationSettings value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }


        public static void SetCustomSynchronizationSettings(Guid solutionGuid, SolutionSynchronizationSettings settings)
        {
            Refresh();
            string key = solutionGuid.ToString();
            if (settings != null)
            {
                Current[key] = settings;
            }
            else
            {
                Current.Remove(key);
            }
        }


        private static SyncConfiguration _current;

        private static SyncConfiguration Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new SyncConfiguration();

                    var loadedConfiguration = AppData.LoadData<Dictionary<string, SolutionSynchronizationSettings>>(APP_DATA_FILENAME);
                    if (loadedConfiguration == null)
                    {
                        Default = SyncConfiguration.DefaultSettings;
                        AppData.SaveData<Dictionary<string, SolutionSynchronizationSettings>>(APP_DATA_FILENAME, _current);
                    }
                    else
                    {
                        foreach (var item in loadedConfiguration)
                        {
                            _current.Add(item.Key, item.Value);
                        }
                    }
                }
                return _current;
            }
        }


        public static void Refresh()
        {
            _current = null;
        }


        public static void Save()
        {
            AppData.SaveData<Dictionary<string, SolutionSynchronizationSettings>>(APP_DATA_FILENAME, Current);
        }

    }
}
