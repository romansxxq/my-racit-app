using MyRACIT.Models.Entities;

namespace MyRACIT.Strategies
{
    /// <summary>
    /// Стратегія для обчислення зваженої оцінки
    /// Враховує максимальний бал завдання як вагу
    /// </summary>
    public class WeightedGradeStrategy : IGradeStrategy
    {
        public string StrategyName => "Зважений бал";

        public string Description => "Обчислення зваженого середнього з урахуванням важливості завдань (MaxGrade)";

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

            // Обчислюємо зважену суму: (оцінка / макс.бал завдання) * макс.бал завдання
            // Це дає відсотковий результат для кожного завдання, помножений на його вагу
            double totalWeightedScore = 0;
            int totalWeight = 0;

            foreach (var grade in grades)
            {
                // Нормалізуємо оцінку до відсотків
                double percentage = (double)grade.Value / grade.Assignment.MaxGrade;
                
                // Додаємо зважений результат
                totalWeightedScore += percentage * grade.Assignment.MaxGrade;
                totalWeight += grade.Assignment.MaxGrade;
            }

            // Обчислюємо фінальний бал (нормалізуємо до 5.0)
            double finalGrade = (totalWeightedScore / totalWeight) * 5.0;

            return Math.Round(finalGrade, 2);
        }
    }
}
