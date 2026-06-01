namespace MyRACIT.Services.FileValidation
{
    /// <summary>
    /// Конкретна стратегія для файлів робіт студентів (submission).
    /// Дозволено: PDF, DOCX, PPTX, ZIP, TXT, PNG, JPG — до 20 MB.
    /// Більш широкий перелік, ніж для завдань викладача.
    /// </summary>
    public class SubmissionFileValidator : IFileValidationStrategy
    {
        private static readonly HashSet<string> _allowed =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".docx", ".pptx", ".zip", ".txt", ".png", ".jpg", ".jpeg"
            };

        private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

        public bool IsValid(IFormFile file, out string error)
        {
            var ext = Path.GetExtension(file.FileName);

            if (!_allowed.Contains(ext))
            {
                error = $"Недозволений тип файлу '{file.FileName}'. Дозволено: PDF, DOCX, PPTX, ZIP, TXT, PNG, JPG.";
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                error = $"Файл '{file.FileName}' перевищує 20 MB.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
