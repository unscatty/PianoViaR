using System;
using System.Collections.Generic;
using System.Linq;

namespace PianoViaR.Helpers
{
    public class RandomIndexer
    {
        List<int> indexes;
        Random random = new Random();

        public RandomIndexer(int size)
        {
            indexes = Enumerable.Range(0, size).ToList();
        }

        public RandomIndexer(int size, int seed)
        : this(size)
        {
            random = new Random(seed);
        }

        public int Next()
        {
            if (indexes.Count <= 0)
            {
                throw new IndexOutOfRangeException("Index list is empty");
            }

            var randomIndex = random.Next(0, indexes.Count);

            return indexes.RemoveAndGetItem(randomIndex);
        }
    }
}