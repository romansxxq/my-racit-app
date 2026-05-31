namespace MyRACIT.Models.Exceptions;

/// <summary>
/// Виникає, коли намагаються створити користувача з email, що вже існує в системі.
/// </summary>
public class UserAlreadyExistsException : RacitException
{
    public string Email { get; }

    public UserAlreadyExistsException(string email)
        : base($"Користувач з email '{email}' вже існує в системі.")
    {
        Email = email;
    }
}
