using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Arr3d<T> : IEnumerable<T> {
    private T[][][] arr;
    private int length1;
    private int length2;
    private int length3;

    public Arr3d(Vector3Int length, T initialValue = default) : this(length.x, length.y, length.z, initialValue) { }

    public Arr3d(int length1, int length2, int length3, T initialValue = default) {
        this.length1 = length1;
        this.length2 = length2;
        this.length3 = length3;

        arr = new T[length1][][];
        for (int i = 0; i < length1; i++) {
            arr[i] = new T[length2][];
            for (int j = 0; j < length2; j++)
                arr[i][j] = new T[length3];
        }

        if (!EqualityComparer<T>.Default.Equals(initialValue, default)) {
            FillAll(initialValue);
        }
    }

    public Arr2d<T> CrossSection(int y) {
        var arr2d = new Arr2d<T>(length1, length3);
        for(var i = 0; i < length1; i++)
        for (var j = 0; j < length3; j++)
            arr2d[i, j] = arr[i][y][j];
        return arr2d;
    }

    public void Expand(int dimension, int size, bool reverseSide = false, T value = default) {
        if (size == 0) return;
        var newLength1 = length1;
        var newLength2 = length2;
        var newLength3 = length3;
        var dimensionVec = new Vector3Int(0, 0, 0);
        switch (dimension) {
            case 1:
                newLength1 += size;
                dimensionVec.x = size;
                break;
            case 2:
                newLength2 += size;
                dimensionVec.y = size;
                break;
            case 3:
                newLength3 += size;
                dimensionVec.z = size;
                break;
            default:
                throw new InvalidOperationException($"Invalid dimension {dimension}. Should be one of: [1, 2, 3].");
        }

        var arrTemp = new T[newLength1][][];
        for (int i = 0; i < newLength1; i++) {
            arrTemp[i] = new T[newLength2][];
            for (int j = 0; j < newLength2; j++) {
                arrTemp[i][j] = new T[newLength3];
                if (!EqualityComparer<T>.Default.Equals(value, default))
                    for (int k = 0; k < newLength3; k++)
                        arrTemp[i][j][k] = value;
            }
        }

        for (int i = 0; i < length1; i++)
        for (int j = 0; j < length2; j++)
        for (int k = 0; k < length3; k++) {
            if (!reverseSide) {
                arrTemp[i][j][k] = arr[i][j][k];
            } else
                arrTemp[i + dimensionVec.x][j + dimensionVec.y][k + dimensionVec.z] = arr[i][j][k];
        }

        arr = arrTemp;
        length1 = newLength1;
        length2 = newLength2;
        length3 = newLength3;
    }

    public void FillAll(T value) {
        for (int i = 0; i < length1; i++)
        for (int j = 0; j < length2; j++)
        for (int k = 0; k < length3; k++)
            arr[i][j][k] = value;
    }

    public void Fill(Vector3Int from, Vector3Int to, T value) {
        for (int i = from.x; i < to.x; i++)
        for (int j = from.y; j < to.y; j++)
        for (int k = from.z; k < to.z; k++)
            arr[i][j][k] = value;
    }

    public int Length1 => length1;
    public int Length2 => length2;
    public int Length3 => length3;
    public Vector3Int Length => new Vector3Int(length1, length2, length3);
    public T this[int x, int y, int z] {
        get {
            if (x < 0 || x >= length1 || y < 0 || y >= length2 || z < 0 || z >= length3)
                return default;
            return arr[x][y][z];
        }
        set {
            if (x < 0 || x >= length1 || y < 0 || y >= length2 || z < 0 || z >= length3)
                return;
            arr[x][y][z] = value;
        }
    }
    public T this[Vector3Int index] {
        get => this[index.x, index.y, index.z];
        set => this[index.x, index.y, index.z] = value;
    }

    public IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < length1; i++)
        for (int j = 0; j < length2; j++)
        for (int k = 0; k < length3; k++) {
            if (arr[i][j][k] == null) continue;
            yield return arr[i][j][k];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}