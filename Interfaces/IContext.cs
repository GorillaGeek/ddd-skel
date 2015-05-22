namespace Gorilla.DDD
{
    public interface IContext
    {
        void EnableAutoDetectChanges();

        void DisableAutoDetectChanges();
    }
}
