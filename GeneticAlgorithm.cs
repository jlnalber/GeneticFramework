using System;
using System.Threading.Tasks;
using Utils;

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
        public bool UseBest;
        public Func<T, double> Fitness = t => t.Fitness();

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
            this.UseBest = false;
        }

        private void ReproduceAndReplace((T, double)[] scores)
        {
            Random random = new();
            T[] newPopulation = new T[this.Population.Length];

            for (int i = 0; i + 1 < this.Population.Length; i += 2)
            {
                (T, T) parents = (null, null);
                switch (this.SelectionType)
                {
                    case SelectionTypeEnum.Roulette: parents = PickRoulette(this.UseBest ? scores.AddToArray((this.Best.Item1, this.Best.Item2)) : scores); break;
                    case SelectionTypeEnum.Tournament: parents = PickTournament(this.UseBest ? scores.AddToArray((this.Best.Item1, this.Best.Item2)) : scores, (this.Population.Length + 1) / 2); break;
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
            (T, double)[] scores = await Task.Run(() => this.GetScores());
            this.Best = await Task.Run(() => scores.GetBest(((T, double) tupel) => tupel.Item2, ((T, double) tupel) => this.ExtraCondition(tupel.Item1)));

            for (int generation = 0; generation < this.MaxGenerations; generation++)
            {
                if (this.Best.Item2 >= this.Threshold && this.ExtraCondition(this.Best.Item1))
                {
                    return this.Best.Item1;
                }

                scores = await Task.Run(() => this.GetScores());
                await Task.Run(() => this.ReproduceAndReplace(scores));
                await Task.Run(() => this.Mutate());

                (T, double) highest = await Task.Run(() => scores.GetBest(((T, double) tupel) => tupel.Item2, ((T, double) tupel) => this.ExtraCondition(tupel.Item1)));
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
            (T, double)[] arr = RandomExt.PickRoulette(wheel, 2);
            return (arr[0].Item1, arr[1].Item1);
        }

        private static (T, T) PickTournament((T, double)[] tupels, int participants)
        {
            (T, double)[] arr = RandomExt.PickTournament(tupels, participants, 2);
            return (arr[0].Item1, arr[1].Item1);
        }

        public (T, double)[] GetScores()
        {
            (T, double)[] scores = new (T, double)[this.Population.Length];

            for (int i = 0; i < this.Population.Length; i++)
            {
                scores[i] = (this.Population[i], this.Fitness(this.Population[i]));
            }

            return scores;
        }
    }
}
