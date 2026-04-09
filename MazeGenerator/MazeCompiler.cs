using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Color = Raylib_cs.Color;

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
        /// Exports the maze as a Godot .tscn file.
        /// </summary>
        public string ExportToGodot(string fileName, int level)
        {
            StringBuilder tscn = new StringBuilder();
            string levelName = "";


            tscn.AppendLine("[gd_scene load_steps=4 format=3]");

            tscn.AppendLine(@$"[ext_resource type= ""Script"" path = ""res://level_transition.gd"" id = ""Level_Transition""]");

            tscn.AppendLine($@"[ext_resource type= ""PackedScene"" path = ""res://scifi_torch.tscn"" id = ""ScifiTorch""]");


            tscn.AppendLine("\n[sub_resource type=\"BoxShape3D\" id=\"BoxShape_Wall\"]");
            tscn.AppendLine("size = Vector3(4, 4, 4)");
            tscn.AppendLine("\n[sub_resource type=\"BoxMesh\" id=\"BoxMesh_Wall\"]");
            tscn.AppendLine("size = Vector3(4, 4, 4)");

            tscn.AppendLine(@"[sub_resource type= ""BoxShape3D"" id = ""BoxShape3D_Exit""]");


            ///COLOR RGBA values
            tscn.AppendLine(@"[sub_resource type= ""StandardMaterial3D"" id = ""Start_Room_Color""]");
            tscn.AppendLine(@"albedo_color = Color(0, 0.12, 0.8, 1)");

            tscn.AppendLine(@"[sub_resource type= ""StandardMaterial3D"" id = ""End_Room_Color""]");
            tscn.AppendLine(@"albedo_color = Color(0.4, 1, 0, 1)");




            tscn.AppendLine(@"[sub_resource type= ""StandardMaterial3D"" id = ""Wall_Color""]");
            float r = (level * 0.3f) % 1.0f;
            float g = (level * 0.6f) % 1.0f;
            float b = (level * 0.9f) % 1.0f;
            tscn.AppendLine(@$"albedo_color = Color({r:F3}, {g:F3}, {b:F3}, 1)");

            tscn.AppendLine("\n[sub_resource type=\"BoxShape3D\" id=\"BoxShape_Floor\"]");
            tscn.AppendLine("size = Vector3(4, 0.1, 4)");
            tscn.AppendLine("\n[sub_resource type=\"BoxMesh\" id=\"BoxMesh_Floor\"]");
            tscn.AppendLine("size = Vector3(4, 0.1, 4)");

            levelName = $"MazeLevel{level}";

            tscn.AppendLine($"\n[node name=\"{levelName}\" type=\"Node3D\"]");


            int i = 0;
            tscn.AppendLine($"[node name= \"Walls\" type = \"Node3D\" parent = \".\"] ");
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
            tscn.AppendLine($"[node name= \"Rooms\" type = \"Node3D\" parent = \".\"] ");
            foreach (var room in Rooms)
            {
                float posX = room.X * 4.0f;
                float posZ = room.Y * 4.0f;

                tscn.AppendLine($"\n[node name=\"Floor_{i}\" type=\"StaticBody3D\" parent=\".\"]");
                // Floors stay at -0.05 to keep the top face at exactly 0.0
                tscn.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, -0.05, {posZ})");
                tscn.AppendLine($"[node name=\"Col\" type=\"CollisionShape3D\" parent=\"Floor_{i}\"]\nshape = SubResource(\"BoxShape_Floor\")");
                tscn.AppendLine($"[node name=\"Mesh\" type=\"MeshInstance3D\" parent=\"Floor_{i}\"]");


                ///order is important

                //set the color
                if (room.Type.ToLowerInvariant().Equals("start"))
                {
                    tscn.AppendLine(@"material_override = SubResource(""Start_Room_Color"")");
                }
                else if (room.Type.ToLowerInvariant().Equals("end"))
                {
                    tscn.AppendLine(@"material_override = SubResource(""End_Room_Color"")");
                }
                tscn.AppendLine(@"mesh = SubResource(""BoxMesh_Floor"")");


                //set the Area3D to go to next level
                if (room.Type.ToLowerInvariant().Equals("end"))
                {
                    tscn.AppendLine(@"[node name= ""ExitArea"" type = ""Area3D"" parent = ""."" ]");
                    tscn.AppendLine(@$"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, 0.05, {posZ})");
                    tscn.AppendLine(@"script = ExtResource(""Level_Transition"")");
                    tscn.AppendLine(@"[node name = ""CollisionShape3D"" type = ""CollisionShape3D"" parent = ""ExitArea""]");
                    tscn.AppendLine(@"shape = SubResource(""BoxShape3D_Exit"")");                    tscn.AppendLine(@"[connection signal=""body_entered"" from=""ExitArea"" to=""ExitArea"" method=""_on_body_entered""]");                }
                else if (room.Type.ToLowerInvariant().Equals("start"))
                {
                    tscn.AppendLine(@"[node name= ""SpawnPoint"" type = ""Marker3D"" parent = ""."" ]");
                    tscn.AppendLine(@$"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, 0.05, {posZ})");
                    tscn.AppendLine(@"gizmo_extents = 3.99");

                }

                i++;
            }
            //TODO:

            tscn.AppendLine($"[node name= \"Torches\" type = \"Node3D\" parent = \".\" ]");
            i = 0;
            foreach (var room in Rooms)
            {
                float posX = room.X * 4.0f;//everything is 4x4x4 meters
                float posZ = room.Y * 4.0f;

                if (room.IsLit)
                {
                    tscn.AppendLine($"\n[node name= \"ScifiTorch_{i}\" parent = \"Torches\"  instance = ExtResource(\"ScifiTorch\")]");
                    tscn.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, 4.0, {posZ})");
                }
                ++i;
            }

            File.WriteAllText(fileName, tscn.ToString());
            return levelName;
        }

        // Internal helper for JSON structure
        private class MazeDataDto
        {
            public List<MazeMetaData> Walls { get; set; } = new();
            public List<MazeMetaData> Rooms { get; set; } = new();

        }
    }
}
