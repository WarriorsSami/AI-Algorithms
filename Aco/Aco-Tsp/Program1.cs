using System;

// Demo of Ant Colony Optimization (ACO) solving a Traveling Salesman Problem (TSP).
// There are many variations of ACO; this is just one approach.
// The problem to solve has a program defined number of cities. We assume that every
// city is connected to every other city. The distance between cities is artificially
// set so that the distance between any two cities is a random value between 1 and 8
// Cities wrap, so if there are 20 cities then D(0,19) = D(19,0).
// Free parameters are alpha, beta, rho, and Q. Hard-coded constants limit min and max
// values of pheromones.

namespace Aco_Tsp
{
    internal static class AntColonyProgram
    {
        private static readonly Random Random = new((int) DateTime.Now.Ticks & 0x0000FFFF);
        // influence of pheromone on direction
        private const int Alpha = 3;

        // influence of adjacent node distance
        private const int Beta = 2;

        // pheromone decrease factor
        private const double Rho = 0.01;

        // pheromone increase factor
        private const double Q = 2.0;

        public static void Main1(string[] args)
        {
            try
            {
                Console.WriteLine("\nBegin Ant Colony Optimization demo\n");

                var numCities = 60;
                var numAnts = 4;
                var maxTime = 1000;

                Console.WriteLine("Number cities in problem = " + numCities);

                Console.WriteLine("\nNumber ants = " + numAnts);
                Console.WriteLine("Maximum time = " + maxTime);

                Console.WriteLine("\nAlpha (pheromone influence) = " + Alpha);
                Console.WriteLine("Beta (local node influence) = " + Beta);
                Console.WriteLine("Rho (pheromone evaporation coefficient) = " + Rho.ToString("F2"));
                Console.WriteLine("Q (pheromone deposit factor) = " + Q.ToString("F2"));

                Console.WriteLine("\nInitialing dummy graph distances");
                var dists = MakeGraphDistances(numCities);

                Console.WriteLine("\nInitialing ants to random trails\n");
                var ants = InitAnts(numAnts, numCities);
                // initialize ants to random trails
                ShowAnts(ants, dists);

                var bestTrail = BestTrail(ants, dists);
                // determine the best initial trail
                var bestLength = Length(bestTrail, dists);
                // the length of the best trail

                Console.Write("\nBest initial trail length: " + bestLength.ToString("F1") + "\n");
                //Display(bestTrail);

                Console.WriteLine("\nInitializing pheromones on trails");
                var pheromones = InitPheromones(numCities);

                var time = 0;
                Console.WriteLine("\nEntering UpdateAnts - UpdatePheromones loop\n");
                while (time < maxTime)
                {
                    UpdateAnts(ants, pheromones, dists);
                    UpdatePheromones(pheromones, ants, dists);

                    var currBestTrail = BestTrail(ants, dists);
                    var currBestLength = Length(currBestTrail, dists);
                    if (currBestLength < bestLength)
                    {
                        bestLength = currBestLength;
                        bestTrail = currBestTrail;
                        Console.WriteLine("New best length of " + bestLength.ToString("F1") + " found at time " + time);
                    }
                    time += 1;
                }

                Console.WriteLine("\nTime complete");

                Console.WriteLine("\nBest trail found:");
                Display(bestTrail);
                Console.WriteLine("\nLength of best trail found: " + bestLength.ToString("F1"));

                Console.WriteLine("\nEnd Ant Colony Optimization demo\n");
                
                Console.WriteLine("Enter any key to exit...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }
        // Main

        // --------------------------------------------------------------------------------------------

        private static int[][] InitAnts(int numAnts, int numCities)
        {
            var ants = new int[numAnts][];
            for (var k = 0; k <= numAnts - 1; k++)
            {
                var start = Random.Next(0, numCities);
                ants[k] = RandomTrail(start, numCities);
            }
            return ants;
        }

        private static int[] RandomTrail(int start, int numCities)
        {
            // helper for InitAnts
            var trail = new int[numCities];

            // sequential
            for (var i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            // Fisher-Yates shuffle
            for (var i = 0; i <= numCities - 1; i++)
            {
                var r = Random.Next(i, numCities);
                (trail[r], trail[i]) = (trail[i], trail[r]);
            }

            var idx = IndexOfTarget(trail, start);
            // put start at [0]
            (trail[0], trail[idx]) = (trail[idx], trail[0]);

            return trail;
        }

        private static int IndexOfTarget(int[] trail, int target)
        {
            // helper for RandomTrail
            for (var i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            throw new Exception("Target not found in IndexOfTarget");
        }

        private static double Length(int[] trail, int[][] dists)
        {
            // total length of a trail
            var result = 0.0;
            for (var i = 0; i <= trail.Length - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1], dists);
            }
            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        private static int[] BestTrail(int[][] ants, int[][] dists)
        {
            // best trail has shortest total length
            var bestLength = Length(ants[0], dists);
            var idxBestLength = 0;
            for (var k = 1; k <= ants.Length - 1; k++)
            {
                var len = Length(ants[k], dists);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            var numCities = ants[0].Length;
            var bestTrailRenamed = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrailRenamed, 0);
            return bestTrailRenamed;
        }

        // --------------------------------------------------------------------------------------------

        private static double[][] InitPheromones(int numCities)
        {
            var pheromones = new double[numCities][];
            for (var i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }
            for (var i = 0; i <= pheromones.Length - 1; i++)
            {
                for (var j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                    // otherwise first call to UpdateAnts -> BuildTrail -> NextNode -> MoveProbs => all 0.0 => throws
                }
            }
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
        {
            var numCities = pheromones.Length;
            for (var k = 0; k <= ants.Length - 1; k++)
            {
                var start = Random.Next(0, numCities);
                var newTrail = BuildTrail(start, pheromones, dists);
                ants[k] = newTrail;
            }
        }

        private static int[] BuildTrail(int start, double[][] pheromones, int[][] dists)
        {
            var numCities = pheromones.Length;
            var trail = new int[numCities];
            var visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (var i = 0; i <= numCities - 2; i++)
            {
                var cityX = trail[i];
                var next = NextCity(cityX, visited, pheromones, dists);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }

        private static int NextCity(int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            var probs = MoveProbs(cityX, visited, pheromones, dists);

            var cumul = new double[probs.Length + 1];
            for (var i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
                // consider setting cumul[cumul.Length-1] to 1.00
            }

            var p = Random.NextDouble();

            for (var i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }
            throw new Exception("Failure to return valid city in NextCity");
        }

        private static double[] MoveProbs(int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            var numCities = pheromones.Length;
            var taueta = new double[numCities];
            // inclues cityX and visited cities
            var sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (var i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                }
                else if (visited[i])
                {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                }
                else
                {
                    taueta[i] = Math.Pow(pheromones[cityX][i], Alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), Beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            var probs = new double[numCities];
            for (var i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }
            return probs;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            for (var i = 0; i <= pheromones.Length - 1; i++)
            {
                for (var j = i + 1; j <= pheromones[i].Length - 1; j++)
                {
                    for (var k = 0; k <= ants.Length - 1; k++)
                    {
                        var length = Length(ants[k], dists);
                        // length of ant k trail
                        var decrease = (1.0 - Rho) * pheromones[i][j];
                        var increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]))
                        {
                            increase = Q / length;
                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private static bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            var lastIndex = trail.Length - 1;
            var idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY)
            {
                return true;
            }

            if (idx == 0 && trail[lastIndex] == cityY)
            {
                return true;
            }
            if (idx == 0)
            {
                return false;
            }
            if (idx == lastIndex && trail[lastIndex - 1] == cityY)
            {
                return true;
            }
            if (idx == lastIndex && trail[0] == cityY)
            {
                return true;
            }
            if (idx == lastIndex)
            {
                return false;
            }
            if (trail[idx - 1] == cityY)
            {
                return true;
            }
            if (trail[idx + 1] == cityY)
            {
                return true;
            }
            return false;
        }


        // --------------------------------------------------------------------------------------------

        private static int[][] MakeGraphDistances(int numCities)
        {
            var dists = new int[numCities][];
            for (var i = 0; i <= dists.Length - 1; i++)
            {
                dists[i] = new int[numCities];
            }
            for (var i = 0; i <= numCities - 1; i++)
            {
                for (var j = i + 1; j <= numCities - 1; j++)
                {
                    var d = Random.Next(1, 9);
                    // [1,8]
                    dists[i][j] = d;
                    dists[j][i] = d;
                }
            }
            return dists;
        }

        private static double Distance(int cityX, int cityY, int[][] dists)
        {
            return dists[cityX][cityY];
        }

        // --------------------------------------------------------------------------------------------

        private static void Display(int[] trail)
        {
            for (var i = 0; i <= trail.Length - 1; i++)
            {
                Console.Write(trail[i] + " ");
                if (i > 0 && i % 20 == 0)
                {
                    Console.WriteLine("");
                }
            }
            Console.WriteLine("");
        }


        private static void ShowAnts(int[][] ants, int[][] dists)
        {
            for (var i = 0; i <= ants.Length - 1; i++)
            {
                Console.Write(i + ": [ ");

                for (var j = 0; j <= 3; j++)
                {
                    Console.Write(ants[i][j] + " ");
                }

                Console.Write(". . . ");

                for (var j = ants[i].Length - 4; j <= ants[i].Length - 1; j++)
                {
                    Console.Write(ants[i][j] + " ");
                }

                Console.Write("] len = ");
                var len = Length(ants[i], dists);
                Console.Write(len.ToString("F1"));
                Console.WriteLine("");
            }
        }

        private static void Display(double[][] pheromones)
        {
            for (var i = 0; i <= pheromones.Length - 1; i++)
            {
                Console.Write(i + ": ");
                for (var j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    Console.Write(pheromones[i][j].ToString("F4").PadLeft(8) + " ");
                }
                Console.WriteLine("");
            }

        }
    }
}