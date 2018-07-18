using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  

public static class Operators  {
    public static Predicate<T> Or<T>(params Predicate<T>[] predicates)
    {
        return (T item) => predicates.Any(predicate => predicate(item));
    }

    public static Predicate<T> And<T>(params Predicate<T>[] predicates)
    {
        return (T item) => predicates.All(predicate => predicate(item));
    }
}
