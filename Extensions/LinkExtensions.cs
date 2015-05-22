using System.Linq;
using System.Linq.Expressions;

namespace Gorilla.DDD.Extensions
{
    public static class LinkExtensions
    {

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, string direction)
        {
            string methodName = string.Format("OrderBy{0}",
                direction == "Descending" ? "Descending" : "");

            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = null;
            memberAccess = MemberExpression.Property
                   (memberAccess ?? (parameter as Expression), sortColumn);

            LambdaExpression orderByLambda = Expression.Lambda(memberAccess, parameter);

            MethodCallExpression result = Expression.Call(
                      typeof(Queryable),
                      methodName,
                      new[] { query.ElementType, memberAccess.Type },
                      query.Expression,
                      Expression.Quote(orderByLambda));

            return query.Provider.CreateQuery<T>(result);
        }


    }
}
