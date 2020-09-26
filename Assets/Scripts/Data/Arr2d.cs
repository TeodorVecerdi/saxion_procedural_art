using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arr2d<T> : IEnumerable<T> {
    private T[][] arr;
    private int length1;
    private int length2;

    public Arr2d(Vector2Int length, T initialValue = default) : this(length.x, length.y, initialValue) { }

    public Arr2d(int length1, int length2, T initialValue = default) {
        this.length1 = length1;
        this.length2 = length2;

        arr = new T[length1][];
        for (int i = 0; i < length1; i++) {
            arr[i] = new T[length2];
        }

        if (!EqualityComparer<T>.Default.Equals(initialValue, default)) {
            FillAll(initialValue);
        }
    }
    
    public void Expand(int dimension, int size, bool reverseSide = false, T value = default) {
        if (size == 0) return;
        var newLength1 = length1;
        var newLength2 = length2;
        var dimensionVec = new Vector2Int(0, 0);
        switch (dimension) {
            case 1:
                newLength1 += size;
                dimensionVec.x = size;
                break;
            case 2:
                newLength2 += size;
                dimensionVec.y = size;
                break;
            default:
                throw new InvalidOperationException($"Invalid dimension {dimension}. Should be one of: [1, 2].");
        }

        var arrTemp = new T[newLength1][];
        for (int i = 0; i < newLength1; i++) {
            arrTemp[i] = new T[newLength2];
                if (!EqualityComparer<T>.Default.Equals(value, default))
                    for (int j = 0; j < newLength2; j++)
                        arrTemp[i][j] = value;
        }

        for (int i = 0; i < length1; i++)
        for (int j = 0; j < length2; j++) {
            if (!reverseSide) {
                arrTemp[i][j] = arr[i][j];
            } else
                arrTemp[i + dimensionVec.x][j + dimensionVec.y] = arr[i][j];
        }

        arr = arrTemp;
        length1 = newLength1;
        length2 = newLength2;
    }

    public void FillAll(T value) {
        for (int i = 0; i < length1; i++)
        for (int j = 0; j < length2; j++)
            arr[i][j] = value;
    }

    public void Fill(Vector2Int from, Vector2Int to, T value) {
        for (int i = from.x; i < to.x; i++)
        for (int j = from.y; j < to.y; j++)
            arr[i][j] = value;
    }

    public int Length1 => length1;
    public int Length2 => length2;
    public Vector2Int Length => new Vector2Int(length1, length2);
    public T this[int x, int y] {
        get {
            if (x < 0 || x >= length1 || y < 0 || y >= length2)
                return default;
            return arr[x][y];
        }
        set {
            if (x < 0 || x >= length1 || y < 0 || y >= length2)
                return;
            arr[x][y] = value;
        }
    }
    public T this[Vector2Int index] {
        get => this[index.x, index.y];
        set => this[index.x, index.y] = value;
    }

    public IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < length1; i++)
        for (int j = 0; j < length2; j++) {
            if (arr[i][j] == null) continue;
            yield return arr[i][j];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}