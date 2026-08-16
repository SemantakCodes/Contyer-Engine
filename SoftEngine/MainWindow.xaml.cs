using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SoftEngine.Core;
using SoftEngine.Rendering;
using SoftEngine.Textures;
using SoftEngine.Math;

namespace SoftEngine
{
    public partial class MainWindow : Window
    {
        private Device _device;
        private Renderer _renderer;
        private Camera _camera;
        private Mesh _mesh;
        private Texture _texture;
        private WriteableBitmap _writeableBitmap;
        private Stopwatch _stopwatch;
        private long _lastFrameTime;
        private int _frameCount;
        private double _fps;
        
        private bool _moveForward, _moveBackward, _moveLeft, _moveRight, _moveUp, _moveDown;
        private bool _rotateLeft, _rotateRight, _rotateUp, _rotateDown;
        private bool _zoomIn, _zoomOut;

        public MainWindow()
        {
            InitializeComponent();
            InitializeEngine();
            CompositionTarget.Rendering += OnRender;
            _stopwatch = Stopwatch.StartNew();
            _lastFrameTime = _stopwatch.ElapsedMilliseconds;
        }

        private void InitializeEngine()
        {
            int width = 1280;
            int height = 720;

            _device = new Device(width, height);
            _renderer = new Renderer(_device);
            
            _camera = new Camera(
                new Vector3(0, 0, -5),
                new Vector3(0, 0, 0),
                Vector3.UnitY
            );
            _camera.AspectRatio = (float)width / height;

            _mesh = Mesh.CreateCube("Cube", 1.5f);
            _mesh.Position = Vector3.Zero;
            _mesh.Rotation = Vector3.Zero;

            _texture = Texture.CreateCheckerboard(256, 256, 32);

            _renderer.Lights = new Light[]
            {
                Light.CreateDirectional(new Vector3(1, -1, 1), Color4.White, 1.0f),
                Light.CreatePoint(new Vector3(3, 3, -3), new Color4(1f, 0.8f, 0.6f), 0.5f, 20f),
                Light.CreateAmbient(new Color4(0.15f, 0.15f, 0.2f), 0.3f)
            };
            _renderer.AmbientLight = new Color4(0.1f, 0.1f, 0.15f);
            _renderer.Mode = RenderMode.Solid;
            _renderer.Shading = ShadingMode.Gouraud;
            _renderer.EnableBackfaceCulling = true;
            _renderer.EnableDepthBuffer = true;

            _writeableBitmap = new WriteableBitmap(
                width, height, 96, 96, PixelFormats.Bgra32, null);
            
            RenderImage.Source = _writeableBitmap;
            RenderImage.Width = width;
            RenderImage.Height = height;
        }

        private void OnRender(object sender, EventArgs e)
        {
            long currentTime = _stopwatch.ElapsedMilliseconds;
            float deltaTime = (currentTime - _lastFrameTime) / 1000f;
            _lastFrameTime = currentTime;

            UpdateCamera(deltaTime);
            RenderFrame();
            UpdateUI();
        }

        private void UpdateCamera(float deltaTime)
        {
            float moveSpeed = 3f * deltaTime;
            float rotateSpeed = 2f * deltaTime;
            float zoomSpeed = 2f * deltaTime;

            Vector3 forward = Vector3.Normalize(_camera.Target - _camera.Position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, _camera.Up));
            Vector3 up = _camera.Up;

            Vector3 moveVector = Vector3.Zero;
            
            if (_moveForward) moveVector += forward;
            if (_moveBackward) moveVector -= forward;
            if (_moveLeft) moveVector -= right;
            if (_moveRight) moveVector += right;
            if (_moveUp) moveVector += up;
            if (_moveDown) moveVector -= up;

            if (moveVector != Vector3.Zero)
            {
                _camera.Move(Vector3.Normalize(moveVector) * moveSpeed);
            }

            if (_rotateLeft) _camera.RotateAroundTarget(rotateSpeed, 0);
            if (_rotateRight) _camera.RotateAroundTarget(-rotateSpeed, 0);
            if (_rotateUp) _camera.RotateAroundTarget(0, rotateSpeed);
            if (_rotateDown) _camera.RotateAroundTarget(0, -rotateSpeed);

            if (_zoomIn) _camera.Zoom(1f - zoomSpeed);
            if (_zoomOut) _camera.Zoom(1f + zoomSpeed);

            var rot = _mesh.Rotation;
            rot.Y += deltaTime * 0.5f;
            rot.X += deltaTime * 0.3f;
            _mesh.Rotation = rot;
        }

        private void RenderFrame()
        {
            _device.Clear(new Color4(0.1f, 0.1f, 0.15f));
            _device.ClearDepth();

            _renderer.SetCamera(_camera);
            _renderer.ActiveTexture = _texture;
            _renderer.Render(_mesh);

            UpdateBitmap();
        }

