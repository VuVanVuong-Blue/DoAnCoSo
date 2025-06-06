using System.Linq.Expressions;
using System.Threading.Tasks;
using System_Music.Models.SqlModels;

namespace System_Music.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync<TId>(TId id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync<TId>(TId id);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        Task<List<LikeTrack>> GetLikesByUserAsync(string userId); // Sửa từ int sang string
    }
}