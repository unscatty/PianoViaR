using System.Collections.Generic;

namespace PianoViaR.Helpers
{
    public static class ListExtensions
    {
        public static T RemoveAndGetItem<T>(this IList<T> list, int iIndexToRemove)
        {
            var item = list[iIndexToRemove];
            list.RemoveAt(iIndexToRemove);
            return item;
        }
    }
}