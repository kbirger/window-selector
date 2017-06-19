using System;
using System.Linq;

namespace WindowSelector.Common
{
    public static class Extensions
    {
        public static bool ContainsCaseInsensitive(this string @this, string text)
        {
            if (@this == null) return false;
            return @this.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }


        public static bool In(this object @this, params object[] items)
        {
            return items.Contains(@this);
        }

        public static bool InRange<T>(this T @this, T minValue, T maxValue) where T: IComparable
        {
            return @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;
        }

        //public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        //{
        //    foreach (var t in @this)
        //    {
        //        action(t);
        //    }
        //}

    }
}
