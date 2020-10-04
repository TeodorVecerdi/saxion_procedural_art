using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils {
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
    
    public static (List<Vector3>, List<int>) Combine(List<(List<Vector3>, List<int>)> lists) {
        var combinedT1 = new List<Vector3>();
        var combinedT2 = new List<int>();
        combinedT1.AddRange(lists[0].Item1);
        combinedT2.AddRange(lists[0].Item2);
        for (var index = 1; index < lists.Count; index++) {
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