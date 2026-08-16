using System;
using System.Numerics;
using SoftEngine.Math;

namespace SoftEngine.Core
{
    public enum LightType
    {
        Directional,
        Point,
        Ambient
    }

    public class Light
    {
        public LightType Type { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Color4 Color { get; set; }
        public float Intensity { get; set; } = 1f;
        public float Range { get; set; } = 100f;
        public float ConstantAttenuation { get; set; } = 1f;
        public float LinearAttenuation { get; set; } = 0f;
        public float QuadraticAttenuation { get; set; } = 0f;

        public Light(LightType type, Vector3 positionOrDirection, Color4 color)
        {
            Type = type;
            Color = color;
            
            if (type == LightType.Directional)
            {
                Direction = Vector3.Normalize(positionOrDirection);
                Position = Vector3.Zero;
            }
            else
            {
                Position = positionOrDirection;
                Direction = Vector3.Zero;
            }
        }

        public static Light CreateDirectional(Vector3 direction, Color4 color, float intensity = 1f)
        {
            var light = new Light(LightType.Directional, direction, color);
            light.Intensity = intensity;
            return light;
        }

        public static Light CreatePoint(Vector3 position, Color4 color, float intensity = 1f, float range = 100f)
        {
            var light = new Light(LightType.Point, position, color);
            light.Intensity = intensity;
            light.Range = range;
            return light;
        }

        public static Light CreateAmbient(Color4 color, float intensity = 0.1f)
        {
            var light = new Light(LightType.Ambient, Vector3.Zero, color);
            light.Intensity = intensity;
            return light;
        }

        public Vector3 GetDirection(Vector3 surfacePosition)
        {
            if (Type == LightType.Directional)
                return -Direction;
            else if (Type == LightType.Point)
                return Vector3.Normalize(Position - surfacePosition);
            return Vector3.Zero;
        }

        public float GetAttenuation(Vector3 surfacePosition)
        {
            if (Type == LightType.Directional || Type == LightType.Ambient)
                return 1f;

            float distance = Vector3.Distance(Position, surfacePosition);
            if (distance > Range)
                return 0f;

            float attenuation = 1f / (ConstantAttenuation + LinearAttenuation * distance + QuadraticAttenuation * distance * distance);
            return Math.Clamp(attenuation, 0f, 1f);
        }

        public Color4 GetColor(Vector3 surfacePosition)
        {
            float attenuation = GetAttenuation(surfacePosition);
            return Color * (Intensity * attenuation);
        }
    }
}