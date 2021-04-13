using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mutants.Business
{
    public class Mutant : IMutant
    {
        private string[] dna;
        private readonly char[] allowedLetters;
        private readonly int sizeOfSequence = 4;
        private readonly int minimunOcurrences = 2;
        private int N;

        public Mutant(char[] allowedLetters, int sizeOfSequence = 4, int minimunOcurrences = 2)
        {
            this.allowedLetters = allowedLetters;
            this.sizeOfSequence = sizeOfSequence;
            this.minimunOcurrences = minimunOcurrences;
            validateConstructorParameters();
        }

        private void validateConstructorParameters()
        {
            if (allowedLetters.Length < 1)
            {
                throw new Exception("We need something to validate the DNA");
            }

            if (sizeOfSequence <= 1)
            {
                throw new Exception("Invalid size for sequence to find");
            }

            if (minimunOcurrences < 2)
            {
                throw new Exception("Invalid number of ocurrencies to find");
            }

            if (allowedLetters.Any(c => !Char.IsUpper(c)))
            {
                throw new Exception("Allowed letters should be in uppercase");
            }
        }

        private void validateDnaParameters()
        {
            if (dna.Length == 0)
                throw new Exception("DNA not received!");

            N = dna[0].Length;

            if (dna.Count(d => d.Length != N) > 0)
            {
                throw new Exception($"Every parameter in DNA sequence must have the same length: {N}");
            }

            if (dna.Length != N)
            {
                throw new Exception($"DNA parameter must have the form NxN, received {N}x{dna.Length}");
            }

            if (dna.SelectMany(x => x).Distinct().Any(c => !allowedLetters.Contains(c)))
            {
                throw new Exception("DNA has invalid letters");
            }
        }

        private int processDna()
        {
            var found = 0;
            var chainList = new List<string>();
            Char[] chain = new Char[N];

            #region Strings to look into dna data
            var sequencesToFind = new List<String>();
            for (int i = 0; i < allowedLetters.Length; i++)
            {
                sequencesToFind.Add(new String(allowedLetters[i], this.sizeOfSequence));
            }
            #endregion

            #region Searching horizontally
            for (int i = 0; i < sequencesToFind.Count; i++)
            {
                found += dna.Count(row => row.IndexOf(sequencesToFind[i]) != -1);
                if (found >= this.minimunOcurrences)
                    break;
            }

            if (found >= this.minimunOcurrences)
                return found;
            #endregion

            #region Searching vertically
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    chain[j] = dna[j][i];
                }

                chainList.Clear();
                chainList.Add(new string(chain, 0, N));

                for (int x = 0; x < sequencesToFind.Count; x++)
                {
                    found += chainList.Count(row => row.IndexOf(sequencesToFind[x]) != -1);
                    if (found >= this.minimunOcurrences)
                        break;
                }

            }

            if (found >= this.minimunOcurrences)
                return found;
            #endregion

            #region Searching in diagonals
            chainList.Clear();
            #region Top-left -> Right-Bottom
            for (int i = 0; i <= N - this.sizeOfSequence; i++)
            {
                int j = 0;
                while (j < N - i)
                {
                    chain[j] = dna[i + j][j];
                    j++;
                }

                // Fill with spaces
                while (j < N)
                {
                    chain[j] = ' ';
                    j++;
                }

                chainList.Add(new string(chain, 0, N));
            }

            for (int i = 1; i <= N - this.sizeOfSequence; i++)
            {
                int j = 0;
                while (j < N - i)
                {
                    chain[j] = dna[j][i + j];
                    j++;
                }

                // Fill with spaces
                while (j < N)
                {
                    chain[j] = ' ';
                    j++;
                }

                chainList.Add(new string(chain, 0, N));
            }

            for (int x = 0; x < sequencesToFind.Count; x++)
            {
                found += chainList.Count(row => row.IndexOf(sequencesToFind[x]) != -1);
                if (found >= this.minimunOcurrences)
                    break;
            }

            if (found >= this.minimunOcurrences)
                return found;
            #endregion

            chainList.Clear();
            #region Top-Right -> Bottom-Left

            for (int i = this.sizeOfSequence - 1; i < N; i++)
            {
                int j = 0;
                while (j <= i)
                {
                    chain[j] = dna[j][i - j];
                    j++;
                }

                // Fill with spaces
                while (j < N)
                {
                    chain[j] = ' ';
                    j++;
                }

                chainList.Add(new string(chain, 0, N));
            }

            for (int i = 1; i <= N - this.sizeOfSequence; i++)
            {
                int j = 0;
                while (j < N - i)
                {
                    chain[j] = dna[i + j][N - 1 - j];
                    j++;
                }

                // Fill with spaces
                while (j < N)
                {
                    chain[j] = ' ';
                    j++;
                }

                chainList.Add(new string(chain, 0, N));
            }

            for (int x = 0; x < sequencesToFind.Count; x++)
            {
                found += chainList.Count(row => row.IndexOf(sequencesToFind[x]) != -1);
                if (found >= this.minimunOcurrences)
                    break;
            }

            if (found >= this.minimunOcurrences)
                return found;
            #endregion

            if (found >= this.minimunOcurrences)
                return found;
            #endregion

            return found;
        }

        public bool IsMutant(string[] dna)
        {
            this.dna = dna;
            validateDnaParameters();

            int ocurrencesFound = processDna();
            return ocurrencesFound >= minimunOcurrences;
        }
    }
}
