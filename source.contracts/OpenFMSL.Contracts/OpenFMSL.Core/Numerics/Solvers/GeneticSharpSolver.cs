using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics.Solvers
{

    public enum DecisionType { Continuous, Integer, Discrete };

    public class DesignDecision
    {
        public string Name { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public DecisionType Type { get; set; } = DecisionType.Continuous;

        public int TotalBits
        {
            get
            {
                switch (Type)
                {
                    case DecisionType.Continuous:
                        return 16;
                    case DecisionType.Integer:
                        return 16;
                    case DecisionType.Discrete:
                        return 1;
                    default:
                        return 16;
                }
            }
        }

        public int FractionDigits
        {
            get
            {
                switch (Type)
                {
                    case DecisionType.Continuous:
                        return 3;
                    case DecisionType.Integer:
                        return 0;
                    case DecisionType.Discrete:
                        return 0;
                    default:
                        return 0;
                }
            }
        }


    }

    public class GeneticSharpSolver
    {
        Func<FloatingPointChromosome, GeneticSharpSolver, double> _objective;
        Action<GeneticAlgorithm> _generationCallback;
        GeneticAlgorithm _ga;
        int _populationSize = 10;
        int _offspringNumber = 20;
        int _maxGenerations = 5;
        List<DesignDecision> _decisions = new List<DesignDecision>();

        public int CurrentGeneration
        {
            get
            {
                if (_ga != null)
                    return _ga.GenerationsNumber;
                else
                    return -1;
            }
        }
        public GeneticSharpSolver AddDecision(DesignDecision decision)
        {
            _decisions.Add(decision);
            return this;
        }
        public GeneticSharpSolver AddDoubleDecision(string name, double min, double max)
        {
            _decisions.Add(new DesignDecision { Name = name, LowerBound = min, UpperBound = max, Type = DecisionType.Continuous });
            return this;
        }
        public GeneticSharpSolver AddIntegerDecision(string name, double min, double max)
        {
            _decisions.Add(new DesignDecision { Name = name, LowerBound = min, UpperBound = max, Type = DecisionType.Integer });
            return this;
        }


        public GeneticSharpSolver SetObjectiveFunction(Func<FloatingPointChromosome, GeneticSharpSolver, double> objective)
        {
            _objective = objective;
            return this;
        }

        public GeneticSharpSolver SetGenerationCallback(Action<GeneticAlgorithm> callback)
        {
            _generationCallback = callback;
            return this;
        }

        public GeneticSharpSolver SetGenerationInfo(int populationSize, int offspringNumber)
        {
            _populationSize = populationSize;
            _offspringNumber = offspringNumber;
            return this;
        }
        public GeneticSharpSolver SetMaximumGenerations(int maxGenerations)
        {
            _maxGenerations = maxGenerations;
            return this;
        }


        public GeneticSharpSolver Setup()
        {
            var chromosome = new FloatingPointChromosome(
             _decisions.Select(d => d.LowerBound).ToArray(),
             _decisions.Select(d => d.UpperBound).ToArray(),
              _decisions.Select(d => d.TotalBits).ToArray(),
              _decisions.Select(d => d.FractionDigits).ToArray());

            var population = new Population(_populationSize, _populationSize + _offspringNumber, chromosome);

            var fitness = new FuncFitness((c) => { return _objective(c as FloatingPointChromosome, this); });
            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            var mutation = new FlipBitMutation();
            var termination = new GenerationNumberTermination(_maxGenerations);

            _ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            _ga.Termination = termination;
            if (_generationCallback != null)
                _ga.GenerationRan += (sender, e) => _generationCallback(_ga);

            return this;
        }

        public GeneticSharpSolver Run()
        {
            if (_ga == null)
                Setup();

            _ga.Start();
            return this;
        }


    }

    /*
     * class MainClass
    {
        public static void Main(string[] args)
        {
            float maxWidth = 998f;
            float maxHeight = 680f;

            var chromosome = new FloatingPointChromosome(
                new double[] { 0, 0, 0, 0 },
                new double[] { maxWidth, maxHeight, maxWidth, maxHeight },
                new int[] { 10, 10, 10, 10 },
                new int[] { 0, 0, 0, 0 });
            
            var population = new Population(50, 100, chromosome);

            var fitness = new FuncFitness((c) =>
            {
                var fc = c as FloatingPointChromosome;

                var values = fc.ToFloatingPoints();
                var x1 = values[0];
                var y1 = values[1];
                var x2 = values[2];
                var y2 = values[3];

                return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            });

            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            var mutation = new FlipBitMutation();
            var termination = new FitnessStagnationTermination(100);

            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            ga.Termination = termination;

            Console.WriteLine("Generation: (x1, y1), (x2, y2) = distance");

            var latestFitness = 0.0;

            ga.GenerationRan += (sender, e) =>
            {
                var bestChromosome = ga.BestChromosome as FloatingPointChromosome;
                var bestFitness = bestChromosome.Fitness.Value;

                if (bestFitness != latestFitness)
                {
                    latestFitness = bestFitness;
                    var phenotype = bestChromosome.ToFloatingPoints();

                    Console.WriteLine(
                        "Generation {0,2}: ({1},{2}),({3},{4}) = {5}",
                        ga.GenerationsNumber,
                        phenotype[0],
                        phenotype[1],
                        phenotype[2],
                        phenotype[3],
                        bestFitness
                    );
                }
            };

            ga.Start();

            Console.ReadKey();
        }
    }
     * */
}
