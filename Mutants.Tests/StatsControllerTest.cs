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
    public class StatsControllerTest
    {
        [Fact]
        public async Task WhenStatsExists_ReturnsOK()
        {
            var logger = new Mock<ILogger<StatsController>>();
            var memory = new Mock<MemoryCache>();
            var logRepo = new Mock<ILogger<Repository>>();
            var dynamo = new Mock<IAmazonDynamoDB>();
            var repository = new Mock<Repository>(memory.Object, logRepo.Object, dynamo.Object);

            repository.Setup(x => x.GetStats()).ReturnsAsync(new Stats(1, 100, 40, 0.4f));

            var _sut = new StatsController(logger.Object, repository.Object);

            IActionResult actionResult = await _sut.Get();
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task WhenStatsDoesNotExists_Returns403()
        {
            var logger = new Mock<ILogger<StatsController>>();
            var memory = new Mock<MemoryCache>();
            var logRepo = new Mock<ILogger<Repository>>();
            var dynamo = new Mock<IAmazonDynamoDB>();
            var repository = new Mock<Repository>(memory.Object, logRepo.Object, dynamo.Object);

            repository.Setup(x => x.GetStats()).Throws(new Exception("Error loading database"));

            var _sut = new StatsController(logger.Object, repository.Object);

            IActionResult actionResult = await _sut.Get();
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }
    }
}
