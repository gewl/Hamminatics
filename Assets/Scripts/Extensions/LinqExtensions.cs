﻿using System;   
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public static class LinqExtensions {
    // Cribbed from: https://stackoverflow.com/questions/3645644/whats-your-favorite-linq-to-objects-operator-which-is-not-built-in/3645715#3645715
    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T element)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        return ConcatIterator(element, source, false);
    }

    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> tail, T head)
    {
        if (tail == null)
            throw new ArgumentNullException("tail");
        return ConcatIterator(head, tail, true);
    }

    private static IEnumerable<T> ConcatIterator<T>(T extraElement, IEnumerable<T> source, bool insertAtStart)
    {
        if (insertAtStart)
            yield return extraElement;
        foreach (var e in source)
            yield return e;
        if (!insertAtStart)
            yield return extraElement;
    }

    private static Random rng = new Random();  

    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static T GetRandomElement<T>(this IList<T> list)
    {
        return list[rng.Next(list.Count)];
    }

    public static T GetAndRemoveRandomElement<T>(this IList<T> list)
    {
        int randomIndex = rng.Next(list.Count);
        T itemAtIndex = list[randomIndex];
        list.RemoveAt(randomIndex);
        return itemAtIndex;
    }

    public static T GetRandomElementInRange<T>(this IList<T> list, int min, int max)
    {
        if (min < 0)
        {
            min = 0;
        }
        if (max > list.Count)
        {
            max = list.Count;
        }

        return list[rng.Next(min, max)];
    }

    // Source: https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property
    public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static void RemoveBySwap<T>(this List<T> list, int index)
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
    }

    public static void RemoveBySwap<T>(this List<T> list, Predicate<T> predicate)
    {
        int index = list.FindIndex(predicate);
        RemoveBySwap(list, index);
    }
}
