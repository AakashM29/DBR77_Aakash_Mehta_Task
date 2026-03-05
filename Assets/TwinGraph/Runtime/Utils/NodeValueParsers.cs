using System;
using System.Globalization;
using UnityEngine;

namespace TwinGraph.Runtime.Utils
{
    internal static class NodeValueParsers
    {
        public static Vector3 ParseVector3(string raw, Vector3 fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            var parts = raw.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                return fallback;
            }

            if (
                float.TryParse(
                    parts[0],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var x
                )
                && float.TryParse(
                    parts[1],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var y
                )
                && float.TryParse(
                    parts[2],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var z
                )
            )
            {
                return new Vector3(x, y, z);
            }

            return fallback;
        }

        public static float ParseFloat(string raw, float fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            if (
                float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            )
            {
                return value;
            }

            return fallback;
        }

        public static bool ParseBool(string raw, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            if (bool.TryParse(raw, out var boolean))
            {
                return boolean;
            }

            switch (raw.Trim().ToLowerInvariant())
            {
                case "1":
                case "y":
                case "yes":
                case "on":
                    return true;
                case "0":
                case "n":
                case "no":
                case "off":
                    return false;
                default:
                    return fallback;
            }
        }
    }
}
