using Mutants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.Cache
{
    public class MemoryCache : ICache<Processed>
    {
        private Dictionary<string, Processed> dictionary;

        public MemoryCache()
        {
            dictionary = new Dictionary<string, Processed>();
        }

        public Processed Get(string key)
        {
            if (dictionary.TryGetValue(key, out var processed))
                return processed;
            else
                return null;
        }

        public bool Set(string key, Processed value)
        {
            return dictionary.TryAdd(key, value);
        }
    }
}
