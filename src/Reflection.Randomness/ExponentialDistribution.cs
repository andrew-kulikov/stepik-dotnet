using System;

namespace Reflection.Randomness
{
    public class ExponentialDistribution : IContinuousDistribution
    {
        public readonly double Lambda;

        public ExponentialDistribution(double lambda)
        {
            Lambda = lambda;
        }

        public double Generate(Random rnd)
        {
            var u = rnd.NextDouble();
            return -Math.Log(u) / Lambda;
        }
    }
}
