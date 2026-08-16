using System;
using System.Numerics;
using SoftEngine.Math;

namespace SoftEngine.Core
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; } = Vector3.UnitY;
        public float FieldOfView { get; set; } = MathF.PI / 3f; // 60 degrees
        public float AspectRatio { get; set; } = 16f / 9f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000f;

        public Matrix4x4 ViewMatrix => MatrixExtensions.CreateViewMatrix(Position, Target, Up);
        public Matrix4x4 ProjectionMatrix => MatrixExtensions.CreateProjectionMatrix(FieldOfView, AspectRatio, NearPlane, FarPlane);
        public Matrix4x4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;

        public Camera(Vector3 position, Vector3 target)
        {
            Position = position;
            Target = target;
        }

        public Camera(Vector3 position, Vector3 target, Vector3 up) : this(position, target)
        {
            Up = up;
        }

        public void LookAt(Vector3 target)
        {
            Target = target;
        }

        public void Move(Vector3 offset)
        {
            Position += offset;
            Target += offset;
        }

        public void RotateAroundTarget(float yaw, float pitch)
        {
            Vector3 direction = Target - Position;
            float distance = direction.Length();
            direction = Vector3.Normalize(direction);

            float currentYaw = MathF.Atan2(direction.X, direction.Z);
            float currentPitch = MathF.Asin(direction.Y);

            float newYaw = currentYaw + yaw;
            float newPitch = MathHelper.Clamp(currentPitch + pitch, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);

            direction.X = MathF.Sin(newYaw) * MathF.Cos(newPitch);
            direction.Y = MathF.Sin(newPitch);
            direction.Z = MathF.Cos(newYaw) * MathF.Cos(newPitch);

            Position = Target - direction * distance;
        }

        public void Zoom(float factor)
        {
            Vector3 direction = Target - Position;
            float distance = direction.Length();
            distance = MathHelper.Max(0.1f, distance * factor);
            Position = Target - Vector3.Normalize(direction) * distance;
        }
    }
}