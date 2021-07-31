namespace GeneticFramework
{
    public abstract class Chromosome
    {
        public abstract double Fitness();

        public abstract Chromosome GetRandomInstance();

        public abstract (Chromosome, Chromosome) Crossover(Chromosome chromosome);

        public abstract void Mutate();
    }
}
