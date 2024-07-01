using System;
using System.Threading;
using Spedition.Fuel.Shared.Helpers;

namespace Spedition.Fuel.DataAccess
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly IDbContextFactory<SpeditionContext> contextFactory;
        private string message;

        public Repository(IDbContextFactory<SpeditionContext> contextFactory) => this.contextFactory = contextFactory;

        public virtual string Message => message ?? string.Empty;

        private SpeditionContext context;

        private SpeditionContext Context => context == null || string.IsNullOrEmpty(context?.ContextId.InstanceId.ToString()) ? context = contextFactory.CreateDbContext() : context;

        private SpeditionContext contextAsync;

        private SpeditionContext ContextAsync
        {
            get
            {
                if(contextAsync == null || string.IsNullOrEmpty(contextAsync?.ContextId.InstanceId.ToString()))
                {
                    var f = async () => await contextFactory?.CreateDbContextAsync();
                    return contextAsync = f.Invoke().GetAwaiter().GetResult();
                }
                else
                {
                    return contextAsync;
                }
            }
        }

        private DbSet<T> DbSet => Context?.Set<T>();

        private DbSet<T> DbSetAsync => ContextAsync?.Set<T>();

        public long CountItems => DbSet?.AsQueryable()?.LongCount() ?? 0;

        #region Read

        public virtual IQueryable<T> GetIQueryable()
        {
            return DbSet?.AsQueryable();
        }

        /// <summary>
        /// Метод для получения коллекции из заданного числа T-экземпляров с указанием, сколько T-экземпляров необходимо пропустить (пагинация).
        /// </summary>
        /// <param name="take">Число T-экземпляров, которые необходимо взять.</param>
        /// <param name="skip">Число T-экземпляров, которое необходимо пропустить.</param>
        /// <returns>Коллекция из заданного числа T-экземпляров.</returns>
        public virtual async Task<List<T>> GetAsync(int take, int? skip, CancellationToken token = default)
        {
            return (await DbSetAsync?.AsQueryable()?.Skip(skip ?? 0)?.Take(take)?.ToListAsync(token)) ?? new ();
        }

        public virtual List<T> Get()
        {
            return DbSet?.AsEnumerable()?.ToList() ?? new();
        }

        public virtual async Task<List<T>> GetAsync(CancellationToken token = default)
        {
            return (await DbSetAsync?.AsQueryable()?.ToListAsync(token)) ?? new ();
        }

        public virtual async Task<T> GetAsync(int id, CancellationToken token = default)
        {
            return DbSetAsync?.FindAsync(new object[] { id }, token).GetAwaiter().GetResult();
        }

        public virtual async Task<T> GetAsync(CancellationToken token = default, params object[] keyValues)
        {
            return DbSetAsync?.FindAsync(keyValues, token).GetAwaiter().GetResult();
        }
        #endregion

        #region Create

        /// <summary>
        /// Метод для добавления в БД новой сущности.
        /// </summary>
        /// <param Name="entity">Сущность, подлежащая сохранению в БД.</param>
        /// <returns>Добавленная в БД сущность.</returns>
        public virtual T Add(T entity)
        {
            T result;
            message = string.Empty;
            try
            {
                result = DbSet.Add(entity).Entity;
                Context.SaveChanges();
                message = $"Операция успешно завершена ! Данные добавлены в систему !";
            }
            catch (Exception exc)
            {
                CleanChangeTracker();
                exc.LogError(GetType().FullName, nameof(Add));
                message = $"Запись не была добавлена в систему, произошла ошибка на уровне базы данных ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }

            return result;
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken token = default)
        {
            T result;
            message = string.Empty;
            try
            {
                result = (await contextAsync.AddAsync(entity, token)).Entity;
                Context.SaveChanges();
                message = $"Операция успешно завершена ! Данные добавлены в систему !";
            }
            catch (Exception exc)
            {
                await CleanChangeTrackerAsync();
                exc.LogError(GetType().FullName, nameof(Add));
                message = $"Запись не была добавлена в систему, произошла ошибка на уровне базы данных ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }

            return result;
        }

        /// <summary>
        /// Метод для добавления в БД новой сущности.
        /// </summary>
        /// <param Name="entities">Коллекция сущностей, подлежащая сохранению в БД.</param>
        public virtual void Add(IEnumerable<T> entities)
        {
            message = string.Empty;
            try
            {
                DbSet.AddRange(entities);
                Context.SaveChanges();
                message = $"Операция успешно завершена ! Коллекция из {entities?.Count() ?? 0} элементов была добавлена в систему !";
            }
            catch (Exception exc)
            {
                CleanChangeTracker();
                exc.LogError(GetType().FullName, nameof(Add));
                message = $"Записи не были добавлены в систему, произошла ошибка на уровне базы данных ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }
        }

        public virtual async Task AddAsync(IEnumerable<T> entities, CancellationToken token = default)
        {
            message = string.Empty;
            try
            {
                await DbSet.AddRangeAsync(entities, token);
                Context.SaveChanges();
                message = $"Операция успешно завершена ! Коллекция из {entities?.Count() ?? 0} элементов была добавлена в систему !";
            }
            catch (Exception exc)
            {
                await CleanChangeTrackerAsync();
                exc.LogError(GetType().FullName, nameof(Add));
                message = $"Записи не были добавлены в систему, произошла ошибка на уровне базы данных ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Асинхронный метод для обновления в БД существующей сущности.
        /// </summary>
        /// <param Name="entity">Сущность, подлежащая обновлению в БД.</param>
        /// <returns>Обновленная в БД сущность.</returns>
        public virtual T Update(T entity)
        {
            message = string.Empty;
            T result = entity;
            try
            {
                result = DbSet?.Update(entity)?.Entity;
                Context?.SaveChanges();
                message = $"Операция успешно завершена ! Данные были обновлены в системе !";
            }
            catch (Exception exc)
            {
                CleanChangeTracker();
                exc.LogError(GetType().FullName, nameof(Update));
                message = $"Запись не была обновлена в системе, произошла ошибка на уровне базы данных ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }

            return result;
        }

        /// <summary>
        /// Асинхронный метод для обновления в БД коллекции существующих сущностей.
        /// </summary>
        /// <param Name="entities">Коллекция сущностей, подлежащая обновлению в БД.</param>
        public virtual void Update(IEnumerable<T> entities)
        {
            message = string.Empty;

            try
            {
                DbSet.UpdateRange(entities);
                Context?.SaveChanges();
                message = $"Операция успешно завершена ! Коллекция из {entities?.Count() ?? 0} элементов была обновлена в системе !";
            }
            catch (Exception exc)
            {
                CleanChangeTracker();
                exc.LogError(GetType().FullName, nameof(Update));
                message = $"Записи не были обновлены в системе, произошла ошибка на уровне базы данных ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Метод для удаления из БД выбранной сущности по ее идентификатору.
        /// </summary>
        /// <param Name="id">Идентификатор сущности, подлежазей удалению.</param>
        /// <returns>Результат удаленния сущности.</returns>
        public virtual bool Remove(int id)
        {
            message = string.Empty;
            var result = false;

            var item = DbSet?.Find(id);

            if (item != null)
            {
                try
                {
                    var resDel = DbSet?.Remove(item);
                    Context?.SaveChanges();
                    result = true;
                }
                catch (Exception exc)
                {
                    CleanChangeTracker();
                    exc.LogError(GetType().FullName, nameof(Remove));
                    message = $"Запись не подлежит удалению из системы, так как участвует в других записях ! " +
                              $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                              $"{exc.GetExeceptionMessages()} ! ";
                    throw;
                }
            }
            else
            {
                message = "Запись не найдена";
            }

            return result;
        }

        /// <summary>
        /// Метод для удаления из БД выбранной сущности по ее составному идентификатору.
        /// </summary>
        /// <param Name="keyValues">Составной идентификатор сущности.</param>
        /// <returns>Удаленная сущность.</returns>
        public virtual bool Remove(object[] keyValues)
        {
            message = string.Empty;
            var result = false;

            var item = DbSet?.Find(keyValues) ?? null;

            if (item != null)
            {
                try
                {
                    var resDel = DbSet?.Remove(item);
                    Context?.SaveChanges();
                    result = true;
                    message = $"Операция успешно завершена ! Данные были удалены из системы !";
                }
                catch (Exception exc)
                {
                    CleanChangeTracker();
                    exc.LogError(GetType().FullName, nameof(Remove));
                    message = $"Запись не подлежит удалению из системы, так как участвует в других записях ! " +
                              $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                              $"{exc.GetExeceptionMessages()} ! ";
                    throw;
                }
            }
            else
            {
                message = "Запись не найдена";
            }

            return result;
        }

        /// <summary>
        /// Метод для удаления из БД коллекции сущностей.
        /// </summary>
        /// <param Name="ids">Коллекция идентификатров сущностей, подлежащая удалению из БД.</param>
        public virtual bool Remove(IEnumerable<int> ids)
        {
            List<T> entities = new ();
            var result = false;

            if (ids != null)
            {
                ids?.ToList()?.ForEach(id => entities.Add(DbSet?.Find(id) ?? null));
            }

            entities.RemoveAll(entity => entity == null);

            try
            {
                DbSet?.RemoveRange(entities);
                Context?.SaveChanges();
                result = true;
                message = $"Операция успешно завершена ! Коллекция из {ids?.Count() ?? 0} элементов была удалена из системы !";
            }
            catch (Exception exc)
            {
                CleanChangeTracker();
                exc.LogError(GetType().FullName, nameof(Remove));
                message = $"Записи не подлежит удалению из системы, так как участвует в других записях ! " +
                          $"Можете обратиться в службу поддержки и оставить заявку на удаление ! " +
                          $"{exc.GetExeceptionMessages()} ! ";
                throw;
            }

            return result;
        }

        #endregion

        #region Filters

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
        {
            return await DbSetAsync?.AsQueryable()?.AnyAsync(predicate, token);
        }

        public virtual bool Any(Func<T, bool> predicate)
        {
            return DbSet?.AsEnumerable<T>()?.Any(predicate) ?? false;
        }

        public virtual async Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
        {
            return await DbSetAsync?.AsQueryable()?.FirstOrDefaultAsync(predicate, token);
        }

        public virtual T FindFirst(Func<T, bool> predicate)
        {
            return DbSet?.AsEnumerable()?.FirstOrDefault(predicate);
        }

        public virtual async Task<List<T>> FindRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
        {
            return await DbSetAsync?.AsQueryable()?.Where(predicate)?.ToListAsync(token) ?? new();
        }

        public virtual List<T> FindRange(Func<T, bool> predicate)
        {
            return DbSet?.AsEnumerable()?.Where(predicate)?.ToList() ?? new();
        }

        public virtual long FindCount(Expression<Func<T, bool>> predicate)
        {
            return DbSet?.AsQueryable()?.Where(predicate)?.LongCount() ?? 0;
        }
        #endregion

        #region Additional

        public void CleanTEntriesChangeTracker()
        {
            foreach (EntityEntry entityEntry in Context?.ChangeTracker?.Entries<T>()?.ToArray() ?? new EntityEntry[0])
            {
                if (entityEntry?.Entity != null)
                {
                    entityEntry.State = EntityState.Detached;
                }
            }
        }

        /// <summary>
        /// Detaches all of the EntityEntry objects that have been added to the ChangeTracker.
        /// </summary>
        /// <returns>Task.</returns>
        private void CleanChangeTracker()
        {
            foreach (EntityEntry entityEntry in Context?.ChangeTracker?.Entries()?.ToArray() ?? new EntityEntry[0])
            {
                if (entityEntry?.Entity != null)
                {
                    entityEntry.State = EntityState.Detached;
                }
            }
        }

        /// <summary>
        /// Detaches all of the EntityEntry objects that have been added to the ChangeTracker.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task CleanChangeTrackerAsync()
        {
            foreach (EntityEntry entityEntry in Context?.ChangeTracker?.Entries()?.ToArray() ?? new EntityEntry[0])
            {
                if (entityEntry?.Entity != null)
                {
                    entityEntry.State = EntityState.Detached;
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод для освобождения ресурсов, занятых контекстом.
        /// </summary>
        private void DisposeContext()
        {
            Context.Dispose();
        }

        /// <summary>
        /// Вспомогательный асинхронный метод для освобождения ресурсов, занятых контекстом.
        /// </summary>
        private async Task DisposeContextAsync()
        {
            await ContextAsync.DisposeAsync();
        }
        #endregion
    }
}
