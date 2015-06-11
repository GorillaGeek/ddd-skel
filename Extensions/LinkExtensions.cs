using System.Linq;
using System.Linq.Expressions;

namespace Gorilla.DDD.Extensions
{
    public static class LinkExtensions
    {

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, string direction)
        {
            var methodName = string.Format("OrderBy{0}",
                direction == "Descending" ? "Descending" : "");
            var parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = null;
            foreach (var property in sortColumn.Split('.'))
            {
                memberAccess = Expression.Property(memberAccess ?? ((Expression) parameter), property);
            }

            var orderByLambda = Expression.Lambda(memberAccess, parameter);
            var result = Expression.Call(
                      typeof(Queryable),
                      methodName,
                      new[] { query.ElementType, memberAccess.Type },
                      query.Expression,
                      Expression.Quote(orderByLambda));

            return query.Provider.CreateQuery<T>(result);
        }


    }
}
