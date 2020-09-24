using UnityEngine;

public static class RandUtils {
    public static bool Bool => Random.Range(0, 2) == 0;

    public static bool BoolWeighted(float weight) {
        return Random.Range(0f, 1f) < weight;
    }

    public static T ItemWeighted<T>(T first, T second, float weight) {
        return BoolWeighted(weight) ? first : second;
    }

    /// <summary>
    /// Returns a random item based on weights. Weights are incremental, meaning if `firstWeight` is 0.5, the first item
    /// has a 50% chance to be picked, and if the `secondWeight` is 0.75, the second item has a 25% chance to be picked.
    /// </summary>
    public static T ItemWeighted<T>(T first, T second, T third, float firstWeight, float secondWeight) {
        var num = Random.Range(0f, 1f);
        return num < firstWeight ? first : num < secondWeight ? second : third;
    }
}