using System;
using System.Collections.Generic;

namespace HumanGlassWatcher.Character
{
    internal static class CharacterMath
    {
        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }

        public static float Clamp(float value, float minimum, float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }

        public static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 64)
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (!char.IsLetterOrDigit(character) && character != '_' && character != '-')
                {
                    return false;
                }
            }

            return true;
        }

        public static string[] CopyStrings(IEnumerable<string> values)
        {
            if (values == null)
            {
                return Array.Empty<string>();
            }

            var copy = new List<string>();
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    copy.Add(value);
                }
            }

            return copy.ToArray();
        }
    }
}
