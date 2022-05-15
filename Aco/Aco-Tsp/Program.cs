using System;
using System.Collections.Generic;
using Aco_Domain.AntColony;
using Aco_Domain.Graph;

namespace Aco_Tsp
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                const string filename = "..\\..\\..\\Data\\Cities.txt";
                var graph = GraphReader.ReadGraphAsAdjMatrix(filename);
                var ants = new List<Ant>(AcoUtils.NumAnts);
                
                for (var i = 0; i < AcoUtils.NumAnts; i++)
                {
                    ants.Add(new Ant(graph.NumberOfVertices, i));
                }

                var pheromones = new Pheromones(graph.NumberOfVertices);
                
                AcoUtils.RunAcoTsp(ants, graph, pheromones);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}