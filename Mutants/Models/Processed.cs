using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.Models
{
    public class Processed
    {

        public Processed(bool inDatabase, bool isMutant)
        {
            InDatabase = inDatabase;
            IsMutant = isMutant;
        }

        public bool InDatabase { get; set; }
        public bool IsMutant { get; set; }
    }
}
