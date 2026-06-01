namespace MyRACIT.Models.Exceptions;

/// <summary>
/// Базовий клас для всіх доменних виключень системи My RACIT.
/// </summary>
public class RacitException : Exception
{
    public RacitException(string message) : base(message) { }
    public RacitException(string message, Exception innerException) : base(message, innerException) { }
}
