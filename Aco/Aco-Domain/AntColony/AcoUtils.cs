using System;
using System.Collections.Generic;
using System.Linq;
using Aco_Domain.Graph;

namespace Aco_Domain.AntColony
{
    public static class AcoUtils
    {
        public static readonly Random Random = new((int)DateTime.Now.Ticks & 0x0000FFFF);

        // influence of pheromone on direction
        public const int Alpha = 4;

        // influence of adjacent node distance
        public const int Beta = 3;

        // pheromone decrease factor
        public const double Rho = 0.01;

        // pheromone increase factor
        public const double Q = 2.0;

        public const int NumAnts = 10;
        public const int MaxTime = 10000;

        public const double InitialPheromone = 0.01;
        
        public const double TauetaMin = 0.0001;
        public const double TauetaMax = double.MaxValue / 2000;
        
        public const double PheroMin = 0.0001;
        public const double PheroMax = 100000.01;

        public static void RunAcoTsp(
            IEnumerable<Ant> ants,
            IGraph<int> graph,
            Pheromones pheromones
        )
        {
            DisplayLine("\nBegin Ant Colony Optimization for TSP", ConsoleColor.Yellow);

            DisplayLine(
                $"\nNumber of cities in problem = {graph.NumberOfVertices}",
                ConsoleColor.Yellow
            );

            DisplayLine($"\nNumber ants = {NumAnts}", ConsoleColor.Yellow);
            DisplayLine($"Maximum time = {MaxTime}", ConsoleColor.Yellow);

            DisplayLine($"\nAlpha (pheromone influence) = {Alpha}", ConsoleColor.Yellow);
            DisplayLine($"Beta (local node influence) = {Beta}", ConsoleColor.Yellow);
            DisplayLine($"Rho (pheromone evaporation coefficient) = {Rho:F2}", ConsoleColor.Yellow);
            DisplayLine($"Q (pheromone deposit factor) = {Q:F2}", ConsoleColor.Yellow);

            DisplayLine("\nInitializing graph has been completed", ConsoleColor.Yellow);
            DisplayLine(
                "Initializing ants on random trails has been completed",
                ConsoleColor.Yellow
            );
            DisplayLine(
                "Initializing pheromones on trails has been completed",
                ConsoleColor.Yellow
            );
            DisplayLine("\nInitializing pheromones has been completed", ConsoleColor.Yellow);
            Console.WriteLine();

            var antsArray = ants.ToArray();
            ShowAnts(antsArray, graph);

            var bestAnt = GetBestAntTrail(antsArray, graph);
            var bestWeight = GetTrailWeight(bestAnt, graph);

            DisplayLine($"\nBest initial trail length: {bestWeight:F1}", ConsoleColor.Yellow);

            var time = 0;
            DisplayLine("\nEntering UpdateAnts - UpdatePheromones loop\n", ConsoleColor.Red);

            while (time < MaxTime)
            {
                foreach (var ant in antsArray)
                {
                    ant.Update(pheromones, graph);
                }
                pheromones.Update(antsArray, graph);

                var currBestAnt = GetBestAntTrail(antsArray, graph);
                var currBestWeight = GetTrailWeight(currBestAnt, graph);
                if (currBestWeight < bestWeight)
                {
                    bestWeight = currBestWeight;
                    bestAnt = currBestAnt;
                    DisplayLine(
                        $"New best length of {bestWeight:F1} km found at time {time}",
                        ConsoleColor.Yellow
                    );
                }
                time++;
            }

            DisplayLine("\nTime complete", ConsoleColor.Red);

            DisplayLine("\nBest trail found:", ConsoleColor.Yellow);
            DisplayTrailForAnt(bestAnt, graph);
            DisplayLine($"\nLength of best trail found: {bestWeight:F1} km", ConsoleColor.Yellow);

            DisplayLine("\nEnd Ant Colony Optimization demo\n", ConsoleColor.Yellow);
        }

