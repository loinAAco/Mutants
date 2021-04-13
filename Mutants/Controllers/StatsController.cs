using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mutants.Business;
using Mutants.DataAccess;
using Mutants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly ILogger<StatsController> _logger;
        private readonly IRepository repository;

        public StatsController(ILogger<StatsController> logger, IRepository repository)
        {
            _logger = logger;
            this.repository = repository;
        }

        [HttpGet(Name ="stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Get()
        {
            try
            {
                var stats = await repository.GetStats();
                return Ok(new { 
                    count_mutant_dna = stats.TotalMutants, 
                    count_human_dna = stats.TotalHumans, 
                    ratio = stats.Ratio 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return this.StatusCode(StatusCodes.Status403Forbidden);
            } 
        }
    }
}
