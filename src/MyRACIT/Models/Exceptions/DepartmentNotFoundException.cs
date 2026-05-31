namespace MyRACIT.Models.Exceptions;

/// <summary>
/// Виникає, коли кафедру з вказаним ID не знайдено в базі даних.
/// </summary>
public class DepartmentNotFoundException : RacitException
{
    public int DepartmentId { get; }

    public DepartmentNotFoundException(int departmentId)
        : base($"Кафедру з ID {departmentId} не знайдено.")
    {
        DepartmentId = departmentId;
    }
}
