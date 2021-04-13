using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.Cache
{
    public interface ICache<T>
    {
        T Get(string key);
        bool Set(string key, T value);
    }
}
