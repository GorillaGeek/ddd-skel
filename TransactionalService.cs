using System.Transactions;

namespace Gorilla.DDD
{
    public abstract class TransactionalService
    {

        public TransactionScope BeginTransaction()
        {
            return new TransactionScope();
        }

    }
}
