using System;
using System.Collections.Generic;
using System.Reflection;

namespace Reflection.Randomness
{
    public class FromDistributionAttribute : Attribute
    {
        public FromDistributionAttribute(Type distributionType, params object[] parameters)
        {
            DistributionType = distributionType;
            Parameters = parameters;
        }

        public Type DistributionType { get; set; }
        public object[] Parameters { get; set; }
    }

    public class Generator<T>
    {
        private static readonly IDictionary<PropertyInfo, FromDistributionAttribute> PropertyDistributions =
            new Dictionary<PropertyInfo, FromDistributionAttribute>();

        static Generator()
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var distributionAttribute = property.GetCustomAttribute<FromDistributionAttribute>();

                if (distributionAttribute == null) continue;

                PropertyDistributions.Add(property, distributionAttribute);
            }
        }

        public T Generate(Random rnd)
        {
            var obj = Activator.CreateInstance<T>();

            foreach (var randomPropertyData in PropertyDistributions)
            {
                if (randomPropertyData.Value.DistributionType.GetInterface(nameof(IContinuousDistribution)) == null)
                    throw new ArgumentException(
                        $"Wrong distribution type. Expected {typeof(IContinuousDistribution)}, but got {randomPropertyData.Value.DistributionType}");

                try
                {
                    var distribution = (IContinuousDistribution) Activator.CreateInstance(
                        randomPropertyData.Value.DistributionType,
                        randomPropertyData.Value.Parameters);

                    randomPropertyData.Key.SetValue(obj, distribution.Generate(rnd));
                }
                catch (MissingMethodException)
                {
                    throw new ArgumentException(
                        $"Cannot find constructor of type {randomPropertyData.Value.DistributionType} with given amount of arguments");
                }
            }

            return obj;
        }
    }
}