using System;
using System.Threading.Tasks;

namespace GeneticFramework
{
    public class GeneticAlgorithm<T> where T : Chromosome
    {
        public T[] Population;
        public double Threshold;
        public int MaxGenerations;
        public double MutationChance;
        public double CrossoverChance;
        public SelectionTypeEnum SelectionType;
        public Func<T, bool> ExtraCondition;
        public Action<int, T[], (T, double)> ForEachGeneration;
        private (T, double) Best;

        public enum SelectionTypeEnum
        {
            Roulette, Tournament
        }

        public GeneticAlgorithm(T[] intialPopulation, double threshold, int maxGenerations = 100, double mutationChance = 0.01, double crossoverChance = 0.7, SelectionTypeEnum selectionType = SelectionTypeEnum.Tournament)
        {
            this.Population = intialPopulation;
            this.Threshold = threshold;
            this.MaxGenerations = maxGenerations;
            this.MutationChance = mutationChance;
            this.CrossoverChance = crossoverChance;
            this.SelectionType = selectionType;
            this.ExtraCondition = (T _) => true;
            this.ForEachGeneration = (int _, T[] _, (T, double) _) => { };
        }

        private void ReproduceAndReplace((T, double)[] scores, bool useBest = true)
        {
            Random random = new();
            T[] newPopulation = new T[this.Population.Length];

            for (int i = 0; i + 1 < this.Population.Length; i += 2)
            {
                (T, T) parents = (null, null);
                switch (this.SelectionType)
                {
                    case SelectionTypeEnum.Roulette: parents = PickRoulette(useBest ? scores.AddToArray(this.Best) : scores); break;
                    case SelectionTypeEnum.Tournament: parents = PickTournament(useBest ? scores.AddToArray(this.Best) : scores, this.Population.Length / 2); break;
                }

                if (random.NextDouble() < this.CrossoverChance)
                {
                    (T, T) newChromosomes = (((T, T))(parents.Item1).Crossover(parents.Item2));
                    newPopulation[i] = newChromosomes.Item1;
                    newPopulation[i + 1] = newChromosomes.Item2;
                }
                else
                {
                    newPopulation[i] = parents.Item1;
                    newPopulation[i + 1] = parents.Item2;
                }
            }

            if (this.Population.Length % 2 == 1)
            {
                newPopulation[this.Population.Length - 1] = this.Population[random.Next(this.Population.Length)];
            }

            this.Population = newPopulation;
        }

        private void Mutate()
        {
            Random random = new();
            foreach (T i in this.Population)
            {
                if (random.NextDouble() < this.MutationChance)
                {
                    i.Mutate();
                }
            }
        }

        public async Task<T> RunAsync()
        {
            (T, double)[] scores = this.GetScores();
            this.Best = Utils.GetBest(scores, ((T, double) tupel) => tupel.Item2, ((T, double) tupel) => this.ExtraCondition(tupel.Item1));

            for (int generation = 0; generation < this.MaxGenerations; generation++)
            {
                if (this.Best.Item2 >= this.Threshold && this.ExtraCondition(this.Best.Item1))
                {
                    return this.Best.Item1;
                }
                
                scores = this.GetScores();
                await Task.Run(() => this.ReproduceAndReplace(scores));
                await Task.Run(() => this.Mutate());

                (T, double) highest = Utils.GetBest(scores, ((T, double) tupel) => tupel.Item2, ((T, double) tupel) => this.ExtraCondition(tupel.Item1));
                bool extraHighest = await Task.Run(() => this.ExtraCondition(highest.Item1));
                bool extraBest = await Task.Run(() => this.ExtraCondition(this.Best.Item1));
                if ((highest.Item2 > this.Best.Item2 && !(extraHighest ^ extraBest)) || (extraHighest && !extraBest))
                {
                    this.Best = highest;
                }

                this.ForEachGeneration(generation, this.Population, this.Best);
            }

            return this.Best.Item1;
        }

        private static (T, T) PickRoulette((T, double)[] wheel)
        {
            T[] arr = Utils.Choices(wheel, 2);
            return (arr[0], arr[1]);
        }
    
        private static (T, T) PickTournament((T, double)[] tupels, int participants)
        {
            (T, double)[] arr = Utils.NElements(tupels, participants);
            (T, double)[] arr2 = Utils.NLargest(arr, 2, ((T, double) i) => i.Item2);
            return (arr2[0].Item1, arr2[1].Item1);
        }

        public (T, double)[] GetScores()
        {
            (T, double)[] scores = new (T, double)[this.Population.Length];

            for (int i = 0; i < this.Population.Length; i++)
            {
                scores[i] = (this.Population[i], this.Population[i].Fitness());
            }

            return scores;
        }
    }
}