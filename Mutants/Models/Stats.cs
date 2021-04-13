using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.Models
{
    public class Stats
    {
        public Stats(int id, int totalHumans, int totalMutants, float ratio)
        {
            Id = id;
            TotalHumans = totalHumans;
            TotalMutants = totalMutants;
            Ratio = ratio;
        }

        public int Id { get; set; }
        public int TotalMutants { get; set; }
        public int TotalHumans { get; set; }
        public float Ratio { get; set; }
    }
}
