using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mutants.Cache;
using Mutants.DataAccess;
using Mutants.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mutants.Tests
{
    public class RepositoryTest
    {
        private string[] mutant = { "AAAAGA", "CAGAGC", "TTAAGT", "AGAAGG", "CCCCTA", "TCACTG" };
        private string[] human = { "CTGCGA", "CAGTAC", "TTATGT", "AGAAGG", "CTACTA", "TCGCTG" };
        private string mutantKey = "AAAAGACAGAGCTTAAGTAGAAGGCCCCTATCACTG";
        private string humanKey = "CTGCGACAGTACTTATGTAGAAGGCTACTATCGCTG";
        private Processed notProcessedMutant = new Processed(false, true);
        private Processed notProcessedHuman = new Processed(false, false);
        private Processed processedMutant = new Processed(true, true);
        private Processed processedHuman = new Processed(true, false);

        private ICache<Processed> GetCache()
        {
            ICache<Processed> memory = new MemoryCache();
            return memory;
        }

        private Repository GetRepository(ICache<Processed> memory)
        {
            var logger = new Mock<ILogger<Repository>>();
            var dynamoDB = new Mock<IAmazonDynamoDB>();
            var repository = new Repository(memory, logger.Object, dynamoDB.Object);

            return repository;
        }

        private Repository GetRepositoryThatFoundDNA(ICache<Processed> memory)
        {
            var logger = new Mock<ILogger<Repository>>();
            var dynamoDB = new Mock<IAmazonDynamoDB>();

            dynamoDB.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new GetItemResponse() 
                        { 
                            IsItemSet = true, 
                            Item = new Dictionary<string, AttributeValue> {
                                { "isMutant", new AttributeValue("false") }
                            }
                        }
                );

            var repository = new Repository(memory, logger.Object, dynamoDB.Object);

            return repository;
        }

        private Repository GetRepositoryWithPerfectStats(ICache<Processed> memory)
        {
            var logger = new Mock<ILogger<Repository>>();
            var dynamoDB = new Mock<IAmazonDynamoDB>();

            AttributeValue totalHumansAttribute = new AttributeValue("100");
            totalHumansAttribute.N = "100";
            AttributeValue totalMutantsAttribute = new AttributeValue("40");
            totalMutantsAttribute.N = "40";

            dynamoDB.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new GetItemResponse()
                    {
                        IsItemSet = true,
                        Item = new Dictionary<string, AttributeValue> {
                                { "total_humans", totalHumansAttribute },
                                { "total_mutants", totalMutantsAttribute }
                            }
                    }
                );

            var repository = new Repository(memory, logger.Object, dynamoDB.Object);

            return repository;
        }

        [Fact]
        public async Task When_DnaWasNotFoundInCache_ReturnFalse()
        {
            var memory = GetCache();
            var _sut = GetRepository(memory);

            var processed = await _sut.DnaWasProcessed(mutant);

            processed.InDatabase.Should().BeFalse();
            processed.IsMutant.Should().BeFalse();
        }

        [Fact]
        public async Task When_DnaWasNotProcessed_ReturnFalse()
        {
            var memory = GetCache();
            var _sut = GetRepository(memory);

            memory.Set(mutantKey, new Processed(false, false));
            var processed = await _sut.DnaWasProcessed(mutant);

            processed.InDatabase.Should().BeFalse();
            processed.IsMutant.Should().BeFalse();
        }

        [Fact]
        public async Task When_DnaWasProcessed_ReturnIsMutantFalse()
        {
            var memory = GetCache();
            var _sut = GetRepository(memory);

            memory.Set(humanKey, new Processed(true, false));
            var processed = await _sut.DnaWasProcessed(human);

            processed.InDatabase.Should().BeTrue();
            processed.IsMutant.Should().BeFalse();
        }

        [Fact]
        public async Task When_DnaWasProcessedInDatabase_ReturnIsMutantFalse()
        {
            var memory = GetCache();
            var _sut = GetRepositoryThatFoundDNA(memory);

            var processed = await _sut.DnaWasProcessed(mutant);

            processed.InDatabase.Should().BeTrue();
            processed.IsMutant.Should().BeFalse();
        }



        [Fact]
        public async Task When_DnaWasProcessed_ReturnIsMutantTrue()
        {
            var memory = GetCache();
            var _sut = GetRepository(memory);

            memory.Set(mutantKey, new Processed(true, true));
            var processed = await _sut.DnaWasProcessed(mutant);

            processed.InDatabase.Should().BeTrue();
            processed.IsMutant.Should().BeTrue();
        }

        [Fact]
        public async Task When_SaveDnaIsCorrect_ReturnTrue()
        {
            var memory = GetCache();
            var _sut = GetRepository(memory);

            bool saved = await _sut.SaveDnaValidation(mutant, true);

            var processed = memory.Get(mutantKey);
            processed.InDatabase.Should().BeTrue();
            processed.IsMutant.Should().BeTrue();
            saved.Should().BeTrue();
        }

        [Fact]
        public async Task When_SaveDnaIsNotCorrect_ReturnFalse()
        {
            var memory = GetCache();
            var _sut = GetRepository(memory);

            bool saved = await _sut.SaveDnaValidation(null, true);

            saved.Should().BeFalse();
        }

        [Fact]
        public async Task When_HasInfoReturnCorrectStats()
        {
            var memory = GetCache();
            var _sut = GetRepositoryWithPerfectStats(memory);

            Stats stats = await _sut.GetStats();

            stats.Should().NotBeNull();
            stats.TotalHumans.Should().Be(100);
            stats.TotalMutants.Should().Be(40);
            stats.Ratio.Should().BeGreaterThan(0);
        }
    }
}
