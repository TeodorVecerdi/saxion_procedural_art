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

    public static (List<T1>, List<T2>) Combine<T1, T2>(params (List<T1>, List<T2>)[] lists) {
        var combinedT1 = new List<T1>();
        var combinedT2 = new List<T2>();
        foreach (var (t1List, t2List) in lists) {
            combinedT1.AddRange(t1List);
            combinedT2.AddRange(t2List);
        }

        return (combinedT1, combinedT2);
    }
    
    public static (List<Vector3>, List<int>) Combine(params (List<Vector3>, List<int>)[] lists) {
        var combinedT1 = new List<Vector3>();
        var combinedT2 = new List<int>();
        combinedT1.AddRange(lists[0].Item1);
        combinedT2.AddRange(lists[0].Item2);
        for (var index = 1; index < lists.Length; index++) {
            var (t1List, t2List) = lists[index];
            var offset = combinedT1.Count;
            combinedT1.AddRange(t1List);
            var newList = new List<int>(t2List);
            for (var i = 0; i < newList.Count; i++) {
                newList[i] += offset;
            }

            combinedT2.AddRange(newList);
        }

        return (combinedT1, combinedT2);
    }
}