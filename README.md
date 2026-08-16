# CONTYER ENGINE

A CPU-based 3D rendering engine built entirely from scratch in C#.

This project does not use any hardware-accelerated graphics libraries (like OpenGL, DirectX, Vulkan, or Unity). Every vertex transformation, projection, and pixel rendering is calculated mathematically on the CPU and pushed to a 2D pixel buffer.

*Inspired by David Rousset's 2013 tutorial series on writing a 3D soft engine.*

## Features

* **Custom 3D Mathematics:** `Vector2`, `Vector3`, and `Matrix` (4x4) operations.
* **Camera System:** Position, LookAt target, and View Matrix generation.
* **Perspective Projection:** Transforms 3D world coordinates into 2D screen space.
* **Rasterization:** Scanline triangle filling algorithm.
* **Depth Buffering (Z-Buffer):** Ensures correct object overlap and occlusion.
* **Back-Face Culling:** Optimizes rendering by ignoring geometry facing away from the camera.
* **Lighting:** Flat and Gouraud shading using face and vertex normals.
* **Texture Mapping:** UV coordinate interpolation across rasterized triangles.

## Requirements

* .NET SDK (8.0 or later recommended)
* Visual Studio 2022 / VS Code / Rider

> **Note:** The project uses WPF (`WriteableBitmap`) or Windows Forms (`Bitmap`) strictly for displaying the final 2D pixel array.

