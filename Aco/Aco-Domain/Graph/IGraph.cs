using System.Collections.Generic;

namespace Aco_Domain.Graph 
{
    public interface IGraph<T>
    {
        int NumberOfVertices { get; }
        IEnumerable<T> this[int vertex] { get; }
        int this[int vertex1, int vertex2] { get; }
        Dictionary<string, int> VertexFor { get; }
        Dictionary<int, string> NameFor { get; }
        void AddEdge(int from, int to, int weight);
    }
}