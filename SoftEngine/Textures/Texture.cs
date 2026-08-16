using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using SoftEngine.Math;

namespace SoftEngine.Textures
{
    public class Texture : IDisposable
    {
        private readonly uint[] _pixels;
        private readonly int _width;
        private readonly int _height;
        private bool _disposed;

        public int Width => _width;
        public int Height => _height;

        public Texture(int width, int height)
        {
            _width = width;
            _height = height;
            _pixels = new uint[width * height];
        }

        public Texture(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Texture file not found: {filePath}");

            using var bitmap = new System.Drawing.Bitmap(filePath);
            _width = bitmap.Width;
            _height = bitmap.Height;
            _pixels = new uint[_width * _height];

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, _width, _height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                int byteCount = _width * _height * 4;
                byte[] byteBuffer = new byte[byteCount];
                Marshal.Copy(bitmapData.Scan0, byteBuffer, 0, byteCount);
                Buffer.BlockCopy(byteBuffer, 0, _pixels, 0, byteCount);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static Texture CreateCheckerboard(int width, int height, int squareSize = 16)
        {
            var texture = new Texture(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int squareX = x / squareSize;
                    int squareY = y / squareSize;
                    bool isLight = (squareX + squareY) % 2 == 0;
                    texture._pixels[y * width + x] = isLight 
                        ? new Color4(1f, 1f, 1f).ToArgb() 
                        : new Color4(0f, 0f, 0f).ToArgb();
                }
            }
            return texture;
        }

        public static Texture CreateGradient(int width, int height)
        {
            var texture = new Texture(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / (width - 1);
                    float v = (float)y / (height - 1);
                    var color = new Color4(u, v, 0.5f);
                    texture._pixels[y * width + x] = color.ToArgb();
                }
            }
            return texture;
        }

        public Color4 Sample(float u, float v)
        {
            u = u - MathF.Floor(u);
            v = v - MathF.Floor(v);

            float px = u * (_width - 1);
            float py = v * (_height - 1);

            int x0 = (int)MathF.Floor(px);
            int y0 = (int)MathF.Floor(py);
            int x1 = System.Math.Min(x0 + 1, _width - 1);
            int y1 = System.Math.Min(y0 + 1, _height - 1);

            float fx = px - x0;
            float fy = py - y0;

            uint c00 = _pixels[y0 * _width + x0];
            uint c10 = _pixels[y0 * _width + x1];
            uint c01 = _pixels[y1 * _width + x0];
            uint c11 = _pixels[y1 * _width + x1];

            Color4 col00 = Color4.FromArgb(c00);
            Color4 col10 = Color4.FromArgb(c10);
            Color4 col01 = Color4.FromArgb(c01);
            Color4 col11 = Color4.FromArgb(c11);

            Color4 c0 = Color4.Lerp(col00, col10, fx);
            Color4 c1 = Color4.Lerp(col01, col11, fx);
            Color4 result = Color4.Lerp(c0, c1, fy);

            return result;
        }

        public Color4 SampleNearest(float u, float v)
        {
            u = u - MathF.Floor(u);
            v = v - MathF.Floor(v);

            int x = (int)(u * _width) % _width;
            int y = (int)(v * _height) % _height;

            if (x < 0) x += _width;
            if (y < 0) y += _height;

            return Color4.FromArgb(_pixels[y * _width + x]);
        }

        public void SetPixel(int x, int y, Color4 color)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                _pixels[y * _width + x] = color.ToArgb();
            }
        }

        public Color4 GetPixel(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                return Color4.FromArgb(_pixels[y * _width + x]);
            }
            return Color4.Black;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}