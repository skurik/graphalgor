using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    class MatchingTests
    {
        [Test]
        public void Finds_Maximum_Cardinality_Matching_In_Unweighted_Bipartite_Graph()
        {
            var matching = HopcroftKarpMaxBipartiteMatcher.MaximumMatching(
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 },
                new List<Tuple<int, int>>
                {
                    Tuple.Create(1, 5), Tuple.Create(1, 6),
                    Tuple.Create(2, 4), Tuple.Create(2, 6),
                    Tuple.Create(3, 4), Tuple.Create(3, 5)
                });
        
            Assert.That(matching, Is.EquivalentTo(new[] { Tuple.Create(1, 5), Tuple.Create(2, 6), Tuple.Create(3, 4) }));
        }
    }
}