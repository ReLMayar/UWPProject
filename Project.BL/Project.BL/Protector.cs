using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project.BL
{
    public class Protector
    {
        #region Работа с кэшем

        /// <summary>
        /// Сохранение данных в кэш.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Данные.</param>
        public async static void SaveData<T>(T data)
        {
            string jsonData = JsonConvert.SerializeObject(data);

            var res = Task.Run(async () =>
               {
                   StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                   StorageFile sampleFile = await storageFolder.CreateFileAsync(data.GetType().ToString().ToLower(), CreationCollisionOption.ReplaceExisting);
                   await FileIO.WriteTextAsync(sampleFile, jsonData);
               });
            await res;
        }

        private static string _data;
        private async static void getText<T>(T data)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile sampleFile = await storageFolder.GetFileAsync(data.GetType().ToString().ToLower());
                _data = await FileIO.ReadTextAsync(sampleFile);
            }
            catch { }
        }

        /// <summary>
        /// Получение данных из кэша.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Данные.</param>
        public static void GetData<T>(ref T data)
        {
            try
            {
                getText(data);
                data = JsonConvert.DeserializeObject<T>(_data);
                _data = null;
            }
            catch { }
        }

        #endregion
    }
}
