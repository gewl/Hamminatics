using System;   
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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
}
