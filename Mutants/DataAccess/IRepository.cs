using Mutants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.DataAccess
{
    public interface IRepository
    {
        Task<Processed> DnaWasProcessed(String[] dna);
        Task<bool> SaveDnaValidation(String[] dna, bool isMutant);
        Task<Stats> GetStats();
    }
}
