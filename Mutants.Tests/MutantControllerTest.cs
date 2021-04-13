using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Mutants.Business;
using Mutants.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Mutants.Models;
using Mutants.Cache;
using Mutants.DataAccess;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;

namespace Mutants.Tests
{
    public class MutantControllerTest
    {
        private string[] mutant = { "AAAAGA", "CAGAGC", "TTAAGT", "AGAAGG", "CCCCTA", "TCACTG" };
        private string[] human = { "CTGCGA", "CAGTAC", "TTATGT", "AGAAGG", "CTACTA", "TCGCTG" };
        private string[] emptyDNA = { };
        private string[] invalidDNA = { "CTGC", "CAGAC", "TTATGT", "AGG", "CCTA", "TCTG" };
        private string[] invalidDNALength = { "CTGC", "CAGA", "TTAT", "AGGC", "CCTA" };
        private string[] invalidDNALetters = { "CTXCGA", "CAGTAC", "TTATGT", "AGAAGG", "CTACTA", "TCGCTG" };

        [Fact]
        public async Task WhenIsNotAMutant_ReturnsForbidden()
        {
            var logger = new Mock<ILogger<MutantController>>();
            var mutant = new Mock<IMutant>();
            var memory = new Mock<MemoryCache>();
            var logRepo = new Mock<ILogger<Repository>>();
            var dynamo = new Mock<IAmazonDynamoDB>();
            var repository = new Mock<Repository>(memory.Object, logRepo.Object, dynamo.Object);

            repository.Setup(x => x.DnaWasProcessed(It.IsAny<String[]>())).ReturnsAsync(new Processed(false, false));

            var _sut = new MutantController(logger.Object, repository.Object, mutant.Object);

            IActionResult actionResult = await _sut.Post(new DnaParameter() { Dna = this.emptyDNA });
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task WhenDnaWasProcessedThrowsAndException_IsMutantIsFalse()
        {
            var logger = new Mock<ILogger<MutantController>>();
            var mutant = new Mock<IMutant>();
            var memory = new Mock<MemoryCache>();
            var logRepo = new Mock<ILogger<Repository>>();
            var dynamo = new Mock<IAmazonDynamoDB>();
            var repository = new Mock<Repository>(memory.Object, logRepo.Object, dynamo.Object);

            repository.Setup(x => x.DnaWasProcessed(It.IsAny<String[]>())).Throws(new Exception("Database error"));

            var _sut = new MutantController(logger.Object, repository.Object, mutant.Object);

            IActionResult actionResult = await _sut.Post(new DnaParameter() { Dna = this.mutant });
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }


        [Theory]
        [InlineData(false, new string[] { "CTGC", "CAGAC", "TTATGT", "AGG", "CCTA", "TCTG" })]
        [InlineData(false, new string[] { "CTGC", "CAGA", "TTAT", "AGGC", "CCTA" })]
        [InlineData(false, new string[] { "CTXCGA", "CAGTAC", "TTATGT", "AGAAGG", "CTACTA", "TCGCTG" })]
        public async Task WhenIsNotAValidDNA_ReturnsForbidden(bool isValid, string[] dna)
        {
            var logger = new Mock<ILogger<MutantController>>();
            var mutant = new Mock<IMutant>();
            var memory = new Mock<MemoryCache>();
            var logRepo = new Mock<ILogger<Repository>>();
            var dynamo = new Mock<IAmazonDynamoDB>();
            var repository = new Mock<Repository>(memory.Object, logRepo.Object, dynamo.Object);

            repository.Setup(x => x.DnaWasProcessed(It.IsAny<String[]>())).ReturnsAsync(new Processed(false, false));

            var _sut = new MutantController(logger.Object, repository.Object, mutant.Object);

            IActionResult actionResult = await _sut.Post(new DnaParameter() { Dna = dna });
            var statusCodeResult = (IStatusCodeActionResult)actionResult;

            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ValidateInRepoBeforeProcessing(bool InDatabase, bool IsMutant)
        {
            var logger = new Mock<ILogger<MutantController>>();
            var mutant = new Mock<IMutant>();
            var memory = new Mock<MemoryCache>();
            var logRepo = new Mock<ILogger<Repository>>();
            var dynamo = new Mock<IAmazonDynamoDB>();
            var repository = new Mock<Repository>(memory.Object, logRepo.Object, dynamo.Object);

            repository.Setup(x => x.DnaWasProcessed(It.IsAny<String[]>())).ReturnsAsync(new Processed(InDatabase, IsMutant));

            var _sut = new MutantController(logger.Object, repository.Object, mutant.Object);

            IActionResult actionResult = await _sut.Post(new DnaParameter() { Dna = this.emptyDNA });
            var statusCodeResult = (IStatusCodeActionResult)actionResult;

            if (IsMutant)
                statusCodeResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            else
                statusCodeResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }
    }
}
