using Gorilla.DDD.Extensions;
using Gorilla.DDD.Pagination;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gorilla.DDD
{

    public delegate Task BeforeSaveEventHandler<T>(object sender, T e);
    public delegate Task BeforePersistEventHandler<T>(object sender, T e);
    public delegate Task BeforeDeleteEventHandler<T>(object sender, T e);

    public delegate Task AfterSaveEventHandler<T>(object sender, T e);
    public delegate Task AfterPersistEventHandler<T>(object sender, T e);
    public delegate Task AfterDeleteEventHandler<T>(object sender, T e);

    public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TContext : DbContext, IContext
        where TEntity : Entity
        where TKey : IComparable
    {
        protected TContext _context;

        public event BeforeSaveEventHandler<TEntity> BeforeSave;
        public event BeforePersistEventHandler<TEntity> BeforePersist;
        public event BeforeDeleteEventHandler<TEntity> BeforeDelete;

        public event AfterSaveEventHandler<TEntity> AfterSave;
        public event AfterPersistEventHandler<TEntity> AfterPersist;
        public event AfterDeleteEventHandler<TEntity> AfterDelete;

        [Inject]
        public void InjectDependencies(TContext context)
        {
            _context = context;
        }

        public DbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public virtual async Task<TEntity> Find(TKey id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<List<TEntity>> All()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {

            await OnBeforePersist(entity);
            await OnBeforeSave(entity);

            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();

            await OnAfterPersist(entity);
            await OnAfterSave(entity);

            return entity;
        }

        public virtual async Task<TEntity> Update(TEntity entity)
        {
            await OnBeforeSave(entity);

            try
            {
                _context.EnableAutoDetectChanges();
                await _context.SaveChangesAsync();

                await OnAfterSave(entity);
            }
            finally
            {
                _context.DisableAutoDetectChanges();
            }

            return entity;
        }

        public virtual async Task<bool> Remove(TKey id)
        {
            var entity = await Find(id);

            if (entity == null)
            {
                throw new ArgumentException("Entity not found");
            }

            return await Remove(entity);
        }

        public virtual async Task<bool> Remove(TEntity entity)
        {
            await OnBeforeDelete(entity);

            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();

            await OnAfterDelete(entity);

            return true;
        }

        /// <summary>
        /// Converts the result to a TResult entity
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="exp">Where Clause</param>
        /// <param name="columns">columns mapping</param>
        public virtual async Task<List<TResult>> SelectBy<TResult>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns)
        {
            return await QueryBy<TResult>(exp, columns).ToListAsync();
        }

        public virtual async Task<List<TEntity>> SelectBy(Expression<Func<TEntity, bool>> exp)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = AddFixedConditions(query);

            return await query.Where(exp)
                              .ToListAsync();
        }

        public virtual async Task<List<TResult>> Select<TResult>(Expression<Func<TEntity, TResult>> columns)
        {
            return await Query(columns).ToListAsync();
        }

        public virtual IQueryable<TResult> QueryBy<TResult>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = AddFixedConditions(query);

            return query.Where(exp)
                        .Select<TEntity, TResult>(columns);
        }

        public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);
            return query.Where(exp);
        }

        public virtual IQueryable<U> Query<U>(Expression<Func<TEntity, U>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);
            return query.Select<TEntity, U>(columns);
        }

        public virtual async Task<PagedResult<TResult>> SelectPagedBy<TResult>(PaginationSettings settings, Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);

            var total = await query.CountAsync();

            query = query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString());

            var data = await query.Where(exp)
                         .Select<TEntity, TResult>(columns)
                         .Skip(settings.Skip)
                         .Take(settings.Take)
                         .ToListAsync();

            return new PagedResult<TResult>(settings.Page, total, settings.Page, data);
        }

        public virtual async Task<PagedResult<TEntity>> SelectPagedBy(PaginationSettings settings, Expression<Func<TEntity, bool>> exp)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);

            var total = await query.CountAsync();

            query = query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString());

            var data = await query.Where(exp)
                         .Skip(settings.Skip)
                         .Take(settings.Take)
                         .ToListAsync();

            return new PagedResult<TEntity>(settings.Page, total, settings.Page, data);
        }

        public virtual async Task<PagedResult<TResult>> SelectPaged<TResult>(PaginationSettings settings, Expression<Func<TEntity, TResult>> columns)
        {

            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);

            var total = await query.CountAsync();

            query = query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString());

            var data = await query.Select<TEntity, TResult>(columns)
                         .Skip(settings.Skip)
                         .Take(settings.Take)
                         .ToListAsync();

            return new PagedResult<TResult>(settings.Page, total, settings.Page, data);
        }

        protected virtual IQueryable<TEntity> AddFixedConditions(IQueryable<TEntity> query)
        {
            return query;
        }

        protected async Task OnBeforePersist(TEntity entity)
        {
            if (BeforePersist != null)
            {
                await BeforePersist(this, entity);
            }
        }

        protected async Task OnBeforeSave(TEntity entity)
        {
            if (BeforeSave != null)
            {
                await BeforeSave(this, entity);
            }
        }

        protected async Task OnBeforeDelete(TEntity entity)
        {
            if (BeforeDelete != null)
            {
                await BeforeDelete(this, entity);
            }
        }

        protected async Task OnAfterPersist(TEntity entity)
        {
            if (AfterPersist != null)
            {
                await AfterPersist(this, entity);
            }
        }

        protected async Task OnAfterSave(TEntity entity)
        {
            if (AfterSave != null)
            {
                await AfterSave(this, entity);
            }
        }
        protected async Task OnAfterDelete(TEntity entity)
        {
            if (AfterDelete != null)
            {
                await AfterDelete(this, entity);
            }
        }

    }
}
