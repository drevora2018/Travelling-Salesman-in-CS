using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_GA
{
    class MainProgram
    {
        public List<Cities> cities = new List<Cities>();
        public List<Individuals> individuals = new List<Individuals>();
        public List<Individuals> BestIndividuals = new List<Individuals>();

        /// <summary>
        /// Creates a list of cities from textfile.
        /// </summary>
        /// <param name="path"></param>
        public void AddToDS(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] values = line.Split(' ');
                cities.Add(new Cities(int.Parse(values[0]) - 1, double.Parse(values[1]), double.Parse(values[2])));
            }
        }

        /// <summary>
        /// Selects the best individual from the sample population. 
        /// Picks parents fit for breeding and breeds them (and possibly the child will be mutated) 
        /// </summary>
        /// <param name="TournamentSize"></param>
        /// <returns>List of individuals and their children to be returned to the population </returns>
        public List<Individuals> TournamentSelection(int tournamentSize, int Elitism, int MutationChance)
        {
            Random rnd = new Random();
            List<Individuals> best = new List<Individuals>();
            List<Individuals> losers = new List<Individuals>();

            while (individuals.Count >= tournamentSize + Elitism)
            {
                if (individuals.Count < tournamentSize)
                    break;
                List<Individuals> tournament = new List<Individuals>();
                //pick out random individuals for fighting purposes
                for (int i = 0; i < tournamentSize; i++)
                {
                    int index = rnd.Next(0, individuals.Count);
                    tournament.Add(individuals[index]);
                    individuals.RemoveAt(index);
                }

                //pick out parents
                //this is the fighting part, oh yeah babyyyyy fight to the death
                Individuals bestInd = tournament[0];
                for (int i = 1; i < tournament.Count; i++)
                {
                    if (tournament[i].Fitness < bestInd.Fitness)
                    {
                        bestInd = tournament[i];
                    }
                }
                //a winner has been chosen, now we add it to best
                best.Add(bestInd);
            }

            List<Individuals> children = new List<Individuals>();
            
            //The name Children is a little weird to have here, but we just keep the Elitism best individuals from the population and select them to be put back
            //into the pool of individuals, ordered by fitness.
            foreach (var item in best.OrderBy(x => x.Fitness).Take(Elitism))
            {
                children.Add(item);
            }

            //Select two RANDOM parents from the best list, make them have sex and produce baby
            //The babys genes are a product of a crossover from the genes of the two parents
            //Then add them back into the pool
            for (int i = 0; i < best.Count; i++) { 
                var random = rnd.Next(best.Count-1);
                var random2 = rnd.Next(best.Count-1);
                var child = CrossOver(best.ElementAt(random), best.ElementAt(random2));
                child.CalculcateExternalFitness();
                children.Add(child);
                
            }

            //mutate based on the parents
            //idk why we endorse mutation, i guess it helps with widening the search field but at the same time
            //who wants to expose their child to radioactivity???
            //idfk but we add their radioactive ass back to the pool as well
            for (int i = 0; i < children.Count; i++)
            {
                if (rnd.Next(0, MutationChance) == 0)
                {
                    var individualToAdd = Mutation(children[i]);
                }
            }

            children.AddRange(best);
            
            return children;
        }

        /// <summary>
        /// This method represents the crossover operation in a genetic algorithm for the TSP. 
        /// Crossover involves selecting a random segment of genes from one parent (Parent1) and filling the remaining positions with genes from another parent (Parent2)
        /// to create a new individual (offspring).
        /// </summary>
        /// <param name="Parent1"></param>
        /// <param name="Parent2"></param>
        /// <returns>A child of two parents</returns>
        public Individuals CrossOver(Individuals Parent1, Individuals Parent2)
        {
            Random rand = new Random();
            int[] crossover = new int[53];
            var ActualCities = new List<Cities>();

            for (int i = 0; i < crossover.Length; i++)
            {
                crossover[i] = -1;
            }
            //select a random range in crossover
            int point1 = rand.Next(1, 52);
            int point2 = rand.Next(point1 + 1, 52);

            for (int j = point1; j < point2; j++)
            {
                crossover[j] = Parent1.CitiesVisited[j];
            }

            List<int> remainingGenes = Parent2.CitiesVisited.Except(crossover).ToList();
            for (int i = 0; i < crossover.Length-1; i++)
            {
                if (crossover[i] == -1)
                {
                    crossover[i] = remainingGenes[0];
                    remainingGenes.RemoveAt(0);
                }
            }
            crossover[52] = 0;

            foreach (var item in crossover)
            {
                ActualCities.Add(cities.Find(x => x.ID == item));
            }


            return new Individuals(crossover, ActualCities);
        }

        /// <summary>
        /// Takes values between two points, and reverses them.
        /// </summary>
        /// <param name="Individual"></param>
        /// <returns></returns>
        public Individuals Mutation(Individuals Individual)
        {
            Random rand = new Random();
            var ActualCities = new List<Cities>();

            int point1 = rand.Next(1, 52);
            int point2 = rand.Next(point1+1, 52);

            Individual.CitiesVisited[point1..point2].Reverse();
            Individual.Cities.Clear();
            foreach (var item in Individual.CitiesVisited)
            {
                Individual.Cities.Add(cities.Find(x => x.ID == item));
            }
            Individual.CalculcateExternalFitness();
            return Individual;
        }
    }
}
