using System.Collections.Generic;
using UnityEngine;

public static class Path {
    public static List<Vector2Int> SimplifyPath(List<Vector2Int> directions) {
        var simplified = new List<Vector2Int> {directions[0]};
        var counts = new List<int> {1};
        
        for (var i = 1; i < directions.Count; i++) {
            if (directions[i] != simplified[simplified.Count - 1]) {
                simplified.Add(directions[i]);
                counts.Add(1);
            }
            else counts[simplified.Count - 1]++;
        }

        for (var i = 0; i < simplified.Count; i++) {
            simplified[i] *= counts[i];
        }
        return simplified;
    }
}