namespace HumanGlassWatcher.Character.Determinism
{
    /// <summary>
    /// SplitMix64-based generator with an explicitly owned algorithm. Unlike System.Random,
    /// its output does not change with framework or Unity runtime versions.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private ulong state;

        public DeterministicRandom(int seed)
        {
            state = unchecked((ulong)(long)seed) ^ 0xD1B54A32D192ED03UL;
        }

        public ulong NextUInt64()
        {
            state += 0x9E3779B97F4A7C15UL;
            var value = state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }

        public float NextUnit()
        {
            return (NextUInt64() >> 40) / 16777216f;
        }

        public float Range(float minimum, float maximum)
        {
            return minimum + ((maximum - minimum) * NextUnit());
        }

        public int Range(int minimumInclusive, int maximumExclusive)
        {
            var range = maximumExclusive - minimumInclusive;
            if (range <= 0)
            {
                return minimumInclusive;
            }

            return minimumInclusive + (int)(NextUInt64() % (uint)range);
        }
    }
}
