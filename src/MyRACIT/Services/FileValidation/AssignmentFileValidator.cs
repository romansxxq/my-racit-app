namespace MyRACIT.Services.FileValidation
{
    /// <summary>
    /// Конкретна стратегія для файлів завдань (викладач завантажує матеріали).
    /// Дозволено: PDF, DOCX, PPTX — до 10 MB.
    /// </summary>
    public class AssignmentFileValidator : IFileValidationStrategy
    {
        private static readonly HashSet<string> _allowed =
            new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".pptx" };

        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public bool IsValid(IFormFile file, out string error)
        {
            var ext = Path.GetExtension(file.FileName);

            if (!_allowed.Contains(ext))
            {
                error = $"Недозволений тип файлу '{file.FileName}'. Дозволено: PDF, DOCX, PPTX.";
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                error = $"Файл '{file.FileName}' перевищує 10 MB.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
