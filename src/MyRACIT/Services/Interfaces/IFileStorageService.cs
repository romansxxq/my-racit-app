namespace MyRACIT.Services.Interfaces
{
    /// <summary>
    /// Інтерфейс для роботи з файловою системою
    /// Використовується для завантаження файлів завдань та робіт студентів
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Зберігає файл на диск
        /// </summary>
        /// <param name="file">Файл для збереження</param>
        /// <param name="folder">Папка призначення (assignments, submissions, тощо)</param>
        /// <returns>Відносний шлях до збереженого файлу</returns>
        Task<string> SaveFileAsync(IFormFile file, string folder);
        
        /// <summary>
        /// Отримує файл з диску
        /// </summary>
        /// <param name="filePath">Відносний шлях до файлу</param>
        /// <returns>Байтовий масив файлу</returns>
        Task<byte[]> GetFileAsync(string filePath);
        
        /// <summary>
        /// Видаляє файл з диску
        /// </summary>
        /// <param name="filePath">Відносний шлях до файлу</param>
        Task DeleteFileAsync(string filePath);
        
        /// <summary>
        /// Перевіряє чи існує файл
        /// </summary>
        /// <param name="filePath">Відносний шлях до файлу</param>
        /// <returns>true якщо файл існує</returns>
        bool FileExists(string filePath);
        
        /// <summary>
        /// Отримує розмір файлу в байтах
        /// </summary>
        /// <param name="filePath">Відносний шлях до файлу</param>
        /// <returns>Розмір файлу або -1 якщо файл не існує</returns>
        long GetFileSize(string filePath);
        
        /// <summary>
        /// Отримує MIME-тип файлу за розширенням
        /// </summary>
        /// <param name="fileName">Ім'я файлу</param>
        /// <returns>MIME-тип (наприклад, "application/pdf")</returns>
        string GetContentType(string fileName);
    }
}
