using MyRACIT.Data;
using MyRACIT.Models.Entities;
using MyRACIT.Models.Exceptions;

namespace MyRACIT.Services.UserCreation
{
    /// <summary>
    /// Конкретний creator для викладачів.
    /// Крок 2: перевіряє існування кафедри.
    /// Крок 4: створює TeacherProfile.
    /// </summary>
    public class TeacherCreator : UserCreatorBase
    {
        private readonly int _departmentId;

        public TeacherCreator(MyRacitDbContext context, int departmentId) : base(context)
        {
            _departmentId = departmentId;
        }

        protected override UserRole GetRole() => UserRole.Teacher;

        protected override async Task ValidateDependenciesAsync()
        {
            var department = await _context.Departments.FindAsync(_departmentId);
            if (department == null)
                throw new DepartmentNotFoundException(_departmentId);
        }

        protected override async Task CreateProfileAsync(User user)
        {
            var profile = new TeacherProfile { UserId = user.Id, DepartmentId = _departmentId };
            _context.TeacherProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }
    }
}
