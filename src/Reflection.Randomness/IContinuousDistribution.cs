using System;

namespace Reflection.Randomness
{
    public interface IContinuousDistribution
    {
        double Generate(Random rnd);
    }
}
