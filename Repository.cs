using Gorilla.DDD.Extensions;
using Gorilla.DDD.Pagination;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gorilla.DDD
{

    public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TContext : DbContext, IContext
        where TEntity : Entity
        where TKey : IComparable
    {
        protected TContext _context;

#if DEBUG
        private DateTime _createDateTime = DateTime.Now;

        public DateTime CreatedDateTime => _createDateTime;
#endif

        [Inject]
        public void InjectDependencies(TContext context)
        {
            _context = context;
        }

        public virtual async Task<TEntity> Find(TKey id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<List<TEntity>> All()
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<List<TEntity>> AllWithInclude(params Expression<Func<TEntity, object>>[] includeSelector)
        {
            return await includeSelector.Aggregate(_context.Set<TEntity>().AsQueryable(), IncludeInternal)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public virtual async Task<TEntity> Update(TEntity entity)
        {
            try
            {
                _context.EnableAutoDetectChanges();
                await _context.SaveChangesAsync();
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
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();

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

        public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> exp = null)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);

            if (exp != null)
            {
                query = query.Where(exp);
            }

            return query;
        }

        public virtual IQueryable<U> Query<U>(Expression<Func<TEntity, U>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);
            return query.Select<TEntity, U>(columns);
        }

        public virtual async Task<PagedResult<TEntity>> SelectPagedBy(PaginationSettings settings, Expression<Func<TEntity, bool>> exp)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);

            if (exp != null)
            {
                query = query.Where(exp);
            }


            var total = await query.CountAsync();
            var data = await query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString())
                         .Skip(settings.Skip)
                         .Take(settings.Take)
                         .ToListAsync();

            return new PagedResult<TEntity>(settings.Page, total, settings.Take, data);
        }

        public virtual async Task<PagedResult<TResult>> SelectPagedBy<TResult>(PaginationSettings settings, Expression<Func<TEntity, bool>> exp, Expression<Func<TEntity, TResult>> columns)
        {
            var query = _context.Set<TEntity>().AsQueryable();
            query = AddFixedConditions(query);

            if (exp != null)
            {
                query = query.Where(exp);
            }

            var total = await query.CountAsync();
            var data = await query.OrderBy<TEntity>(settings.OrderColumn, settings.OrderDirection.ToString())
                            .Select<TEntity, TResult>(columns)
                            .Skip(settings.Skip)
                            .Take(settings.Take)
                            .ToListAsync();

            return new PagedResult<TResult>(settings.Page, total, settings.Take, data);
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

            return new PagedResult<TResult>(settings.Page, total, settings.Take, data);
        }

        protected virtual IQueryable<TEntity> AddFixedConditions(IQueryable<TEntity> query)
        {
            return query;
        }

        protected virtual IQueryable<TEntity> FindByIdQuery(IQueryable<TEntity> query, TKey id)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<TEntity> FindWithInclude(TKey id, params Expression<Func<TEntity, object>>[] includeSelector)
        {
            var query = FindByIdQuery(_context.Set<TEntity>(), id);

            query = includeSelector.Aggregate(query, IncludeInternal);

            return await query.FirstOrDefaultAsync();
        }

        protected virtual IQueryable<TEntity> FindWithInclude(IQueryable<TEntity> query, params Expression<Func<TEntity, object>>[] includeSelector)
        {
            return includeSelector.Aggregate(query, IncludeInternal);
        }

        private IQueryable<T> IncludeInternal<T>(IQueryable<T> query, Expression<Func<TEntity, object>> selector)
        {
            string propertyName = FuncToString(selector);
            return query.Include(propertyName);
        }

        private string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression memberExpr = expression.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException("Expression body must be a member expression");
            return memberExpr.Member.Name;
        }

        private static string FuncToString(Expression selector)
        {
            switch (selector.NodeType)
            {
                case ExpressionType.Lambda:

                    var lambdaBody = ((LambdaExpression)selector).Body;

                    return lambdaBody.NodeType == ExpressionType.Call
                        ? FuncToString(lambdaBody)              // That`s meant for expressions like "x=> x.Terms.Select(q=> q.Logo) > Terms.Logo"
                        : ExtractFuncFromString(lambdaBody);    // That`s meant for expressions like "x=> x.Template.Logo"

                case ExpressionType.MemberAccess:
                    return ((selector as MemberExpression).Member as System.Reflection.PropertyInfo).Name;
                case ExpressionType.Call:
                    var method = selector as MethodCallExpression;
                    return FuncToString(method.Arguments[0]) + "." + FuncToString(method.Arguments[1]);
                case ExpressionType.Quote:
                    return FuncToString(((selector as UnaryExpression).Operand as LambdaExpression).Body);
            }
            throw new InvalidOperationException();
        }

        private static string ExtractFuncFromString(Expression lambdaBody)
        {
            var match = new Regex(@"[^.]\.(?<include>\S+)").Match(lambdaBody.ToString());
            return match.Groups["include"].Value.TrimEnd();
        }
    }
}
