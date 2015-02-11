public static class HopcroftKarpMaxBipartiteMatcher
{
    public static List<Tuple<int, int>> MaximumMatching(IEnumerable<int> partition1, IEnumerable<int> partition2, List<Tuple<int, int>> edges)
    {
        var unmatchedVertices1 = new HashSet<int>(partition1);
        var unmatchedVertices2 = new HashSet<int>(partition2);
        var matching = GreedyMatch(partition1, edges, unmatchedVertices1, unmatchedVertices2);

        var augmentingPaths = GetAugmentingPaths(matching, edges, unmatchedVertices1, unmatchedVertices2);
        while (augmentingPaths.Any())
        {
            while (true)
            {
                if (!augmentingPaths.Any())
                    break;

                var augmentingPath = augmentingPaths.First();
                unmatchedVertices1.Remove(augmentingPath.First.Value);
                unmatchedVertices2.Remove(augmentingPath.Last.Value);
                ModifyBySymmetricDifference(matching, augmentingPath);
                augmentingPaths.RemoveAt(0);
            }

            augmentingPaths.AddRange(GetAugmentingPaths(matching, edges, unmatchedVertices1, unmatchedVertices2));
        }

        return matching;
    }

    private static List<Tuple<int, int>> GreedyMatch(IEnumerable<int> partition1, List<Tuple<int, int>> edges, ICollection<int> unmatchedVertices1, ICollection<int> unmatchedVertices2)
    {
        var matching = new List<Tuple<int, int>>();
        var usedVertices = new HashSet<int>();
        foreach (var vertex1 in partition1)
            foreach (var vertex2 in NeighborsOf(vertex1, edges))
            {
                if (!usedVertices.Contains(vertex2))
                {
                    usedVertices.Add(vertex2);
                    unmatchedVertices1.Remove(vertex1);
                    unmatchedVertices2.Remove(vertex2);
                    matching.Add(Tuple.Create(vertex1, vertex2));
                    break;
                }
            }

        return matching;
    }

    private static LinkedList<int> Dfs(int startVertex, ICollection<int> unmatchedVertices, IReadOnlyDictionary<int, HashSet<int>> layeredMap)
    {
        if (!layeredMap.ContainsKey(startVertex))
            return null;

        if (unmatchedVertices.Contains(startVertex))
        {
            var list = new LinkedList<int>();
            list.AddFirst(startVertex);
            return list;
        }

        LinkedList<int> partialPath = null;
        foreach (var vertex in layeredMap[startVertex])
        {
            partialPath = Dfs(vertex, unmatchedVertices, layeredMap);
            if (partialPath != null)
            {
                partialPath.AddLast(startVertex);
                break;
            }
        }

        return partialPath;
    }

    private static List<LinkedList<int>> GetAugmentingPaths(List<Tuple<int, int>> matching, List<Tuple<int, int>> edges, ICollection<int> unmatchedVertices1, ICollection<int> unmatchedVertices2)
    {
        var augmentingPaths = new List<LinkedList<int>>();
        var layeredMap = new Dictionary<int, HashSet<int>>();
        foreach (var vertex in unmatchedVertices1)
            layeredMap[vertex] = new HashSet<int>();

        var oddLayer = new HashSet<int>(unmatchedVertices1);
        HashSet<int> evenLayer;
        var usedVertices = new HashSet<int>(unmatchedVertices1);

        while (true)
        {
            evenLayer = new HashSet<int>();
            foreach (var vertex in oddLayer)
            {
                var neighbors = NeighborsOf(vertex, edges);
                foreach (var neighbor in neighbors)
                {
                    if (usedVertices.Contains(neighbor))
                        continue;

                    evenLayer.Add(neighbor);
                    if (!layeredMap.ContainsKey(neighbor))
                        layeredMap[neighbor] = new HashSet<int>();
                    
                    layeredMap[neighbor].Add(vertex);
                }
            }
            usedVertices.AddRange(evenLayer);
            if (!evenLayer.Any() || evenLayer.Intersect(unmatchedVertices2).Any())
                break;

            oddLayer = new HashSet<int>();
            foreach (var vertex in evenLayer)
            {
                var neighbors = NeighborsOf(vertex, edges);
                foreach (var neighbor in neighbors)
                {
                    if (usedVertices.Contains(neighbor) || !matching.ContainsUndirectedEdge(vertex, neighbor))
                        continue;

                    oddLayer.Add(neighbor);
                    if (!layeredMap.ContainsKey(neighbor))
                        layeredMap[neighbor] = new HashSet<int>();
                    
                    layeredMap[neighbor].Add(vertex);
                }
            }
            usedVertices.AddRange(oddLayer);
        }

        if (!evenLayer.Any())
            return augmentingPaths;

        evenLayer.IntersectWith(unmatchedVertices2);
        foreach (var vertex in evenLayer)
        {
            var augmentingPath = Dfs(vertex, unmatchedVertices1, layeredMap);
            if (augmentingPath != null)
            {
                augmentingPaths.Add(augmentingPath);
                foreach (var augmentingVertex in augmentingPath)
                    layeredMap.Remove(augmentingVertex);
            }
        }

        return augmentingPaths;
    }

    private static void ModifyBySymmetricDifference(List<Tuple<int, int>> matching, LinkedList<int> augmentingPath)
    {
        int operation = 0;
        while (augmentingPath.Count >= 2)
        {
            var first = augmentingPath.First.Value;
            augmentingPath.RemoveFirst();
            var next = augmentingPath.First.Value;

            var edge = Tuple.Create(first, next);
            if ((operation % 2) == 0)
                matching.Add(edge);
            else
                matching.RemoveUndirectedEdge(first, next);

            operation++;
        }
    }
    
    private static bool ContainsUndirectedEdge(this IEnumerable<Tuple<int, int>> edgeSet, int x, int y)
    {
        return edgeSet.Any(e => (e.Item1 == x && e.Item2 == y) || (e.Item1 == y && e.Item2 == x));
    }

    private static void RemoveUndirectedEdge(this List<Tuple<int, int>> edgeSet, int x, int y)
    {
        edgeSet.RemoveAll(e => (e.Item1 == x && e.Item2 == y) || (e.Item1 == y && e.Item2 == x));
    }

    private static List<int> NeighborsOf(int vertex, IEnumerable<Tuple<int, int>> edges)
    {
        return edges.Where(e => e.Item1 == vertex).Select(e => e.Item2)
            .Union(edges.Where(e => e.Item2 == vertex).Select(e => e.Item1))
            .Distinct().ToList();
    }
}