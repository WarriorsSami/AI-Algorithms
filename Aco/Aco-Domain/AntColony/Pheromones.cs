using System.Linq;
using Aco_Domain.Graph;

namespace Aco_Domain.AntColony
{
    public class Pheromones
    {
        private readonly double[,] _pheromones;

        public int Length  => _pheromones.GetLength(0);
        
        public double this[int i, int j] => _pheromones[i, j];

        public Pheromones(int size)
        {
            _pheromones = new double[size, size];
            foreach (var i in Enumerable.Range(0, size))
            {
                foreach (var j in Enumerable.Range(0, size))
                {
                    _pheromones[i, j] = AcoUtils.InitialPheromone;
                }
            }
        }

        public void Update(Ant[] ants, IGraph<int> graph)
        {
            for (var i = 0; i < Length; i++)
            {
                for (var j = i + 1; j < Length; j++)
                {
                    foreach (var ant in ants)
                    {
                        var weight = AcoUtils.GetTrailWeight(ant, graph);
                        var decrease = (1 - AcoUtils.Rho) * _pheromones[i, j];
                        var increase = 0.0;
                        
                        if (AcoUtils.IsEdgeInAntTrail(i, j, ant))
                        {
                            increase = AcoUtils.Q / weight;
                        }
                        
                        _pheromones[i, j] = decrease + increase;
                        
                        if (_pheromones[i, j] < AcoUtils.PheroMin)
                        {
                            _pheromones[i, j] = AcoUtils.PheroMin;
                        }
                        if (_pheromones[i, j] > AcoUtils.PheroMax)
                        {
                            _pheromones[i, j] = AcoUtils.PheroMax;
                        }
                        
                        _pheromones[j, i] = _pheromones[i, j];
                    }
                }
            }
        }
    }
}
