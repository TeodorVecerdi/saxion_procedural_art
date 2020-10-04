using System.Collections.Generic;
using UnityEngine;

public static class ListUtils {
    public static List<T> Combine<T>(params List<T>[] lists) {
        var combined = new List<T>();
        foreach (var list in lists) {
            combined.AddRange(list);
        }

        return combined;
    }
}