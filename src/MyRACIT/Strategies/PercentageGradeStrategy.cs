using MyRACIT.Models.Entities;

namespace MyRACIT.Strategies
{
    /// <summary>
    /// Стратегія для обчислення оцінки на основі відсотка виконання
    /// </summary>
    public class PercentageGradeStrategy : IGradeStrategy
    {
        public string StrategyName => "Відсотковий бал";

        public string Description => "Обчислення на основі загального відсотка виконання завдань";

        public double CalculateFinalGrade(List<Grade> grades)
        {
            if (grades == null || !grades.Any())
            {
                return 0.0;
            }

            // Перевіряємо що всі завдання мають Assignment
            if (grades.Any(g => g.Assignment == null))
            {
                throw new InvalidOperationException("Не всі оцінки мають прив'язку до завдань");
            }

            // Обчислюємо загальний відсоток виконання
            int totalEarned = 0;
            int totalPossible = 0;

            foreach (var grade in grades)
            {
                totalEarned += grade.Value;
                totalPossible += grade.Assignment.MaxGrade;
            }

            if (totalPossible == 0)
            {
                return 0.0;
            }

            // Відсоток виконання
            double percentage = (double)totalEarned / totalPossible * 100;

            // Конвертуємо відсоток у бал від 0 до 5
            // 0-59% = 2, 60-74% = 3, 75-89% = 4, 90-100% = 5
            double finalGrade = percentage switch
            {
                >= 90 => 5.0,
                >= 75 => 4.0,
                >= 60 => 3.0,
                >= 50 => 2.0,
                _ => 0.0
            };

            return finalGrade;
        }
    }
}
