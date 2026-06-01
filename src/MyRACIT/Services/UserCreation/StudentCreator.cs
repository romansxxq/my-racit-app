using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;

namespace MyRACIT.Services.UserCreation
{
    /// <summary>
    /// Конкретний creator для студентів.
    /// Крок 2: перевіряє існування групи.
    /// Крок 4: створює StudentProfile.
    /// </summary>
    public class StudentCreator : UserCreatorBase
    {
        private readonly int _groupId;

        public StudentCreator(MyRacitDbContext context, int groupId) : base(context)
        {
            _groupId = groupId;
        }

        protected override UserRole GetRole() => UserRole.Student;

        protected override async Task ValidateDependenciesAsync()
        {
            var group = await _context.Groups.FindAsync(_groupId);
            if (group == null)
                throw new GroupNotFoundException(_groupId);
        }

        protected override async Task CreateProfileAsync(User user)
        {
            var profile = new StudentProfile { UserId = user.Id, GroupId = _groupId };
            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }
    }
}
