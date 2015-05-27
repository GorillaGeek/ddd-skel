using System;
using Ninject;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

using Gorilla.DDD.Extensions;
using Gorilla.DDD.Pagination;

namespace Gorilla.DDD
{
    public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TContext : DbContext, IContext
        where TEntity : Entity
        where TKey : IComparable
    {
        protected TContext _context;

        public static event EventHandler<EntityEventArgs> OnBeforePersist;
        public static event EventHandler<EntityEventArgs> OnAfterPersist;

        public static event EventHandler<EntityEventArgs> OnBeforeSave;
        public static event EventHandler<EntityEventArgs> OnAfterSave;

        public static event EventHandler<EntityEventArgs> OnBeforeRemove;
        public static event EventHandler<EntityEventArgs> OnAfterRemove;

        [Inject]
        public void InjectDependencies(TContext context)
        {
            this._context = context;
        }

        public virtual async Task<TEntity> Find(TKey id)
        {
            return await this._context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<List<TEntity>> All()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            this.RaiseBeforePersist(entity);
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();
            this.RaiseAfterPersist(entity);

            return entity;
        }

        public virtual async Task<TEntity> Update(TEntity entity)
        {
            this.RaiseBeforeSave(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            this.RaiseAfterSave(entity);

            return entity;
        }

        public virtual async Task<bool> Remove(TKey id)
        {
            var entity = await this.Find(id);

            if (entity == null)
            {
                throw new ArgumentException("Entity not found");
            }

            this.RaiseBeforeRemove(entity);

            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();

            this.RaiseAfterRemove(entity);

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
            return await this.QueryBy<TResult>(exp, columns).ToListAsync();
        }

        public virtual async Task<List<TEntity>> SelectBy(Expression<Func<TEntity, bool>> exp)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = this.AddFixedConditions(query);

            return await query.Where(exp)
                              .ToListAsync();
        }

        public virtual async Task<List<TResult>> Select<TResult>(Expression<Func<TEntity, TResult>> columns)
        {
            return await this.Query(columns).ToListAsync();
        }

        public virtual IQueryable<TResult> QueryBy<TResult>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = this.AddFixedConditions(query);

            return query.Where(exp)
                        .Select<TEntity, TResult>(columns);
        }

        public virtual IQueryable<U> Query<U>(Expression<Func<TEntity, U>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = this.AddFixedConditions(query);
            return query.Select<TEntity, U>(columns);
        }

        public virtual async Task<PagedResult<TResult>> SelectPagedBy<TResult>(PaginationSettings settings, Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = this.AddFixedConditions(query);

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
            query = this.AddFixedConditions(query);

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
            query = this.AddFixedConditions(query);

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

        #region events

        protected virtual void RaiseBeforePersist(Entity entity)
        {
            if (OnBeforePersist != null)
            {
                OnBeforePersist(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseAfterPersist(Entity entity)
        {
            if (OnAfterPersist != null)
            {
                OnAfterPersist(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseBeforeSave(Entity entity)
        {
            if (OnBeforeSave != null)
            {
                OnBeforeSave(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseAfterSave(Entity entity)
        {
            if (OnAfterSave != null)
            {
                OnAfterSave(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseBeforeRemove(Entity entity)
        {
            if (OnBeforeRemove != null)
            {
                OnBeforeRemove(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseAfterRemove(Entity entity)
        {
            if (OnAfterRemove != null)
            {
                OnAfterRemove(this, new EntityEventArgs() { Entity = entity });
            }
        }

        #endregion events

    }
}
