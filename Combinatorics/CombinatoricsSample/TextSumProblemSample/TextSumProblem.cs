using System;
using System.Collections.Generic;
using System.Text;

using Facet.Combinatorics;

namespace TextSumProblemSample {
    /// <summary>
    /// Simple wrapper around a text sum problem, e.g. FOUR + FIVE = NINE.
    /// </summary>
    public class TextSumProblem {

        /// <summary>
        /// Event indicating that another variation has been tried on the solution, includes total number that will be tried.
        /// </summary>
        public event ProgressDelegate OnProgress;
        public delegate void ProgressDelegate(object sender, int current, int total);

        /// <summary>
        /// Event indicating that a solution to the problem has been found, includes a dictionary indicating the solution characters and values.
        /// </summary>
        public event AnswerDelegate OnSolution;
        public delegate void AnswerDelegate(object sender, Dictionary<char, int> solution);

        /// <summary>
        /// Create and solve a problem of numeric substitution, e.g. FOUR + FIVE = NINE.
        /// </summary>
        /// <remarks>
        /// First, the problem is turned into FOUR + FIVE - NINE = 0 with coeffiicents
        /// on each letter.  I.e.:
        ///    (1000F + 100O + 10U + R) + (1000F + 100I + 10V + E) - (1000N + 100I + 10N + E) = 0
        /// => 2000F + 100O + 10U + R + 0I + 10V + 0E + 1010N = 0
        /// Each variation is then generated and the simplified problem is checked for satisifiability.
        /// Events are raised as appropriate.
        /// </remarks>
        public void Solve(string op1, string op2, string sum) {
            int[] ints = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            characters.Clear();
            cofs.Clear();
            AddCofs(op1, 1);
            AddCofs(op2, 1);
            AddCofs(sum, -1);
            Variations<int> variations = new Variations<int>(ints, cofs.Count);
            int count = (int)variations.Count;
            int current = 0;
            foreach(IList<int> variation in variations) {
                if(OnProgress != null) {
                    OnProgress(this, current, count);
                }
                if(Satisfies(variation) == true) {
                    if(OnSolution != null) {
                        Dictionary<char, int> solution = new Dictionary<char, int>();
                        for(int i = 0; i < variation.Count; ++i) {
                            solution.Add(characters[i], variation[i]);
                        }
                        OnSolution(this, solution);
                    }
                }
                ++current;
            }
        }

        /// <summary>
        /// Determine if the given variation satisisfies the problem.
        /// </summary>
        private bool Satisfies(IList<int> variation) {
            int result = 0;
            for(int i = 0; i < variation.Count; ++i) {
                char c = characters[i];
                int cof = cofs[i];
                int var = variation[i];
                result += cof * var;
            }
            return result == 0;
        }

        private void AddCofs(string s, int factor) {
            for(int i = 0; i < s.Length; ++i) {
                char c = s[i];
                int digitFactor = factor * (int)Math.Pow(10, s.Length - i);
                if(characters.Contains(c) == true) {
                    cofs[characters.IndexOf(c)] += digitFactor;
                }
                else {
                    characters.Add(c);
                    cofs.Add(digitFactor);
                }
            }
        }

        private List<char> characters = new List<char>();
        private List<int> cofs = new List<int>();


    }
}
