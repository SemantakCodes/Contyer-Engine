using System;
using System.Numerics;
using SoftEngine.Math;
using SoftEngine.Core;
using SoftEngine.Textures;

namespace SoftEngine.Rendering
{
    public enum RenderMode
    {
        Wireframe,
        Solid,
        Textured
    }

    public enum ShadingMode
    {
        Flat,
        Gouraud
    }

    public class Renderer
    {
        private readonly Device _device;
        private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 _viewProjectionMatrix = Matrix4x4.Identity;
        
        public RenderMode Mode { get; set; } = RenderMode.Solid;
        public ShadingMode Shading { get; set; } = ShadingMode.Gouraud;
        public bool EnableBackfaceCulling { get; set; } = true;
        public bool EnableDepthBuffer { get; set; } = true;
        public Color4 AmbientLight { get; set; } = new Color4(0.1f, 0.1f, 0.1f);
        public Light[] Lights { get; set; } = Array.Empty<Light>();
        public Texture? ActiveTexture { get; set; }

        public Renderer(Device device)
        {
            _device = device;
        }

        public void SetMatrices(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            _worldMatrix = world;
            _viewMatrix = view;
            _projectionMatrix = projection;
            _viewProjectionMatrix = view * projection;
        }

        public void SetCamera(Camera camera)
        {
            _viewMatrix = camera.ViewMatrix;
            _projectionMatrix = camera.ProjectionMatrix;
            _viewProjectionMatrix = _viewMatrix * _projectionMatrix;
        }

        public void Render(Mesh mesh)
        {
            if (mesh.Vertices == null || mesh.Faces == null)
                return;

            Matrix4x4 world = mesh.WorldMatrix;
            Matrix4x4 worldView = world * _viewMatrix;
            Matrix4x4 worldViewProjection = worldView * _projectionMatrix;

            Vector3[] transformedVertices = new Vector3[mesh.Vertices.Length];
            Vector3[] transformedNormals = new Vector3[mesh.Vertices.Length];
            Color4[] vertexColors = new Color4[mesh.Vertices.Length];

            for (int i = 0; i < mesh.Vertices.Length; i++)
            {
                Vertex vertex = mesh.Vertices[i];
                transformedVertices[i] = ProjectVertex(vertex.Position, worldViewProjection);
                transformedNormals[i] = Vector3.TransformNormal(vertex.Normal, world);
                transformedNormals[i] = Vector3.Normalize(transformedNormals[i]);
                
                if (Shading == ShadingMode.Gouraud)
                {
                    vertexColors[i] = CalculateLighting(vertex.Position, vertex.Normal, world, vertex.Color);
                }
            }

            foreach (Face face in mesh.Faces)
            {
                if (face.A >= mesh.Vertices.Length || face.B >= mesh.Vertices.Length || face.C >= mesh.Vertices.Length)
                    continue;

                Vector3 v1 = transformedVertices[face.A];
                Vector3 v2 = transformedVertices[face.B];
                Vector3 v3 = transformedVertices[face.C];

                if (EnableBackfaceCulling && IsBackface(v1, v2, v3))
                    continue;

                if (Mode == RenderMode.Wireframe)
                {
                    DrawWireframeTriangle(v1, v2, v3, Color4.White);
                }
                else
                {
                    Color4 faceColor = Color4.White;
                    if (Shading == ShadingMode.Flat)
                    {
                        Vector3 normal = CalculateFaceNormal(mesh.Vertices[face.A].Position, 
                            mesh.Vertices[face.B].Position, mesh.Vertices[face.C].Position, world);
                        faceColor = CalculateLighting(mesh.Vertices[face.A].Position, normal, world, Color4.White);
                    }

                    Color4 c1 = Shading == ShadingMode.Gouraud ? vertexColors[face.A] : faceColor;
                    Color4 c2 = Shading == ShadingMode.Gouraud ? vertexColors[face.B] : faceColor;
                    Color4 c3 = Shading == ShadingMode.Gouraud ? vertexColors[face.C] : faceColor;

                    Vector2 uv1 = mesh.Vertices[face.A].TextureCoordinates;
                    Vector2 uv2 = mesh.Vertices[face.B].TextureCoordinates;
                    Vector2 uv3 = mesh.Vertices[face.C].TextureCoordinates;

                    DrawTriangle(v1, v2, v3, c1, c2, c3, uv1, uv2, uv3);
                }
            }
        }

