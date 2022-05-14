using System.Collections.Generic;

namespace Aco_Domain.Graph
{
    public class GraphAsAdjMatrix: IGraph<int>
    {
        private readonly int[][] _adjMatrix;
        
        public int NumberOfVertices { get; }
        public Dictionary<string, int> VertexFor { get; } = new();
        public Dictionary<int, string> NameFor { get; } = new();

        public IEnumerable<int> this[int vertex] => _adjMatrix[vertex];

        public int this[int vertex1, int vertex2] => _adjMatrix[vertex1][vertex2];

        public GraphAsAdjMatrix(int numberOfVertices = 0)
        {
            NumberOfVertices = numberOfVertices;
            _adjMatrix = new int[numberOfVertices][];
            for (var i = 0; i < numberOfVertices; i++)
            {
                _adjMatrix[i] = new int[numberOfVertices];
                for (var j = 0; j < numberOfVertices; j++)
                {
                    _adjMatrix[i][j] = 0;
                }
            }
        }

        public void AddEdge(int from, int to, int weight)
        {
            _adjMatrix[from][to] = _adjMatrix[to][from] = weight;
        }
    }
}