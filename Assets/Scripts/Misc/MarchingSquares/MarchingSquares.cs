using System.Collections.Generic;
using UnityEngine;
using static Direction;

public class MarchingSquares {

    public static List<Vector2Int> March(Arr2d<bool> data) {
        var directions = new List<Vector2Int>();
        
        var x = 0;
        var y = 0;
        while (!data[x, y]) {
            x++;
            if (x >= data.Length1) {
                x = 0;
                y++;
            }

            if (y >= data.Length2) {
                Debug.LogError("MarchingSquares could not find valid path.");
                return new List<Vector2Int>();
            }
        }
        var previous = Vector2Int.zero;
        var startX = x;
        var startY = y;
        do {
            Vector2Int current;
            switch (Value(x, y, data)) {
                case  1: current = N; break;
                case  2: current = E; break;
                case  3: current = E; break;
                case  4: current = W; break;
                case  5: current = N; break;
                case  6: current = previous == N ? W : E; break;
                case  7: current = E; break;
                case  8: current = S; break;
                case  9: current = previous == E ? N : S; break;
                case 10: current = S; break;
                case 11: current = S; break;
                case 12: current = W; break;
                case 13: current = N; break;
                default: current = W; break;
            }
            directions.Add(current);
            x += current.x;
            y += current.y;
            previous = current;
        } while (x != startX || y != startY);
        
        return Path.SimplifyPath(directions);
    }

    private static int Value(int x, int y, Arr2d<bool> data) {
        var sum = 0;
        if (data[x-1, y-1]) sum |= 1;
        if (data[x, y-1]) sum |= 2;
        if (data[x-1, y]) sum |= 4;
        if (data[x, y]) sum |= 8;
        return sum;
    }

}