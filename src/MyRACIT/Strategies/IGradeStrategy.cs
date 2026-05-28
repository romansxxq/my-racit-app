using MyRACIT.Models.Entities;

namespace MyRACIT.Strategies
{
    /// <summary>
    /// Інтерфейс стратегії для обчислення фінальної оцінки
    /// Реалізує Strategy Pattern
    /// </summary>
    public interface IGradeStrategy
    {
        /// <summary>
        /// Обчислити фінальну оцінку на основі списку оцінок
        /// </summary>
        /// <param name="grades">Список оцінок студента за завданнями</param>
        /// <returns>Фінальна оцінка (0.0 - 5.0)</returns>
        double CalculateFinalGrade(List<Grade> grades);

        /// <summary>
        /// Назва стратегії для відображення
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Опис стратегії
        /// </summary>
        string Description { get; }
    }
}
