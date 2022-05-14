using System.IO;

namespace Aco_Domain.Graph
{
    public static class GraphReader
    {
        public static IGraph<int> ReadGraphAsAdjMatrix(string fileName)
        {
            using var sr = new StreamReader(fileName);
            
            var nrOfVertices = int.Parse(sr.ReadLine() ?? string.Empty);
            var graph = new GraphAsAdjMatrix(nrOfVertices);
            
            for (var i = 0; i < nrOfVertices; i++)
            {
                var line = sr.ReadLine() ?? string.Empty;
                var tokens = line.Split(' ');
                
                var vertex = int.Parse(tokens[0]);
                var name = tokens[1];
                
                graph.NameFor[vertex] = name;
                graph.VertexFor[name] = vertex;
            }

            for (var i = 0; i < nrOfVertices - 1; i++)
            {
                var line = sr.ReadLine() ?? string.Empty;
                var tokens = line.Split(' ');
                
                for (var j = 0; j < tokens.Length; j++)
                {
                    var weight = int.Parse(tokens[j]);
                    graph.AddEdge(i, i + j + 1, weight);
                }
            }    
            
            return graph;
        }
    }
}