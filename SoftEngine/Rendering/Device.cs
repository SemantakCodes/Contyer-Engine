using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SoftEngine.Math;
using SoftEngine.Core;

namespace SoftEngine.Rendering
{
    public class Device : IDisposable
    {
        private readonly int _width;
        private readonly int _height;
        private readonly uint[] _backBuffer;
        private readonly float[] _depthBuffer;
        private readonly byte[] _colorBuffer;
        private GCHandle _backBufferHandle;
        private GCHandle _depthBufferHandle;
        private bool _disposed;

        public int Width => _width;
        public int Height => _height;
        public uint[] BackBuffer => _backBuffer;
        public float[] DepthBuffer => _depthBuffer;

        public Device(int width, int height)
        {
            _width = width;
            _height = height;
            _backBuffer = new uint[width * height];
            _depthBuffer = new float[width * height];
            _colorBuffer = new byte[width * height * 4];
            
            _backBufferHandle = GCHandle.Alloc(_backBuffer, GCHandleType.Pinned);
            _depthBufferHandle = GCHandle.Alloc(_depthBuffer, GCHandleType.Pinned);
            
            Clear();
        }

        public void Clear(Color4 color = default)
        {
            uint clearColor = (color == default) ? Color4.Black.ToArgb() : color.ToArgb();
            Array.Fill(_backBuffer, clearColor);
            Array.Fill(_depthBuffer, float.MaxValue);
        }

        public void ClearDepth()
        {
            Array.Fill(_depthBuffer, float.MaxValue);
        }

        public void Present(IntPtr hdc)
        {
        }

        public void PutPixel(int x, int y, Color4 color, float depth = float.MaxValue)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return;

            int index = y * _width + x;
            
            if (depth < _depthBuffer[index])
            {
                _depthBuffer[index] = depth;
                _backBuffer[index] = color.ToArgb();
            }
        }

        public void PutPixel(int x, int y, uint color, float depth = float.MaxValue)
        {
            if (x < 0 || x >= _width || y >= _height)
                return;

            int index = y * _width + x;
            
            if (depth < _depthBuffer[index])
            {
                _depthBuffer[index] = depth;
                _backBuffer[index] = color;
            }
        }

        public bool DepthTest(int x, int y, float depth)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return false;

            int index = y * _width + x;
            return depth < _depthBuffer[index];
        }

        public void DrawPoint(Vector3 point, Color4 color)
        {
            int x = (int)MathF.Round(point.X);
            int y = (int)MathF.Round(point.Y);
            PutPixel(x, y, color, point.Z);
        }

        public void DrawLine(Vector2 p1, Vector2 p2, Color4 color)
        {
            DrawLine((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, color, 0f, 0f);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Color4 color, float z1 = 0f, float z2 = 0f)
        {
            int dx = System.Math.Abs(x2 - x1);
            int dy = System.Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2;
            int e2;

            float z = z1;
            float zStep = (z2 - z1) / System.Math.Max(dx, dy);

            while (true)
            {
                PutPixel(x1, y1, color, z);
                
                if (x1 == x2 && y1 == y2) break;
                
                e2 = err;
                if (e2 > -dx) { err -= dy; x1 += sx; z += zStep; }
                if (e2 < dy) { err += dx; y1 += sy; z += zStep; }
            }
        }

        public Vector3 Project(Vector3 coord, Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            Vector3 worldCoord = Vector3.Transform(coord, world);
            Vector3 viewCoord = Vector3.Transform(worldCoord, view);
            Vector4 projCoord = Vector4.Transform(new Vector4(viewCoord, 1f), projection);

            if (projCoord.W != 0f)
            {
                projCoord.X /= projCoord.W;
                projCoord.Y /= projCoord.W;
                projCoord.Z /= projCoord.W;
            }

            float x = (projCoord.X + 1f) * 0.5f * _width;
            float y = (-projCoord.Y + 1f) * 0.5f * _height;
            float z = projCoord.Z;

            return new Vector3(x, y, z);
        }

        public Vector3 Project(Vector3 coord, Matrix4x4 viewProjection)
        {
            Vector4 projCoord = Vector4.Transform(new Vector4(coord, 1f), viewProjection);

            if (projCoord.W != 0f)
            {
                projCoord.X /= projCoord.W;
                projCoord.Y /= projCoord.W;
                projCoord.Z /= projCoord.W;
            }

            float x = (projCoord.X + 1f) * 0.5f * _width;
            float y = (-projCoord.Y + 1f) * 0.5f * _height;
            float z = projCoord.Z;

            return new Vector3(x, y, z);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_backBufferHandle.IsAllocated)
                    _backBufferHandle.Free();
                if (_depthBufferHandle.IsAllocated)
                    _depthBufferHandle.Free();
                _disposed = true;
            }
        }
    }
}