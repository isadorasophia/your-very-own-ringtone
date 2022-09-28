using System.Collections.Generic;

#nullable enable

namespace YourVeryOwnRingtone
{
    public static class Utils
    {
        /// <summary>
        /// Simple deconstruction for KeyValuePair.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey key, out TValue value)
        {
            key = source.Key;
            value = source.Value;
        }
    }
}
