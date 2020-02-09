using System;

namespace Utils
{
    public static class FloatExtensions
    {
        public static float LimitToRange(
            this float value, float inclusiveMinimum, float inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }

        public static float ToRadians(this float deg) => deg * MathF.PI / 180.0f;
    }
}