        private Vector3 ProjectVertex(Vector3 vertex, Matrix4x4 worldViewProjection)
        {
            Vector4 projCoord = Vector4.Transform(new Vector4(vertex, 1f), worldViewProjection);

            if (projCoord.W != 0f)
            {
                projCoord.X /= projCoord.W;
                projCoord.Y /= projCoord.W;
                projCoord.Z /= projCoord.W;
            }

            float x = (projCoord.X + 1f) * 0.5f * _device.Width;
            float y = (-projCoord.Y + 1f) * 0.5f * _device.Height;
            float z = projCoord.Z;

            return new Vector3(x, y, z);
        }

        private bool IsBackface(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v1;
            Vector3 normal = Vector3.Cross(edge1, edge2);
            
            return normal.Z > 0;
        }

        private Vector3 CalculateFaceNormal(Vector3 p1, Vector3 p2, Vector3 p3, Matrix4x4 world)
        {
            Vector3 wp1 = Vector3.Transform(p1, world);
            Vector3 wp2 = Vector3.Transform(p2, world);
            Vector3 wp3 = Vector3.Transform(p3, world);
            
            Vector3 edge1 = wp2 - wp1;
            Vector3 edge2 = wp3 - wp1;
            Vector3 normal = Vector3.Cross(edge1, edge2);
            
            return Vector3.Normalize(normal);
        }

        private Color4 CalculateLighting(Vector3 worldPosition, Vector3 worldNormal, Matrix4x4 world, Color4 baseColor)
        {
            Color4 finalColor = AmbientLight * baseColor;

            foreach (Light light in Lights)
            {
                Vector3 lightDir = light.GetDirection(worldPosition);
                if (lightDir == Vector3.Zero)
                    continue;

                float diff = MathHelper.Max(0f, Vector3.Dot(worldNormal, lightDir));
                Color4 lightColor = light.GetColor(worldPosition);
                finalColor += baseColor * lightColor * diff;
            }

            return new Color4(
                MathHelper.Clamp(finalColor.R, 0f, 1f),
                MathHelper.Clamp(finalColor.G, 0f, 1f),
                MathHelper.Clamp(finalColor.B, 0f, 1f),
                baseColor.A
            );
        }

        private void DrawWireframeTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color4 color)
        {
            _device.DrawLine(new Vector2(v1.X, v1.Y), new Vector2(v2.X, v2.Y), color);
            _device.DrawLine(new Vector2(v2.X, v2.Y), new Vector2(v3.X, v3.Y), color);
            _device.DrawLine(new Vector2(v3.X, v3.Y), new Vector2(v1.X, v1.Y), color);
        }

        private void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3, 
            Color4 c1, Color4 c2, Color4 c3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (v1.Y > v2.Y) { Swap(ref v1, ref v2); Swap(ref c1, ref c2); Swap(ref uv1, ref uv2); }
            if (v2.Y > v3.Y) { Swap(ref v2, ref v3); Swap(ref c2, ref c3); Swap(ref uv2, ref uv3); }
            if (v1.Y > v2.Y) { Swap(ref v1, ref v2); Swap(ref c1, ref c2); Swap(ref uv1, ref uv2); }

            float y1 = v1.Y, y2 = v2.Y, y3 = v3.Y;

