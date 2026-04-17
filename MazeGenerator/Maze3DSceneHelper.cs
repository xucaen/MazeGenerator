using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MazeGenerator
{
    public class Maze3DSceneHelper
    {
        private List<Maze3DMetaData> Walls = new List<Maze3DMetaData>();
        private List<Maze3DMetaData> Rooms = new List<Maze3DMetaData>();

        // These match your specific file names
        private readonly List<string> TorchTypes = new List<string>() {
            "Dungeon_torch_capsul.tscn"
        };

        public void LoadFromJson(string json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<MazeDataDto>(json, options);
            Walls = data?.Walls ?? new List<Maze3DMetaData>();
            Rooms = data?.Rooms ?? new List<Maze3DMetaData>();
        }

        public SceneParts MakeGodotScene(int cubeId)
        {
            StringBuilder res = new StringBuilder();
            StringBuilder nodes = new StringBuilder();

            // Use modulo to pick ONE torch type for this entire cube
            int torchIndex = cubeId % TorchTypes.Count;
            string selectedTorch = TorchTypes[torchIndex];

            // 1. REGISTER RESOURCES
            // Register your specific floor scene and the torch
            res.AppendLine($@"[ext_resource type=""PackedScene"" path=""res://assets/{selectedTorch}"" id=""active_torch""]");

            // Materials
            res.AppendLine(@"[sub_resource type=""StandardMaterial3D"" id=""End_Room_Color""]");
            res.AppendLine("albedo_color = Color(0, 1, 0, 1)"); // Green
                                                                // Define a Shader that colors based on face direction
            res.AppendLine(@"[sub_resource type=""Shader"" id=""Wall_Shader""]");
            res.AppendLine("code = \"shader_type spatial;\\n" +
                           "uniform vec4 top_color : source_color;\\n" +
                           "uniform vec4 side_color : source_color;\\n" +
                           "varying vec3 local_normal;\\n" +
                           "void vertex() { local_normal = NORMAL; }\\n" +
                           "void fragment() {\\n" +
                           "    if (local_normal.y > 0.5) ALBEDO = top_color.rgb;\\n" + // If pointing UP
                           "    else ALBEDO = side_color.rgb;\\n" +                      // All other sides
                           "}\"");

            // Define the Material using the Shader
            res.AppendLine(@"[sub_resource type=""ShaderMaterial"" id=""Wall_Material""]");
            res.AppendLine("shader = SubResource(\"Wall_Shader\")");
            res.AppendLine("shader_parameter/top_color = Color(0, 0, 0.4, 1)");   // Dark Blue
            res.AppendLine("shader_parameter/side_color = Color(0.9, 0.8, 0.2, 1)"); // Yellowish


            // Register Wall Mesh (Keep these procedural or swap for a .tscn if you have one)
            res.AppendLine(@"[sub_resource type=""BoxShape3D"" id=""Maze_BoxShape""]");
            res.AppendLine(@$"size = Vector3({CubeDimensions.HorizontalSize}, {CubeDimensions.VerticalSize}, {CubeDimensions.HorizontalSize})");

            res.AppendLine(@"[sub_resource type=""BoxMesh"" id=""Maze_BoxMesh""]");
            res.AppendLine(@$"size = Vector3({CubeDimensions.HorizontalSize}, {CubeDimensions.VerticalSize}, {CubeDimensions.HorizontalSize})");
            res.AppendLine(@"material = SubResource(""Wall_Material"")");

            // Exit Detection Shape
            res.AppendLine(@"[sub_resource type=""BoxShape3D"" id=""Exit_Trigger_Shape""]");
            res.AppendLine(@$"size = Vector3({CubeDimensions.HorizontalSize / 2}, {CubeDimensions.VerticalSize / 2}, {CubeDimensions.HorizontalSize / 2})");

            // 2. PROCESS WALLS
            List<Maze3DMetaData> sortedWalls = Walls.OrderByDescending(w => w.Y).ToList();
            Maze3DMetaData firstWall = sortedWalls.First();//to get the proper Y index

            for (int i = 0; i < Walls.Count; i++)
            {
                var wall = Walls[i];
                float gx = wall.X * CubeDimensions.HorizontalSize;
                float gy = wall.Y * CubeDimensions.VerticalSize;
                float gz = wall.Z * CubeDimensions.HorizontalSize;

                nodes.AppendLine($@"[node name=""Wall_{i}"" type=""StaticBody3D"" parent=""MazeSection/Walls""]");
                nodes.AppendLine($@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {gx}, {gy}, {gz})");
                string path = $"MazeSection/Walls/Wall_{i}";
                nodes.AppendLine($@"[node name=""Mesh"" type=""MeshInstance3D"" parent=""{path}""]");
                nodes.AppendLine(@"mesh = SubResource(""Maze_BoxMesh"")");
                nodes.AppendLine($@"[node name=""Col"" type=""CollisionShape3D"" parent=""{path}""]");
                nodes.AppendLine(@"shape = SubResource(""Maze_BoxShape"")");

               // Console.WriteLine($"Create wall node: \tWall_{i} at ({wall.X}, {wall.Y}, {wall.Z})");
            }

            // 3. PROCESS ROOMS (Floors and Torches)

            for (int r = 0; r < Rooms.Count; r++)
            {
                var room = Rooms[r];
                float gx = room.X * CubeDimensions.HorizontalSize;
                float gy = room.Y * CubeDimensions.VerticalSize;
                float gz = room.Z * CubeDimensions.HorizontalSize;


                if (room.IsLit)
                {
                    // Center the torch horizontally in the 5x5 area (if HorizontalSize is 5)
                    float torchX = gx;
                    float torchZ = gz;
                    float torchY = gy - 2.0f;

                    nodes.AppendLine($@"[node name=""Torch_{r}"" parent=""MazeSection/Torches"" instance=ExtResource(""active_torch"")]");
                    nodes.AppendLine($@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {torchX}, {torchY}, {torchZ})");

                    Console.WriteLine($"Torch placed at ({torchX}, {torchY}, {torchZ})");

                }

            }

            return new SceneParts { Resources = res.ToString(), Nodes = nodes.ToString() };
        }

        private class MazeDataDto { public List<Maze3DMetaData> Walls { get; set; } public List<Maze3DMetaData> Rooms { get; set; } }
    }
}