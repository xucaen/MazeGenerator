using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace MazeGenerator
{
    public class WorldBuilder
    {
        StringBuilder sb = new StringBuilder();

        // Helix configuration constants
        private const float Radius = 10.0f;       // Distance from center
        private const float HeightStep = 25.0f;   // Vertical distance between levels
        private const float RotationStep = 0.5f; // Radians to rotate each level (approx 30 degrees)

        internal void Init()
        {
            sb.AppendLine("[gd_scene format=3]");
            sb.AppendLine();

            sb.AppendLine("[ext_resource type= \"Script\" path = \"res://main.gd\" id = \"main_gd\"]");
            sb.AppendLine("[ext_resource type= \"PackedScene\" path = \"res://player.tscn\" id = \"player\"]");
            sb.AppendLine("[ext_resource type= \"PackedScene\"  path = \"res://world.tscn\" id = \"world\"]");
            sb.AppendLine();
        }

        internal void AddLevels(List<string> levelNames)
        {
            // First loop: Resources
            // We use the level name as the 'id' so we can reference it easily later
            for (int i = 0; i < levelNames.Count; i++)
            {
                string name = levelNames[i];
                sb.AppendLine($"[ext_resource type=\"PackedScene\" path=\"res://{name}.tscn\" id=\"{name}\"]");
            }

            sb.AppendLine();

            // Define the Root Node (Required for a valid scene)
            sb.AppendLine("[node name=\"Main\" type=\"Node3D\"]");
            sb.AppendLine("script = ExtResource(\"main_gd\")");
            sb.AppendLine();

            // Second loop: Node Instantiation with Helix Transform
            for (int i = 0; i < levelNames.Count; i++)
            {
                string name = levelNames[i];

                // --- Helix Calculations ---
                float angle = i * RotationStep;
                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;
                float y = i * HeightStep;

                // Create a rotation matrix (Transform3D) around the Y axis
                // Transform3D format: (basis_x, basis_y, basis_z, origin)
                // A rotation of 'angle' around Y:
                float cosA = MathF.Cos(angle);
                float sinA = MathF.Sin(angle);

                // Row-major representation for Godot's Transform3D string:
                // (xx, xy, xz, yx, yy, yz, zx, zy, zz, ox, oy, oz)
                string transform = $"Transform3D({cosA}, 0, {-sinA}, 0, 1, 0, {sinA}, 0, {cosA}, {x}, {y}, {z})";

                sb.AppendLine($"[node name=\"{name}\" parent=\".\" instance=ExtResource(\"{name}\")]");
                sb.AppendLine($"transform = {transform}");
                sb.AppendLine();
            }
        }

        internal void Finish()
        {
            // Saves to the project root as Main.tscn
            sb.AppendLine("[node name= \"Player\" parent = \".\" instance = ExtResource(\"player\")]");
            sb.AppendLine("[node name= \"World\" parent = \".\" instance = ExtResource(\"world\")]");
            sb.AppendLine();

            try
            {
                File.WriteAllText("../../../Main.tscn", sb.ToString());
                Console.WriteLine("Scene successfully generated at Main.tscn");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }
}