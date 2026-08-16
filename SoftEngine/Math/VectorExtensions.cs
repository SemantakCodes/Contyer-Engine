using System;
using System.Numerics;

namespace SoftEngine.Math
{
    public static class VectorExtensions
    {
        public static Vector3 ToVector3(this Vector2 v, float z = 0f)
        {
            return new Vector3(v.X, v.Y, z);
        }

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector4 ToVector4(this Vector3 v, float w = 1f)
        {
            return new Vector4(v.X, v.Y, v.Z, w);
        }

        public static Vector3 ToVector3(this Vector4 v)
        {
            if (v.W != 0f)
                return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
            return new Vector3(v.X, v.Y, v.Z);
        }
    }

    public static class MatrixExtensions
    {
        public static Matrix4x4 CreateTranslation(Vector3 position)
        {
            return Matrix4x4.CreateTranslation(position);
        }

        public static Matrix4x4 CreateRotationX(float radians)
        {
            return Matrix4x4.CreateRotationX(radians);
        }

        public static Matrix4x4 CreateRotationY(float radians)
        {
            return Matrix4x4.CreateRotationY(radians);
        }

        public static Matrix4x4 CreateRotationZ(float radians)
        {
            return Matrix4x4.CreateRotationZ(radians);
        }

        public static Matrix4x4 CreateRotation(Vector3 rotation)
        {
            return Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        public static Matrix4x4 CreateScale(Vector3 scale)
        {
            return Matrix4x4.CreateScale(scale);
        }

        public static Matrix4x4 CreateWorldMatrix(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            return CreateScale(scale) * CreateRotation(rotation) * CreateTranslation(position);
        }

        public static Matrix4x4 CreateViewMatrix(Vector3 cameraPosition, Vector3 target, Vector3 up)
        {
            return Matrix4x4.CreateLookAt(cameraPosition, target, up);
        }

        public static Matrix4x4 CreateProjectionMatrix(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }

        public static Vector3 TransformCoordinate(this Vector3 vector, Matrix4x4 matrix)
        {
            return Vector3.Transform(vector, matrix);
        }

        public static Vector3 TransformNormal(this Vector3 normal, Matrix4x4 matrix)
        {
            return Vector3.TransformNormal(normal, matrix);
        }
    }
}