namespace MyRACIT.Services.FileValidation
{
    /// <summary>
    /// Strategy Pattern — інтерфейс стратегії валідації файлів.
    /// Дозволяє мати різні правила для різних типів завантажень
    /// (завдання викладача, роботи студентів тощо) без зміни контролерів.
    /// </summary>
    public interface IFileValidationStrategy
    {
        /// <summary>
        /// Перевіряє файл за правилами конкретної стратегії.
        /// </summary>
        /// <param name="file">Файл для перевірки</param>
        /// <param name="error">Повідомлення про помилку (порожнє якщо файл валідний)</param>
        /// <returns>true — файл пройшов перевірку</returns>
        bool IsValid(IFormFile file, out string error);
    }
}
