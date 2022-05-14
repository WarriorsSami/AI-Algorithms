using Aco_Domain.Graph;

namespace Aco_Domain.AntColony
{
    public class Ant
    {
        private int[] _tabuList;
        
        public int this[int index] => _tabuList[index];
        
        public int Length => _tabuList.Length;
        
        public int Id { get; }

        public Ant(int size, int id)
        {
            Id = id;
            var start = AcoUtils.Random.Next(0, size);
            _tabuList = AcoUtils.GetRandomTrail(start, size);
        }

        public void Update(Pheromones pheromones, IGraph<int> graph)
        {
            var start = AcoUtils.Random.Next(0, Length);
            _tabuList = AcoUtils.BuildNewTrail(start, pheromones, graph);
        }
    }
}