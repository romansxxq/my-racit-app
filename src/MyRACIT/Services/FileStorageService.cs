using Microsoft.AspNetCore.Http;
using MyRACIT.Services.Interfaces;

namespace MyRACIT.Services
{
    /// <summary>
    /// Сервіс для роботи з файловою системою
    /// Зберігає файли у папці wwwroot/uploads
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;
        
        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
            
            // Створюємо папку uploads якщо не існує
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }
        
        /// <summary>
        /// Зберігає файл на диск
        /// </summary>
        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Файл порожній або не вибраний");
            }
            
            // Перевірка розміру (обмеження 10 MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSize)
            {
                throw new InvalidOperationException($"Розмір файлу перевищує 10 MB");
            }
            
            // Створюємо папку для категорії (assignments, submissions, тощо)
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            // Генеруємо унікальне ім'я файлу
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);
            
            // Зберігаємо файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Повертаємо відносний шлях (для збереження в БД)
            return Path.Combine("uploads", folder, fileName).Replace("\\", "/");
        }
        
        /// <summary>
        /// Отримує файл з диску
        /// </summary>
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Файл не знайдено: {filePath}");
            }
            
            return await File.ReadAllBytesAsync(fullPath);
        }
        
        /// <summary>
        /// Видаляє файл з диску
        /// </summary>
        public Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Перевіряє чи існує файл
        /// </summary>
        public bool FileExists(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            return File.Exists(fullPath);
        }
        
        /// <summary>
        /// Отримує розмір файлу в байтах
        /// </summary>
        public long GetFileSize(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            
            if (!File.Exists(fullPath))
            {
                return -1;
            }
            
            var fileInfo = new FileInfo(fullPath);
            return fileInfo.Length;
        }
        
        /// <summary>
        /// Отримує MIME-тип файлу за розширенням
        /// </summary>
        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
