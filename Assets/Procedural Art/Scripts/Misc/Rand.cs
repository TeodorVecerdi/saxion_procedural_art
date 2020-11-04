using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Took this from the source code of an amazing game called RimWorld.
/// I am not proud of this but for the scope of this project I didn't have time to implement something like this.
/// The Unity Random is really bad.
/// </summary>
public static class Rand {
    private static Stack<ulong> stateStack = new Stack<ulong>();
    private static RandomNumberGenerator random = new RandomNumberGenerator();
    private static uint iterations = 0;
    private static List<int> tmpRange = new List<int>();

    static Rand() {
        random.seed = (uint) DateTime.Now.GetHashCode();
    }

    public static int Seed {
        set {
            if (stateStack.Count == 0)
                Debug.LogError("Modifying the initial rand seed. Call PushState() first. The initial rand seed should always be based on the startup time and set only once.");
            random.seed = (uint) value;
            iterations = 0U;
        }
    }

    public static float Value => random.GetFloat(iterations++);

    public static bool Bool => Value < 0.5;

    public static int Sign => Bool ? 1 : -1;

    public static int Int => random.GetInt(iterations++);

    public static Vector3 UnitVector3 => new Vector3(Gaussian(), Gaussian(), Gaussian()).normalized;

    public static Vector2 UnitVector2 => new Vector2(Gaussian(), Gaussian()).normalized;

    public static Vector2 InsideUnitCircle {
        get {
            Vector2 vector2;
            do {
                vector2 = new Vector2(Value - 0.5f, Value - 0.5f) * 2f;
            } while (vector2.sqrMagnitude > 1.0);

            return vector2;
        }
    }

    public static Vector3 InsideUnitCircleVec3 {
        get {
            Vector2 insideUnitCircle = InsideUnitCircle;
            return new Vector3(insideUnitCircle.x, 0.0f, insideUnitCircle.y);
        }
    }

    private static ulong StateCompressed {
        get => random.seed | (ulong) iterations << 32;
        set {
            random.seed = (uint) (value & uint.MaxValue);
            iterations = (uint) (value >> 32 & uint.MaxValue);
        }
    }

    public static void EnsureStateStackEmpty() {
        if (stateStack.Count <= 0)
            return;
        Debug.LogWarning("Random state stack is not empty. There were more calls to PushState than PopState. Fixing.");
        while (stateStack.Any())
            PopState();
    }

    public static float Gaussian(float centerX = 0.0f, float widthFactor = 1f) {
        return Mathf.Sqrt(-2f * Mathf.Log(Value)) * Mathf.Sin(6.283185f * Value) * widthFactor + centerX;
    }

    public static float GaussianAsymmetric(float centerX = 0.0f, float lowerWidthFactor = 1f, float upperWidthFactor = 1f) {
        float num = Mathf.Sqrt(-2f * Mathf.Log(Value)) * Mathf.Sin(6.283185f * Value);
        if (num <= 0.0)
            return num * lowerWidthFactor + centerX;
        return num * upperWidthFactor + centerX;
    }

    public static int Range(int min, int max) {
        if (max <= min)
            return min;
        return min + Mathf.Abs(Int % (max - min));
    }

    public static int RangeInclusive(int min, int max) {
        if (max <= min)
            return min;
        return Range(min, max + 1);
    }

    public static float Range(float min, float max) {
        if (max <= (double) min)
            return min;
        return Value * (max - min) + min;
    }

    public static bool Chance(float chance) {
        if (chance <= 0.0)
            return false;
        if (chance >= 1.0)
            return true;
        return Value < (double) chance;
    }

    public static bool ChanceSeeded(float chance, int specialSeed) {
        PushState(specialSeed);
        bool flag = Chance(chance);
        PopState();
        return flag;
    }

    public static float ValueSeeded(int specialSeed) {
        PushState(specialSeed);
        float num = Value;
        PopState();
        return num;
    }

    public static float RangeSeeded(float min, float max, int specialSeed) {
        PushState(specialSeed);
        float num = Range(min, max);
        PopState();
        return num;
    }

    public static int RangeSeeded(int min, int max, int specialSeed) {
        PushState(specialSeed);
        int num = Range(min, max);
        PopState();
        return num;
    }

    public static int RangeInclusiveSeeded(int min, int max, int specialSeed) {
        PushState(specialSeed);
        int num = RangeInclusive(min, max);
        PopState();
        return num;
    }

    public static T Element<T>(T a, T b) {
        if (Bool)
            return a;
        return b;
    }

    public static T Element<T>(T a, T b, T c) {
        float num = Value;
        if (num < 0.333330005407333)
            return a;
        if (num < 0.666660010814667)
            return b;
        return c;
    }

    public static T Element<T>(T a, T b, T c, T d) {
        float num = Value;
        if (num < 0.25)
            return a;
        if (num < 0.5)
            return b;
        if (num < 0.75)
            return c;
        return d;
    }

    public static T Element<T>(T a, T b, T c, T d, T e) {
        float num = Value;
        if (num < 0.200000002980232)
            return a;
        if (num < 0.400000005960464)
            return b;
        if (num < 0.600000023841858)
            return c;
        if (num < 0.800000011920929)
            return d;
        return e;
    }

    public static T Element<T>(T a, T b, T c, T d, T e, T f) {
        float num = Value;
        if (num < 0.166659995913506)
            return a;
        if (num < 0.333330005407333)
            return b;
        if (num < 0.5)
            return c;
        if (num < 0.666660010814667)
            return d;
        if (num < 0.833329975605011)
            return e;
        return f;
    }

    public static void PushState() {
        stateStack.Push(StateCompressed);
    }

    public static void PushState(int replacementSeed) {
        PushState();
        Seed = replacementSeed;
    }

    public static void PopState() {
        StateCompressed = stateStack.Pop();
    }
}