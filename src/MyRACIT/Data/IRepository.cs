using System.Linq.Expressions;

namespace MyRACIT.Data
{
    /// <summary>
    /// Інтерфейс репозиторію для CRUD операцій з сутностями
    /// </summary>
    /// <typeparam name="T">Тип сутності</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Отримати сутність за ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Отримати всі сутності
        /// </summary>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Знайти сутності за умовою
        /// </summary>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Додати нову сутність
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Оновити існуючу сутність
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Видалити сутність
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Видалити сутність за ID
        /// </summary>
        Task DeleteByIdAsync(int id);

        /// <summary>
        /// Перевірити чи існує сутність за умовою
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Отримати кількість сутностей
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Зберегти зміни в базі даних
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
