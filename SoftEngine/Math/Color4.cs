using System;

namespace SoftEngine.Math
{
    public struct Color4 : IEquatable<Color4>
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color4(float r, float g, float b, float a = 1.0f)
        {
            R = Math.Clamp(r, 0f, 1f);
            G = Math.Clamp(g, 0f, 1f);
            B = Math.Clamp(b, 0f, 1f);
            A = Math.Clamp(a, 0f, 1f);
        }

        public Color4(byte r, byte g, byte b, byte a = 255)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }

        public static Color4 White => new Color4(1f, 1f, 1f);
        public static Color4 Black => new Color4(0f, 0f, 0f);
        public static Color4 Red => new Color4(1f, 0f, 0f);
        public static Color4 Green => new Color4(0f, 1f, 0f);
        public static Color4 Blue => new Color4(0f, 0f, 1f);
        public static Color4 Yellow => new Color4(1f, 1f, 0f);
        public static Color4 Cyan => new Color4(0f, 1f, 1f);
        public static Color4 Magenta => new Color4(1f, 0f, 1f);

        public uint ToArgb()
        {
            byte a = (byte)(A * 255);
            byte r = (byte)(R * 255);
            byte g = (byte)(G * 255);
            byte b = (byte)(B * 255);
            return (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public static Color4 FromArgb(uint argb)
        {
            byte a = (byte)((argb >> 24) & 0xFF);
            byte r = (byte)((argb >> 16) & 0xFF);
            byte g = (byte)((argb >> 8) & 0xFF);
            byte b = (byte)(argb & 0xFF);
            return new Color4(r, g, b, a);
        }

        public static Color4 operator *(Color4 left, float right)
        {
            return new Color4(left.R * right, left.G * right, left.B * right, left.A);
        }

        public static Color4 operator *(float left, Color4 right)
        {
            return right * left;
        }

        public static Color4 operator *(Color4 left, Color4 right)
        {
            return new Color4(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);
        }

        public static Color4 operator +(Color4 left, Color4 right)
        {
            return new Color4(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
        }

        public static Color4 Lerp(Color4 a, Color4 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Color4(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t
            );
        }

        public bool Equals(Color4 other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public override bool Equals(object obj)
        {
            return obj is Color4 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        public static bool operator ==(Color4 left, Color4 right) => left.Equals(right);
        public static bool operator !=(Color4 left, Color4 right) => !left.Equals(right);
    }
}