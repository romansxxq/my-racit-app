using MyRACIT.Data;
using MyRACIT.Models.Entities;

namespace MyRACIT.Services.UserCreation
{
    /// <summary>
    /// Конкретний creator для адміністраторів.
    /// Крок 2: немає залежностей для перевірки.
    /// Крок 4: немає профілю для створення.
    /// </summary>
    public class AdminCreator : UserCreatorBase
    {
        public AdminCreator(MyRacitDbContext context) : base(context) { }

        protected override UserRole GetRole() => UserRole.Admin;

        protected override Task ValidateDependenciesAsync() => Task.CompletedTask;

        protected override Task CreateProfileAsync(User user) => Task.CompletedTask;
    }
}
