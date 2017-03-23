
using Gorilla.DDD.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gorilla.DDD.Interfaces;

namespace Gorilla.DDD
{

    public interface IRepository<TEntity, TKey>
        where TEntity : Entity
        where TKey : IComparable
    {
        //DbContextTransaction BeginTransaction();

        void Attach(object entity);

        Task<TEntity> Find(TKey id);

        Task<List<TEntity>> All();        

        Task<List<TEntity>> AllWithInclude(params Expression<Func<TEntity, object>>[] includeSelector);

         Task<TEntity> Add(TEntity entity);

        Task<TEntity> Update(TEntity entity);

        Task<bool> Remove(TKey id);

        Task<bool> Remove(TEntity entity);

        Task<List<U>> SelectBy<U>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, U>> columns);

        Task<List<TEntity>> SelectBy(Expression<Func<TEntity, bool>> exp);

        Task<List<U>> Select<U>(Expression<Func<TEntity, U>> columns);

        Task<PagedResult<U>> SelectPagedBy<U>(PaginationSettings settings, Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, U>> columns);

        Task<PagedResult<TEntity>> SelectPagedBy(PaginationSettings settings, Expression<Func<TEntity, bool>> exp);

        Task<PagedResult<U>> SelectPaged<U>(PaginationSettings settings, Expression<Func<TEntity, U>> columns);

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp = null);

        IQueryable<U> Query<U>(Expression<Func<TEntity, U>> columns);

        IQueryable<TResult> QueryBy<TResult>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns);

        Task<TEntity> FindWithInclude(TKey id, params Expression<Func<TEntity, object>>[] includeSelector);
    }
}
