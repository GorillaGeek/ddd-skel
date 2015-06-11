
using Gorilla.DDD.Pagination;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gorilla.DDD
{
    public delegate Task BeforeSaveEventHandler(object sender, EntityEventArgs e);
    public delegate Task BeforePersistEventHandler(object sender, EntityEventArgs e);
    public delegate Task BeforeDeleteEventHandler(object sender, EntityEventArgs e);

    public delegate Task AfterSaveEventHandler(object sender, EntityEventArgs e);
    public delegate Task AfterPersistEventHandler(object sender, EntityEventArgs e);
    public delegate Task AfterDeleteEventHandler(object sender, EntityEventArgs e);

    public interface IRepository<TEntity, TKey>
        where TEntity : Entity
        where TKey : IComparable
    {
        event BeforeSaveEventHandler BeforeSave;
        event BeforePersistEventHandler BeforePersist;
        event BeforeDeleteEventHandler BeforeDelete;

        event AfterSaveEventHandler AfterSave;
        event AfterPersistEventHandler AfterPersist;
        event AfterDeleteEventHandler AfterDelete;

        DbContextTransaction BeginTransaction();

        Task<TEntity> Find(TKey id);

        Task<List<TEntity>> All();

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

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp);

        IQueryable<U> Query<U>(Expression<Func<TEntity, U>> columns);

        IQueryable<TResult> QueryBy<TResult>(Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns);

    }
}
