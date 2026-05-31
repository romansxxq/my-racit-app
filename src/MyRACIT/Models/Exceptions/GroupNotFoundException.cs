namespace MyRACIT.Models.Exceptions;

/// <summary>
/// Виникає, коли група з вказаним ID не знайдена в базі даних.
/// </summary>
public class GroupNotFoundException : RacitException
{
    public int GroupId { get; }

    public GroupNotFoundException(int groupId)
        : base($"Групу з ID {groupId} не знайдено.")
    {
        GroupId = groupId;
    }
}