            if (y2 == y3)
            {
                FillBottomFlatTriangle(v1, v2, v3, c1, c2, c3, uv1, uv2, uv3);
            }
            else if (y1 == y2)
            {
                FillTopFlatTriangle(v1, v2, v3, c1, c2, c3, uv1, uv2, uv3);
            }
            else
            {
                float alpha = (y2 - y1) / (y3 - y1);
                Vector3 v4 = new Vector3(
                    v1.X + (v3.X - v1.X) * alpha,
                    v2.Y,
                    v1.Z + (v3.Z - v1.Z) * alpha
                );
                Color4 c4 = Color4.Lerp(c1, c3, alpha);
                Vector2 uv4 = Vector2.Lerp(uv1, uv3, alpha);

                FillBottomFlatTriangle(v1, v2, v4, c1, c2, c4, uv1, uv2, uv4);
                FillTopFlatTriangle(v2, v4, v3, c2, c4, c3, uv2, uv4, uv3);
            }
        }

        private void FillBottomFlatTriangle(Vector3 v1, Vector3 v2, Vector3 v3,
            Color4 c1, Color4 c2, Color4 c3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            float invSlope1 = (v2.X - v1.X) / (v2.Y - v1.Y);
            float invSlope2 = (v3.X - v1.X) / (v3.Y - v1.Y);
            float invSlopeZ1 = (v2.Z - v1.Z) / (v2.Y - v1.Y);
            float invSlopeZ2 = (v3.Z - v1.Z) / (v3.Y - v1.Y);

            int startY = (int)MathF.Ceiling(v1.Y);
            int endY = (int)MathF.Ceiling(v2.Y);

            for (int y = startY; y <= endY; y++)
            {
                float alpha = (y - v1.Y) / (v2.Y - v1.Y);
                
                float x1 = v1.X + invSlope1 * (y - v1.Y);
                float x2 = v1.X + invSlope2 * (y - v1.Y);
                float z1 = v1.Z + invSlopeZ1 * (y - v1.Y);
                float z2 = v1.Z + invSlopeZ2 * (y - v1.Y);

                Color4 colorLeft = Color4.Lerp(c1, c2, alpha);
                Color4 colorRight = Color4.Lerp(c1, c3, alpha);
                Vector2 uvLeft = Vector2.Lerp(uv1, uv2, alpha);
                Vector2 uvRight = Vector2.Lerp(uv1, uv3, alpha);

                DrawScanline(y, x1, x2, z1, z2, colorLeft, colorRight, uvLeft, uvRight);
            }
        }

        private void FillTopFlatTriangle(Vector3 v1, Vector3 v2, Vector3 v3,
            Color4 c1, Color4 c2, Color4 c3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            float invSlope1 = (v3.X - v1.X) / (v3.Y - v1.Y);
            float invSlope2 = (v3.X - v2.X) / (v3.Y - v2.Y);
            float invSlopeZ1 = (v3.Z - v1.Z) / (v3.Y - v1.Y);
            float invSlopeZ2 = (v3.Z - v2.Z) / (v3.Y - v2.Y);

            int startY = (int)MathF.Ceiling(v1.Y);
            int endY = (int)MathF.Ceiling(v3.Y);

            for (int y = startY; y <= endY; y++)
            {
                float alpha1 = (y - v1.Y) / (v3.Y - v1.Y);
                float alpha2 = (y - v2.Y) / (v3.Y - v2.Y);
                
                float x1 = v1.X + invSlope1 * (y - v1.Y);
                float x2 = v2.X + invSlope2 * (y - v2.Y);
                float z1 = v1.Z + invSlopeZ1 * (y - v1.Y);
                float z2 = v2.Z + invSlopeZ2 * (y - v2.Y);

                Color4 colorLeft = Color4.Lerp(c1, c3, alpha1);
                Color4 colorRight = Color4.Lerp(c2, c3, alpha2);
                Vector2 uvLeft = Vector2.Lerp(uv1, uv3, alpha1);
                Vector2 uvRight = Vector2.Lerp(uv2, uv3, alpha2);

                DrawScanline(y, x1, x2, z1, z2, colorLeft, colorRight, uvLeft, uvRight);
            }
        }

        private void DrawScanline(int y, float x1, float x2, float z1, float z2,
            Color4 c1, Color4 c2, Vector2 uv1, Vector2 uv2)
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
                Swap(ref z1, ref z2);
                Swap(ref c1, ref c2);
                Swap(ref uv1, ref uv2);
            }

            int startX = (int)MathF.Ceiling(x1);
            int endX = (int)MathF.Ceiling(x2);

            for (int x = startX; x <= endX; x++)
            {
                float alpha = (x2 - x1) > 0.0001f ? (x - x1) / (x2 - x1) : 0f;
                float z = z1 + (z2 - z1) * alpha;

                if (!_device.DepthTest(x, y, z))
                    continue;

                Color4 color = Color4.Lerp(c1, c2, alpha);
                Vector2 uv = Vector2.Lerp(uv1, uv2, alpha);

                if (ActiveTexture != null && Mode == RenderMode.Textured)
                {
                    color *= ActiveTexture.Sample(uv.X, uv.Y);
                }

                _device.PutPixel(x, y, color, z);
            }
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}