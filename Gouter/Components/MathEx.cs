namespace Gouter
{
    internal static class MathEx
    {
        public static bool IsWithin(int value, int min, int max)
        {
            return min <= value && value <= max;
        }

        public static int WithIn(int value, int min, int max)
        {
            return min > value ? min : (value > max ? max : value);
        }
    }
}