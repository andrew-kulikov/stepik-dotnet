using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delegates.PairsAnalysis
{
    public static class Analysis
    {
        public static IEnumerable<Tuple<T, T>> Pairs<T>(this IEnumerable<T> items)
        {
            using (var enumerator = items.GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new ArgumentException();

                var cnt = 1;
                var prev = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    yield return new Tuple<T, T>(prev, enumerator.Current);

                    cnt++;
                    prev = enumerator.Current;
                }

                if (cnt == 1) throw new ArgumentException();
            }
        }

        public static int MaxIndex<T>(this IEnumerable<T> items) where T: IComparable
        {
            var orderedItems = items
                .Select((item, id) => (item, id))
                .OrderByDescending(p => p.item);

            try
            {
                return orderedItems.First().id;
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException();
            }
        }

        public static int FindMaxPeriodIndex(params DateTime[] data)
        {
            return data.Pairs().Select(p => p.Item2 - p.Item1).MaxIndex();
        }

        public static double FindAverageRelativeDifference(params double[] data)
        {
            return data.Pairs().Select(p => (p.Item2 - p.Item1) / p.Item1).Average();
        }
    }
}
