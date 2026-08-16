using System;
using System.Numerics;

namespace SoftEngine.Core
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;
        public Color4 Color;

        public Vertex(Vector3 position, Vector3 normal = default, Vector2 texCoords = default, Color4 color = default)
        {
            Position = position;
            Normal = normal == default ? Vector3.Zero : normal;
            TextureCoordinates = texCoords;
            Color = color == default ? Color4.White : color;
        }
    }

    public struct Face
    {
        public int A;
        public int B;
        public int C;

        public Face(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    public class Mesh
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4x4 WorldMatrix => MatrixExtensions.CreateWorldMatrix(Position, Rotation, Scale);

        public Mesh(string name, Vertex[] vertices, Face[] faces)
        {
            Name = name;
            Vertices = vertices;
            Faces = faces;
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
        }

        public static Mesh CreateCube(string name = "Cube", float size = 1f)
        {
            float half = size / 2f;
            var vertices = new Vertex[]
            {
                // Front face (z = +half)
                new Vertex(new Vector3(-half, -half, half), new Vector3(0, 0, 1), new Vector2(0, 1)),
                new Vertex(new Vector3(half, -half, half), new Vector3(0, 0, 1), new Vector2(1, 1)),
                new Vertex(new Vector3(half, half, half), new Vector3(0, 0, 1), new Vector2(1, 0)),
                new Vertex(new Vector3(-half, half, half), new Vector3(0, 0, 1), new Vector2(0, 0)),
                // Back face (z = -half)
                new Vertex(new Vector3(half, -half, -half), new Vector3(0, 0, -1), new Vector2(0, 1)),
                new Vertex(new Vector3(-half, -half, -half), new Vector3(0, 0, -1), new Vector2(1, 1)),
                new Vertex(new Vector3(-half, half, -half), new Vector3(0, 0, -1), new Vector2(1, 0)),
                new Vertex(new Vector3(half, half, -half), new Vector3(0, 0, -1), new Vector2(0, 0)),
                // Left face (x = -half)
                new Vertex(new Vector3(-half, -half, -half), new Vector3(-1, 0, 0), new Vector2(0, 1)),
                new Vertex(new Vector3(-half, -half, half), new Vector3(-1, 0, 0), new Vector2(1, 1)),
                new Vertex(new Vector3(-half, half, half), new Vector3(-1, 0, 0), new Vector2(1, 0)),
                new Vertex(new Vector3(-half, half, -half), new Vector3(-1, 0, 0), new Vector2(0, 0)),
                // Right face (x = +half)
                new Vertex(new Vector3(half, -half, half), new Vector3(1, 0, 0), new Vector2(0, 1)),
                new Vertex(new Vector3(half, -half, -half), new Vector3(1, 0, 0), new Vector2(1, 1)),
                new Vertex(new Vector3(half, half, -half), new Vector3(1, 0, 0), new Vector2(1, 0)),
                new Vertex(new Vector3(half, half, half), new Vector3(1, 0, 0), new Vector2(0, 0)),
                // Top face (y = +half)
                new Vertex(new Vector3(-half, half, half), new Vector3(0, 1, 0), new Vector2(0, 1)),
                new Vertex(new Vector3(half, half, half), new Vector3(0, 1, 0), new Vector2(1, 1)),
                new Vertex(new Vector3(half, half, -half), new Vector3(0, 1, 0), new Vector2(1, 0)),
                new Vertex(new Vector3(-half, half, -half), new Vector3(0, 1, 0), new Vector2(0, 0)),
                // Bottom face (y = -half)
                new Vertex(new Vector3(-half, -half, -half), new Vector3(0, -1, 0), new Vector2(0, 1)),
                new Vertex(new Vector3(half, -half, -half), new Vector3(0, -1, 0), new Vector2(1, 1)),
                new Vertex(new Vector3(half, -half, half), new Vector3(0, -1, 0), new Vector2(1, 0)),
                new Vertex(new Vector3(-half, -half, half), new Vector3(0, -1, 0), new Vector2(0, 0)),
            };

            var faces = new Face[]
            {
                // Front
                new Face(0, 1, 2), new Face(0, 2, 3),
                // Back
                new Face(4, 5, 6), new Face(4, 6, 7),
                // Left
                new Face(8, 9, 10), new Face(8, 10, 11),
                // Right
                new Face(12, 13, 14), new Face(12, 14, 15),
                // Top
                new Face(16, 17, 18), new Face(16, 18, 19),
                // Bottom
                new Face(20, 21, 22), new Face(20, 22, 23),
            };

            return new Mesh(name, vertices, faces);
        }

        public static Mesh CreateTriangle(string name = "Triangle", float size = 1f)
        {
            float half = size / 2f;
            var vertices = new Vertex[]
            {
                new Vertex(new Vector3(0, half, 0), new Vector3(0, 0, 1), new Vector2(0.5f, 0)),
                new Vertex(new Vector3(-half, -half, 0), new Vector3(0, 0, 1), new Vector2(0, 1)),
                new Vertex(new Vector3(half, -half, 0), new Vector3(0, 0, 1), new Vector2(1, 1)),
            };

            var faces = new Face[]
            {
                new Face(0, 1, 2),
            };

            return new Mesh(name, vertices, faces);
        }
    }
}