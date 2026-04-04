using Raylib_cs;
using Color = Raylib_cs.Color;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace MazeGenerator
{
    public class MazeCompiler
    {
        // Simple DTO to prevent "IsEmpty" from being serialized
        public record GridPoint(int X, int Y);

        private HashSet<MazeMetaData> Walls = new HashSet<MazeMetaData>();
        private HashSet<MazeMetaData> Rooms = new HashSet<MazeMetaData>();


        /// <summary>
        /// Loads maze data from a JSON file.
        /// </summary>
        public void LoadFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Maze file not found.", filePath);

            string jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Deserialize into our clean DTO format
            var data = JsonSerializer.Deserialize<MazeDataDto>(jsonString, options);

            Walls.Clear();
            Rooms.Clear();

            if (data?.Walls != null) foreach (var p in data.Walls) Walls.Add(p);
            if (data?.Rooms != null) foreach (var p in data.Rooms) Rooms.Add(p);
        }

        /// <summary>
        /// Runs a Raylib preview window of the maze.
        /// </summary>
        public void RunPreview()
        {
            Raylib.InitWindow(1280, 720, "Maze Compiler Preview");
            Raylib.SetTargetFPS(60);

            Camera3D camera = new Camera3D
            {
                Position = new Vector3(5, 10, 15),
                Target = new Vector3(1, 0, 5),
                Up = new Vector3(0, 1, 0),
                FovY = 45f,
                Projection = CameraProjection.Perspective
            };

            while (!Raylib.WindowShouldClose())
            {
                Raylib.UpdateCamera(ref camera, CameraMode.Free);
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.BeginMode3D(camera);

                foreach (var wall in Walls)
                {
                    Raylib.DrawCube(new Vector3(wall.X, 0.5f, wall.Y), 1, 1, 1, Color.Gray);
                    Raylib.DrawCubeWires(new Vector3(wall.X, 0.5f, wall.Y), 1, 1, 1, Color.White);
                }

                foreach (var room in Rooms)
                {
                    Raylib.DrawCube(new Vector3(room.X, 0, room.Y), 1, 0.1f, 1, Color.Blue);
                }

                Raylib.EndMode3D();
                Raylib.DrawText("WASD + Mouse to fly", 10, 10, 20, Color.White);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }

        /// <summary>
        /// Exports the maze as a Godot .tscn file.
        /// </summary>
        public void ExportToGodot(string fileName)
        {
            StringBuilder tscn = new StringBuilder();

            tscn.AppendLine("[gd_scene load_steps=4 format=3]");

            tscn.AppendLine("\n[sub_resource type=\"BoxShape3D\" id=\"BoxShape_Wall\"]");
            tscn.AppendLine("size = Vector3(4, 4, 4)");
            tscn.AppendLine("\n[sub_resource type=\"BoxMesh\" id=\"BoxMesh_Wall\"]");
            tscn.AppendLine("size = Vector3(4, 4, 4)");


            ///COLOR RGBA values
            tscn.AppendLine(@"[sub_resource type= ""StandardMaterial3D"" id = ""Start_Room_Color""]");
            tscn.AppendLine(@"albedo_color = Color(0, 0.12, 0.8, 1)");

            tscn.AppendLine(@"[sub_resource type= ""StandardMaterial3D"" id = ""End_Room_Color""]");
            tscn.AppendLine(@"albedo_color = Color(0.4, 1, 0, 1)");


            tscn.AppendLine(@"[sub_resource type= ""StandardMaterial3D"" id = ""Wall_Color""]");
            tscn.AppendLine(@"albedo_color = Color(1, 0.95, 0.5, 1)");


            tscn.AppendLine("\n[sub_resource type=\"BoxShape3D\" id=\"BoxShape_Floor\"]");
            tscn.AppendLine("size = Vector3(4, 0.1, 4)");
            tscn.AppendLine("\n[sub_resource type=\"BoxMesh\" id=\"BoxMesh_Floor\"]");
            tscn.AppendLine("size = Vector3(4, 0.1, 4)");


            tscn.AppendLine("\n[node name=\"MazeLevel\" type=\"Node3D\"]");

            int i = 0;
            foreach (var wall in Walls)
            {
                // Multiply X and Y by 4 to space them out. 
                // Set Y-height to 2.0 (half the wall height) so the bottom sits at 0.
                float posX = wall.X * 4.0f;
                float posZ = wall.Y * 4.0f;

                tscn.AppendLine($"\n[node name=\"Wall_{i}\" type=\"StaticBody3D\" parent=\".\"]");
                tscn.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, 2.0, {posZ})");
                tscn.AppendLine($"[node name=\"Col\" type=\"CollisionShape3D\" parent=\"Wall_{i}\"]\nshape = SubResource(\"BoxShape_Wall\")");
                tscn.AppendLine($"[node name=\"Mesh\" type=\"MeshInstance3D\" parent=\"Wall_{i}\"]");

                tscn.AppendLine(@"material_override = SubResource(""Wall_Color"")");
                tscn.AppendLine(@"mesh = SubResource(""BoxMesh_Wall"")");
                i++;
            }

            i = 0;
            foreach (var room in Rooms)
            {
                float posX = room.X * 4.0f;
                float posZ = room.Y * 4.0f;

                tscn.AppendLine($"\n[node name=\"Floor_{i}\" type=\"StaticBody3D\" parent=\".\"]");
                // Floors stay at -0.05 to keep the top face at exactly 0.0
                tscn.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, -0.05, {posZ})");
                tscn.AppendLine($"[node name=\"Col\" type=\"CollisionShape3D\" parent=\"Floor_{i}\"]\nshape = SubResource(\"BoxShape_Floor\")");
                tscn.AppendLine($"[node name=\"Mesh\" type=\"MeshInstance3D\" parent=\"Floor_{i}\"]");

                if (room.Type.ToLowerInvariant().Equals("start"))
                {
                    tscn.AppendLine(@"material_override = SubResource(""Start_Room_Color"")");
                }
                else if (room.Type.ToLowerInvariant().Equals("end"))
                {
                    tscn.AppendLine(@"material_override = SubResource(""End_Room_Color"")");
                }
                tscn.AppendLine(@"mesh = SubResource(""BoxMesh_Floor"")");

                i++;
            }

            File.WriteAllText(fileName, tscn.ToString());
        }

        // Internal helper for JSON structure
        private class MazeDataDto
        {
            public List<MazeMetaData> Walls { get; set; } = new();
            public List<MazeMetaData> Rooms { get; set; } = new();
        }
    }
}
