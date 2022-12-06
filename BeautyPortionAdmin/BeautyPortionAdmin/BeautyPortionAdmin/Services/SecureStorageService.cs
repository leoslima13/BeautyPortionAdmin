using BeautyPortionAdmin.Bootstraping;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace BeautyPortionAdmin.Services
{
    public interface ISecureStorageService
    {
        Task SetValue<T>(T value, string key) where T : class;
        Task<T> GetValue<T>(string key) where T : class;
        bool RemoveValue(string key);
    }

    [Singleton]
    public class SecureStorageService : ISecureStorageService
    {
        public async Task SetValue<T>(T value, string key) where T : class
        {
            try
            {
                var jsonValue = JsonConvert.SerializeObject(value);
                await SecureStorage.SetAsync(key, jsonValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when trying to set key: {key} on secure storage. {Environment.NewLine}Ex:{ex.Message}");
            }
        }

        public async Task<T> GetValue<T>(string key) where T : class
        {
            try
            {
                var jsonValue = await SecureStorage.GetAsync(key);
                if (string.IsNullOrEmpty(jsonValue))
                    return null;

                return JsonConvert.DeserializeObject<T>(jsonValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when trying to get key: {key} from secure storage. {Environment.NewLine}Ex:{ex.Message}");
                return default;
            }
        }

        public bool RemoveValue(string key)
        {
            return SecureStorage.Remove(key);
        }
    }
}
