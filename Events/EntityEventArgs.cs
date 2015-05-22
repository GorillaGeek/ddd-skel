using System;

namespace Gorilla.DDD
{
    public class EntityEventArgs : EventArgs
    {
        public Entity Entity { get; set; }
    }
}