        private static void DisplayTrailForAnt(Ant ant, IGraph<int> graph)
        {
            for (var i = 0; i <= ant.Length - 1; i++)
            {
                Display(graph.NameFor[ant[i]] + " ", ConsoleColor.Cyan);
                if (i > 0 && i % 5 == 0)
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        private static void DisplayLine(string message, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        private static void Display(string message, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = oldColor;
        }

        private static void ShowAnts(IEnumerable<Ant> ants, IGraph<int> graph)
        {
            foreach (var ant in ants)
            {
                Console.Write(ant.Id + ": [ ");

                for (var j = 0; j <= 3; j++)
                {
                    Display(graph.NameFor[ant[j]] + " ", ConsoleColor.Cyan);
                }

                Console.Write(". . . ");

                for (var j = ant.Length - 4; j <= ant.Length - 1; j++)
                {
                    Display(graph.NameFor[ant[j]] + " ", ConsoleColor.Cyan);
                }

                Console.Write("] len = ");
                var len = GetTrailWeight(ant, graph);
                Display($"{len:F1} km", ConsoleColor.Green);
                Console.WriteLine();
            }
        }

        public static bool IsEdgeInAntTrail(int from, int to, Ant ant)
        {
            for (var i = 0; i < ant.Length - 1; i++)
            {
                if (ant[i] == from && ant[i + 1] == to)
                {
                    return true;
                }
            }
            
            if (ant[^1] == from && ant[0] == to)
            {
                return true;
            }
            
            if (ant[0] == from && ant[^1] == to)
            {
                return true;
            }
            
            return false;
        }

        public static int GetTrailWeight(Ant ant, IGraph<int> graph)
        {
            var sum = 0;
            for (var i = 0; i < ant.Length - 1; i++)
            {
                sum += graph[ant[i], ant[i + 1]];
            }

            return sum;
        }

        public static int[] GetRandomTrail(int start, int numCities)
        {
            var trail = new int[numCities];

            for (var i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            for (var i = 0; i <= numCities - 1; i++)
            {
                var r = Random.Next(i, numCities);
                (trail[r], trail[i]) = (trail[i], trail[r]);
            }

            var idx = GetIndexOfTarget(trail, start);
            (trail[0], trail[idx]) = (trail[idx], trail[0]);

            return trail;
        }

        public static int[] BuildNewTrail(int start, Pheromones pheromones, IGraph<int> graph)
        {
            var trail = new int[graph.NumberOfVertices];
            var visited = new bool[graph.NumberOfVertices];

            trail[0] = start;
            visited[start] = true;

            for (var i = 0; i < graph.NumberOfVertices - 1; i++)
            {
                var current = trail[i];
                var next = GetNextCity(current, pheromones, graph, visited);
                trail[i + 1] = next;
                visited[next] = true;
            }

            return trail;
        }

        private static int GetNextCity(
            int current,
            Pheromones pheromones,
            IGraph<int> graph,
            bool[] visited
        )
        {
            var cityProbabilities = GetMonteCarloProbabilities(current, pheromones, graph, visited);

            var cumulativeProbabilities = new double[cityProbabilities.Length + 1];
            for (var i = 0; i < cityProbabilities.Length; i++)
            {
                cumulativeProbabilities[i + 1] = cumulativeProbabilities[i] + cityProbabilities[i];
            }

            var p = Random.NextDouble();
            for (var i = 0; i <= cumulativeProbabilities.Length - 1; i++)
            {
                if (p >= cumulativeProbabilities[i] && p < cumulativeProbabilities[i + 1])
                {
                    return i;
                }
            }

            throw new Exception("Unable to find next city");
        }

        private static double[] GetMonteCarloProbabilities(
            int current,
            Pheromones pheromones,
            IGraph<int> graph,
            bool[] visited
        )
        {
            var taueta = new double[graph.NumberOfVertices];
            var sum = 0.0;

            for (var i = 0; i < graph.NumberOfVertices; i++)
            {
                if (i == current || visited[i])
                {
                    taueta[i] = 0;
                }
                else
                {
                    taueta[i] =
                        Math.Pow(pheromones[current, i], Alpha)
                        * Math.Pow(1.0 / graph[current, i], Beta);

                    // Normalize taueta
                    if (taueta[i] < TauetaMin)
                    {
                        taueta[i] = TauetaMin;
                    }
                    if (taueta[i] > TauetaMax)
                    {
                        taueta[i] = TauetaMax;
                    }
                    
                    sum += taueta[i];
                }
            }

            for (var i = 0; i < graph.NumberOfVertices; i++)
            {
                taueta[i] /= sum;
            }

            return taueta;
        }

        private static Ant GetBestAntTrail(Ant[] ants, IGraph<int> graph)
        {
            var bestLength = GetTrailWeight(ants[0], graph);
            var bestAnt = ants[0];

            for (var k = 1; k < ants.Length; k++)
            {
                var len = GetTrailWeight(ants[k], graph);
                if (len < bestLength)
                {
                    bestLength = len;
                    bestAnt = ants[k];
                }
            }

            return bestAnt;
        }

        private static int GetIndexOfTarget(IReadOnlyList<int> trail, int target)
        {
            for (var i = 0; i <= trail.Count - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            throw new Exception("Target index not found");
        }
    }
}
