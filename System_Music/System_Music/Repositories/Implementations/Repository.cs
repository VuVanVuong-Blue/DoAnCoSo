using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System_Music.Models.SqlModels;
using System_Music.Repositories.Interfaces;

namespace System_Music.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SmartMusicDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(SmartMusicDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync<TId>(TId id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync<TId>(TId id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public Task<List<LikeTrack>> GetLikesByUserAsync(string userId)
        {
            return _context.LikeTracks
                  .Where(lt => lt.UserId == userId)
                  .Include(lt => lt.Track)
                  .ToListAsync();
        }
    }
}