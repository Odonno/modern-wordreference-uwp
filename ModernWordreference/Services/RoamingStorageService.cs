using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernWordreference.Services
{
    public interface IRoamingStorageService
    {
        T Retrieve<T>(string key);
        void Save(string key, string value);
        void Save<T>(string key, T value);

        Task<T> RetrieveFileAsync<T>(string filePath);
        Task SaveFileAsync(string filePath, string value);
        Task SaveFileAsync<T>(string filePath, T value);

    }

    public class RoamingStorageService : IRoamingStorageService
    {
        private ApplicationDataContainer _roamingSettings = ApplicationData.Current.RoamingSettings;
        private StorageFolder _roamingFolder = ApplicationData.Current.RoamingFolder;
        private JsonSerializer _serializer = new JsonSerializer();

        public T Retrieve<T>(string key)
        {
            string value = (string)_roamingSettings.Values[key];
            if (value != null)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return default(T);
        }        

        public void Save(string key, string value)
        {
            _roamingSettings.Values[key] = value;
        }        

        public void Save<T>(string key, T value)
        {
            Save(key, JsonConvert.SerializeObject(value));
        }

        public async Task<T> RetrieveFileAsync<T>(string filePath)
        {
            var file = await _roamingFolder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                string value = await FileIO.ReadTextAsync(file);
                if (value != null)
                {
                    return JsonConvert.DeserializeObject<T>(value);
                }
            }
            return default(T);
        }

        public async Task SaveFileAsync(string filePath, string value)
        {
            var file = await _roamingFolder.CreateFileAsync(filePath, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, value);
        }

        public async Task SaveFileAsync<T>(string filePath, T value)
        {
            await SaveFileAsync(filePath, JsonConvert.SerializeObject(value));
        }
    }
}
