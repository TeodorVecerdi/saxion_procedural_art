using System;

/// <summary>
/// Took this from the source code of an amazing game called RimWorld.
/// I am not proud of this but for the scope of this project I didn't have time to implement something like this.
/// The Unity Random is really bad.
/// </summary>
public class RandomNumberGenerator {
    public uint seed = (uint) DateTime.Now.GetHashCode();

    public int GetInt(uint iterations) {
        return (int) GetHash((int) iterations);
    }

    public float GetFloat(uint iterations) {
        return (float) ((GetInt(iterations) - (double) int.MinValue) / uint.MaxValue);
    }

    private uint GetHash(int buffer) {
        uint num1 = Rotate(seed + 374761393U + 4U + (uint) (buffer * -1028477379), 17) * 668265263U;
        uint num2 = (num1 ^ num1 >> 15) * 2246822519U;
        uint num3 = (num2 ^ num2 >> 13) * 3266489917U;
        return num3 ^ num3 >> 16;
    }

    private static uint Rotate(uint value, int count) {
        return value << count | value >> 32 - count;
    }
}