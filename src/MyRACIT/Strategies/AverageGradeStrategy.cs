using MyRACIT.Models.Entities;

namespace MyRACIT.Strategies
{
    /// <summary>
    /// Стратегія для обчислення середнього арифметичного оцінок
    /// </summary>
    public class AverageGradeStrategy : IGradeStrategy
    {
        public string StrategyName => "Середній бал";

        public string Description => "Обчислення середнього арифметичного всіх оцінок";

        public double CalculateFinalGrade(List<Grade> grades)
        {
            if (grades == null || !grades.Any()) return 0.0;
            

            return Math.Round(grades.Average(g => g.Value), 2);
        }
    }
}
