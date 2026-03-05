using System;
using UnityEngine;

namespace TwinGraph.Runtime.Graph
{
    [Serializable]
    public readonly struct Variant
    {
        public enum VariantType
        {
            None,
            Int,
            Float,
            Bool,
            String,
            Vector3,
        }

        private readonly int intValue;
        private readonly float floatValue;
        private readonly bool boolValue;
        private readonly string stringValue;
        private readonly Vector3 vector3Value;

        private Variant(
            VariantType type,
            int intValue,
            float floatValue,
            bool boolValue,
            string stringValue,
            Vector3 vector3Value
        )
        {
            Type = type;
            this.intValue = intValue;
            this.floatValue = floatValue;
            this.boolValue = boolValue;
            this.stringValue = stringValue;
            this.vector3Value = vector3Value;
        }

        public VariantType Type { get; }

        public static Variant FromInt(int value)
        {
            return new Variant(VariantType.Int, value, default, default, string.Empty, default);
        }

        public static Variant FromFloat(float value)
        {
            return new Variant(VariantType.Float, default, value, default, string.Empty, default);
        }

        public static Variant FromBool(bool value)
        {
            return new Variant(VariantType.Bool, default, default, value, string.Empty, default);
        }

        public static Variant FromString(string value)
        {
            return new Variant(
                VariantType.String,
                default,
                default,
                default,
                value ?? string.Empty,
                default
            );
        }

        public static Variant FromVector3(Vector3 value)
        {
            return new Variant(VariantType.Vector3, default, default, default, string.Empty, value);
        }

        public int AsInt(int fallback = default)
        {
            return Type == VariantType.Int ? intValue : fallback;
        }

        public float AsFloat(float fallback = default)
        {
            return Type == VariantType.Float ? floatValue : fallback;
        }

        public bool AsBool(bool fallback = default)
        {
            return Type == VariantType.Bool ? boolValue : fallback;
        }

        public string AsString(string fallback = "")
        {
            return Type == VariantType.String ? stringValue : fallback;
        }

        public Vector3 AsVector3(Vector3 fallback = default)
        {
            return Type == VariantType.Vector3 ? vector3Value : fallback;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case VariantType.Int:
                    return intValue.ToString();
                case VariantType.Float:
                    return floatValue.ToString("G");
                case VariantType.Bool:
                    return boolValue.ToString();
                case VariantType.String:
                    return stringValue ?? string.Empty;
                case VariantType.Vector3:
                    return vector3Value.ToString();
                default:
                    return "None";
            }
        }
    }
}
