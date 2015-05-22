﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorilla.DDD
{
    public interface IContext
    {
        void EnableAutoDetectChanges();

        void DisableAutoDetectChanges();
    }
}