        private unsafe void UpdateBitmap()
        {
            _writeableBitmap.Lock();
            try
            {
                int stride = _writeableBitmap.BackBufferStride;
                byte* ptr = (byte*)_writeableBitmap.BackBuffer.ToPointer();
                uint[] buffer = _device.BackBuffer;

                for (int y = 0; y < _device.Height; y++)
                {
                    byte* rowPtr = ptr + y * stride;
                    uint* pixelPtr = (uint*)rowPtr;
                    
                    int bufferOffset = y * _device.Width;
                    for (int x = 0; x < _device.Width; x++)
                    {
                        pixelPtr[x] = buffer[bufferOffset + x];
                    }
                }
                
                _writeableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, _device.Width, _device.Height));
            }
            finally
            {
                _writeableBitmap.Unlock();
            }
        }

        private void UpdateUI()
        {
            _frameCount++;
            long currentTime = _stopwatch.ElapsedMilliseconds;
            
            if (currentTime - _lastFrameTime >= 1000)
            {
                _fps = _frameCount * 1000.0 / (currentTime - _lastFrameTime);
                _frameCount = 0;
                _lastFrameTime = currentTime;
            }

            InfoText.Text = $"FPS: {_fps:F1} | Mode: {_renderer.Mode} | Shading: {_renderer.Shading} | " +
                           $"Culling: {(_renderer.EnableBackfaceCulling ? "On" : "Off")} | " +
                           $"Cam: ({_camera.Position.X:F1}, {_camera.Position.Y:F1}, {_camera.Position.Z:F1}) | " +
                           $"Mesh Rot: ({_mesh.Rotation.X:F1}, {_mesh.Rotation.Y:F1}, {_mesh.Rotation.Z:F1})";
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.W: _moveForward = true; break;
                case System.Windows.Input.Key.S: _moveBackward = true; break;
                case System.Windows.Input.Key.A: _moveLeft = true; break;
                case System.Windows.Input.Key.D: _moveRight = true; break;
                case System.Windows.Input.Key.Space: _moveUp = true; break;
                case System.Windows.Input.Key.LeftShift: _moveDown = true; break;
                case System.Windows.Input.Key.Left: _rotateLeft = true; break;
                case System.Windows.Input.Key.Right: _rotateRight = true; break;
                case System.Windows.Input.Key.Up: _rotateUp = true; break;
                case System.Windows.Input.Key.Down: _rotateDown = true; break;
                case System.Windows.Input.Key.Q: _zoomIn = true; break;
                case System.Windows.Input.Key.E: _zoomOut = true; break;
                
                case System.Windows.Input.Key.D1: _renderer.Mode = RenderMode.Wireframe; break;
                case System.Windows.Input.Key.D2: _renderer.Mode = RenderMode.Solid; break;
                case System.Windows.Input.Key.D3: _renderer.Mode = RenderMode.Textured; break;
                
                case System.Windows.Input.Key.D4: _renderer.Shading = ShadingMode.Flat; break;
                case System.Windows.Input.Key.D5: _renderer.Shading = ShadingMode.Gouraud; break;
                
                case System.Windows.Input.Key.C: _renderer.EnableBackfaceCulling = !_renderer.EnableBackfaceCulling; break;
                case System.Windows.Input.Key.Z: _renderer.EnableDepthBuffer = !_renderer.EnableDepthBuffer; break;
                
                case System.Windows.Input.Key.R:
                    _mesh.Rotation = Vector3.Zero;
                    _camera.Position = new Vector3(0, 0, -5);
                    _camera.Target = Vector3.Zero;
                    break;
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.W: _moveForward = false; break;
                case System.Windows.Input.Key.S: _moveBackward = false; break;
                case System.Windows.Input.Key.A: _moveLeft = false; break;
                case System.Windows.Input.Key.D: _moveRight = false; break;
                case System.Windows.Input.Key.Space: _moveUp = false; break;
                case System.Windows.Input.Key.LeftShift: _moveDown = false; break;
                case System.Windows.Input.Key.Left: _rotateLeft = false; break;
                case System.Windows.Input.Key.Right: _rotateRight = false; break;
                case System.Windows.Input.Key.Up: _rotateUp = false; break;
                case System.Windows.Input.Key.Down: _rotateDown = false; break;
                case System.Windows.Input.Key.Q: _zoomIn = false; break;
                case System.Windows.Input.Key.E: _zoomOut = false; break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            CompositionTarget.Rendering -= OnRender;
            _device?.Dispose();
            _texture?.Dispose();
            base.OnClosed(e);
        }
    }
}