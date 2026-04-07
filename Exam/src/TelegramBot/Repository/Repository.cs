using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TelegramBot.Repository
{
    internal class Repository<T> : IRepository<T>
    {

        private readonly string FilePath;

        public Repository(string fileName = "")
        {
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = typeof(T).Name + "s";
            }

            fileName = $"{fileName}.json";


            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName);

            if (!File.Exists(FilePath))
            {
                var stream = File.Create(FilePath);
                stream.Close();

            }
        }

  


        public async Task<List<T>> GetAllAsync()
        {
            var json = await File.ReadAllTextAsync(FilePath);
            if (string.IsNullOrEmpty(json))
            {
                return new List<T>();
            }

            var items = JsonSerializer.Deserialize<List<T>>(json);
            return items;
        }

        public async Task SaveAllAsync(List<T> items)
        {
            var json = JsonSerializer.Serialize(items);
            await File.WriteAllTextAsync(FilePath, json);
        }
    }
}
