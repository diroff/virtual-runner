using Abstractions.Interfaces;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Services
{
    public class JsonToFileStorageService : IStorageService
    {
        public async UniTask SaveAsync<T>(string key, T data)
        {
            string path = BuildPath(key);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(path, json);
        }

        public async UniTask<T> LoadAsync<T>(string key)
        {
            string path = BuildPath(key);

            if (!File.Exists(path))
                return default;

            string json = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private string BuildPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key);
        }
    }
}