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
    public class MutantController : ControllerBase
    {
        private readonly ILogger<MutantController> _logger;
        private readonly IRepository repository;
        private readonly IMutant mutant;

        public MutantController(ILogger<MutantController> logger, IRepository repository, IMutant mutant)
        {
            _logger = logger;
            this.repository = repository;
            this.mutant = mutant;
        }

        [HttpPost(Name ="mutant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public virtual async Task<IActionResult> Post([FromBody]DnaParameter param)
        {
            bool isMutant;

            try
            {
                var processedRecord = await repository.DnaWasProcessed(param.Dna);
                if (processedRecord.InDatabase)
                    isMutant = processedRecord.IsMutant;
                else
                {
                    isMutant = this.mutant.IsMutant(param.Dna);
                    await repository.SaveDnaValidation(param.Dna, isMutant);
                }
            }
            catch (Exception ex)
            {
                isMutant = false;
                _logger.LogError(ex.Message);
            } 

            return isMutant ? Ok() : this.StatusCode(StatusCodes.Status403Forbidden);
        }
    }
}
