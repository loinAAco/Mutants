using FluentAssertions;
using Mutants.Business;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Mutants.Tests
{
    public class MutantTest
    {
        #region Data for testing
        private string[] mutant = { "AAAAGA", "CAGAGC", "TTAAGT", "AGAAGG", "CCCCTA", "TCACTG" };
        private string[] human = { "CTGCGA", "CAGTAC", "TTATGT", "AGAAGG", "CTACTA", "TCGCTG" };
        private string[] emptyDNA = { };
        private string[] invalidDNA = { "CTGC", "CAGAC", "TTATGT", "AGG", "CCTA", "TCTG" };
        private string[] invalidDNALength = { "CTGC", "CAGA", "TTAT", "AGGC", "CCTA" };
        private string[] invalidDNALetters = { "CTXCGA", "CAGTAC", "TTATGT", "AGAAGG", "CTACTA", "TCGCTG" };
        private char[] allowedLetters = { 'A', 'T', 'C', 'G' };
        private char[] invalidLetters = { 'A', 't', 'C', 'G' };
        private char[] emptyAllowedLetters = { };
        #endregion

        #region Validation Tests
        [Fact]
        public void Empty_ADN_ShouldRaiseEx()
        {
            Mutant m = new Mutant(allowedLetters);
            Action a = () => {
                m.IsMutant(emptyDNA);
            };

            a.Should()
             .Throw<Exception>()
             .WithMessage("DNA not received!");
        }

        [Fact]
        public void Invalid_ADNChainSizes_ShouldRaiseEx()
        {
            Mutant m = new Mutant(allowedLetters);
            Action a = () => { m.IsMutant(invalidDNA); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("Every parameter in DNA sequence must have the same length: 4");
        }

        [Fact]
        public void Invalid_ADNLength_ShouldRaiseEx()
        {
            Mutant m = new Mutant(allowedLetters);
            Action a = () => { m.IsMutant(invalidDNALength); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("DNA parameter must have the form NxN, received 4x5");
        }

        [Fact]
        public void Invalid_AllowedLetters_ShouldRaiseEx()
        {
            Action a = () => { Mutant m = new Mutant(emptyAllowedLetters); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("We need something to validate the DNA");
        }

        [Fact]
        public void Lowercase_AllowedLetters_ShouldRaiseEx()
        {
            Action a = () => { Mutant m = new Mutant(invalidLetters); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("Allowed letters should be in uppercase");
        }

        [Fact]
        public void When_InvalidSizeOfSequence_ShouldRaiseEx()
        {
            Action a = () => { Mutant m = new Mutant(allowedLetters, 1); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("Invalid size for sequence to find");
        }

        [Fact]
        public void When_NumberOfOcurrencesInvalid_ShouldRaiseEx()
        {
            Action a = () => { Mutant m = new Mutant(allowedLetters, 4, 1); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("Invalid number of ocurrencies to find");
        }

        [Fact]
        public void When_DNAHasInvalidLetters_ShouldRaiseEx()
        {
            Mutant m = new Mutant(allowedLetters);
            Action a = () => { m.IsMutant(invalidDNALetters); };

            a.Should()
             .Throw<Exception>()
             .WithMessage("DNA has invalid letters");
        }
        #endregion

        #region Process Tests
        [Fact]
        public void When_humanDNA_ShouldReturnFalse()
        {
            Mutant m = new Mutant(allowedLetters, 4, 2);
            var expected = m.IsMutant(human);
            expected.Should().BeFalse();
        }

        [Fact]
        public void When_mutantDNA_ShouldReturnTrue()
        {
            Mutant m = new Mutant(allowedLetters, 4, 2);
            var expected = m.IsMutant(mutant);
            expected.Should().BeTrue();
        }
        #endregion
    }
}
