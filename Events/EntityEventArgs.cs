using System;
namespace Gorilla.DDD
{
    public class EntityEventArgs : EventArgs
    {
        public Entity Entity { get; set; }

        public static EntityEventArgs Create(Entity e)
        {
            return new EntityEventArgs
            {
                Entity = e
            };
        }
    }
}
