using System.Collections.Generic;
using UnityEngine;

public class WeightedRandom {
    private float[] weights;
    private float[] additiveWeights;
    private readonly int count;
    private bool calculatedAdditiveWeights;

    public WeightedRandom(params float[] weights) {
        count = weights.Length;
        this.weights = new float[count];
        for (var i = 0; i < count; i++) {
            this.weights[i] = weights[i];
        }

        calculatedAdditiveWeights = false;
    }

    public void NormalizeWeights() {
        var weightSum = 0f;
        for (var i = 0; i < count; i++) {
            weightSum += weights[i];
        }

        var multiplier = 1f / weightSum;
        for (var i = 0; i < count; i++) {
           weights[i] *= multiplier;
        }
        if(calculatedAdditiveWeights)
            CalculateAdditiveWeights();
    }

    public void CalculateAdditiveWeights() {
        additiveWeights = new float[count];
        additiveWeights[0] = weights[0];
        for (var i = 1; i < count; i++) {
            additiveWeights[i] = additiveWeights[i - 1] + weights[i];
        }

        calculatedAdditiveWeights = true;
    }

    public int Value() {
        float[] array = weights;
        if (calculatedAdditiveWeights) array = additiveWeights;
        var randomValue = Random.Range(0f, 1f);
        for (var i = 0; i < count-1; i++) {
            if (randomValue < array[i]) return i;
        }

        return count - 1;
    }
}