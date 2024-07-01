using Microsoft.EntityFrameworkCore;

namespace Spedition.Fuel.DataAccess.Infrastructure.Repositories.Interfaces
{
    public interface IRepository<T>
        where T : class
    {
        string Message { get; }

        long CountItems { get; }

        List<T> Get();

        Task<List<T>> GetAsync(CancellationToken token = default);

        Task<List<T>> GetAsync(int take, int? skip, CancellationToken token = default);

        IQueryable<T> GetIQueryable();

        Task<T> GetAsync(int id, CancellationToken token = default);

        Task<T> GetAsync(CancellationToken token = default, params object[] keyValues);

        T Add(T entity);

        Task<T> AddAsync(T entity, CancellationToken token = default);

        void Add(IEnumerable<T> entities);
        
        Task AddAsync(IEnumerable<T> entities, CancellationToken token = default);

        T Update(T entity);

        void Update(IEnumerable<T> entities);

        bool Remove(int id);

        bool Remove(params object[] keyValues);

        bool Remove(IEnumerable<int> ids);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default);

        bool Any(Func<T, bool> predicate);

        Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default);

        T FindFirst(Func<T, bool> predicate);

        Task<List<T>> FindRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default);

        List<T> FindRange(Func<T, bool> predicate);

        long FindCount(Expression<Func<T, bool>> predicate);

        void CleanTEntriesChangeTracker();
    }
}
