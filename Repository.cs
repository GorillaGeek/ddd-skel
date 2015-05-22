using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Gorilla.DDD.Pagination;
using System.Data.Entity;
using Gorilla.DDD.Extensions;

namespace Gorilla.DDD
{
    public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TContext : DbContext, IContext
        where TEntity: Entity
        where TKey : IComparable
    {
        protected TContext _context;

        public static event EventHandler<EntityEventArgs> EntityAdded;
        public static event EventHandler<EntityEventArgs> EntityUpdated;
        public static event EventHandler<EntityEventArgs> EntityRemoved;
        
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
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();
            this.RaiseEntityAdded(entity);

            return entity;
        }

        public virtual async Task<TEntity> Update(TEntity entity)
        {

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            this.RaiseEntityUpdated(entity);

            return entity;
        }

        public virtual async Task<bool> Remove(TKey id)
        {
            var entity = await this.Find(id);

            if (entity == null)
            {
                throw new ArgumentException("Entity not found");
            }

            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();

            this.RaiseEntityRemoved(entity);

            return true;
        }

        public virtual async Task<List<U>> SelectBy<U>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, U>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = this.AddFixedConditions(query);

            return await query.Where(exp)
                              .Select<TEntity, U>(columns)
                              .ToListAsync();
        }

        public virtual async Task<List<TEntity>> SelectBy(Expression<Func<TEntity, bool>> exp)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = this.AddFixedConditions(query);

            return await query.Where(exp)
                              .ToListAsync();
        }

        public virtual async Task<List<U>> Select<U>(Expression<Func<TEntity, U>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            query = this.AddFixedConditions(query);

            return await query.Select<TEntity, U>(columns)
                              .ToListAsync();
        }

        public virtual async Task<PagedResult<U>> SelectPagedBy<U>(PaginationSettings settings, Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, U>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = this.AddFixedConditions(query);

            var total = await query.CountAsync();

            query = query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString());

            var data = await query.Where(exp)
                         .Select<TEntity, U>(columns)
                         .Skip(settings.Skip)
                         .Take(settings.Take)
                         .ToListAsync();

            return new PagedResult<U>(settings.Page, total, settings.Page, data);
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

        public virtual async Task<PagedResult<U>> SelectPaged<U>(PaginationSettings settings, Expression<Func<TEntity, U>> columns) 
        {

            var query = _context.Set<TEntity>().AsQueryable();
            query = this.AddFixedConditions(query);

            var total = await query.CountAsync();

            query = query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString());

            var data = await query.Select<TEntity, U>(columns)
                         .Skip(settings.Skip)
                         .Take(settings.Take)
                         .ToListAsync();
            
            return new PagedResult<U>(settings.Page, total, settings.Page, data);
        }

        protected virtual IQueryable<TEntity> AddFixedConditions(IQueryable<TEntity> query)
        {
            return query;
        }

        protected virtual void RaiseEntityAdded(Entity entity)
        {
            if (EntityAdded != null)
            {
                EntityAdded(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseEntityUpdated(Entity entity)
        {
            if (EntityUpdated != null)
            {
                EntityUpdated(this, new EntityEventArgs() { Entity = entity });
            }
        }

        protected virtual void RaiseEntityRemoved(Entity entity)
        {
            if (EntityRemoved != null)
            {
                EntityRemoved(this, new EntityEventArgs() { Entity = entity });
            }
        }

    }
}
