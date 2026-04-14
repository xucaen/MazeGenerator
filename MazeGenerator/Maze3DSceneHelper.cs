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
            "scifi_torch_round.tscn", "scifi_torch_prism.tscn", "scifi_torch_box.tscn",
            "scifi_torch_capsule.tscn", "scifi_torch_cylinder.tscn", "scifi_torch_torus.tscn", "scifi_torch_tube.tscn"
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
            //float blockSize = 4.0f;

            // Use modulo to pick ONE torch type for this entire cube
            int torchIndex = cubeId % TorchTypes.Count;
            string selectedTorch = TorchTypes[torchIndex];

            // 1. REGISTER RESOURCES
            // Register your specific floor scene and the torch
            res.AppendLine($@"[ext_resource type=""PackedScene"" path=""res://floor.tscn"" id=""floor_scene""]");
            res.AppendLine($@"[ext_resource type=""PackedScene"" path=""res://{selectedTorch}"" id=""active_torch""]");

            // Materials
            res.AppendLine(@"[sub_resource type=""StandardMaterial3D"" id=""Start_Room_Color""]");
            res.AppendLine("albedo_color = Color(0, 0, 1, 1)"); // Blue
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
            res.AppendLine($"size = Vector3({Program.horizontalSize}, {Program.verticalSize}, {Program.horizontalSize})");

            res.AppendLine(@"[sub_resource type=""BoxMesh"" id=""Maze_BoxMesh""]");
            res.AppendLine($"size = Vector3({Program.horizontalSize}, {Program.verticalSize}, {Program.horizontalSize})");
            res.AppendLine(@"material = SubResource(""Wall_Material"")");

            // Exit Detection Shape
            res.AppendLine(@"[sub_resource type=""BoxShape3D"" id=""Exit_Trigger_Shape""]");
            res.AppendLine($"size = Vector3({Program.horizontalSize / 2}, {Program.verticalSize / 2}, {Program.horizontalSize / 2})");

            // 2. PROCESS WALLS
            for (int i = 0; i < Walls.Count; i++)
            {
                var wall = Walls[i];
                float gx = wall.X * Program.horizontalSize;
                float gy = wall.Y * Program.verticalSize;
                float gz = wall.Z * Program.horizontalSize;

                nodes.AppendLine($@"[node name=""Wall_{i}"" type=""StaticBody3D"" parent=""MazeSection/Walls""]");
                nodes.AppendLine($@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {gx}, {gy}, {gz})");
                string path = $"MazeSection/Walls/Wall_{i}";
                nodes.AppendLine($@"[node name=""Mesh"" type=""MeshInstance3D"" parent=""{path}""]");
                nodes.AppendLine(@"mesh = SubResource(""Maze_BoxMesh"")");
                nodes.AppendLine($@"[node name=""Col"" type=""CollisionShape3D"" parent=""{path}""]");
                nodes.AppendLine(@"shape = SubResource(""Maze_BoxShape"")");
            }

            // 3. PROCESS ROOMS (Floors and Torches)
            // Sort rooms by Z (Vertical) to find top and bottom
            var sortedRooms = Rooms.OrderBy(r => r.Z).ToList();
            var startRoom = sortedRooms.First();
            var endRoom = sortedRooms.Last();

            for (int r = 0; r < Rooms.Count; r++)
            {
                var room = Rooms[r];
                float gx = room.X * Program.horizontalSize;
                float gy = room.Y * Program.verticalSize;
                float gz = room.Z * Program.horizontalSize;


                float floorX = gx + (Program.horizontalSize / 2.0f);
                float floorY = gy - (Program.verticalSize / 2.0f); // Adjust floor to bottom of cell
                float floorZ = gz + (Program.horizontalSize / 2.0f);

                bool isSStartOrEnd = (room == startRoom || room == endRoom);


                // ONLY place a floor if it's NOT a vertical passage
                if (!room.IsVerticalPassage || isSStartOrEnd)
                {
                    // Instance your floor.tscn
                    nodes.AppendLine($@"[node name=""Floor_{r}"" parent=""MazeSection/Rooms"" instance=ExtResource(""floor_scene"")]");
                    nodes.AppendLine($@"transform = Transform3D(2, 0, 0, 0, 2, 0, 0, 0, 2, {gx}, {floorY}, {gz})");
                    Console.WriteLine($"Floor placed at ({gx}, {floorY}, {gz})");
                }
                else
                {
                    // Optional: Place a ladder or just leave it empty for a drop-hole
                    nodes.AppendLine($@"# Room_{r} is a vertical shaft - Floor skipped.");
                }

                // --- START ROOM LOGIC ---
                if (room == startRoom)
                {
                    // Apply Blue Color
                    nodes.AppendLine(@"surface_material_override/0 = SubResource(""Start_Room_Color"")");

                    // Add SpawnPoint (Marker3D)
                    // Note: Transform is relative to Floor instance. 0.5 scale cancels the floor's 2.0 scale.
                    nodes.AppendLine($@"[node name=""SpawnPoint"" type=""Marker3D"" parent=""MazeSection/Rooms/Floor_{r}""]");
                    nodes.AppendLine(@"transform = Transform3D(0.5, 0, 0, 0, 0.5, 0, 0, 0, 0.5, 0, 0.1, 0)");
                    nodes.AppendLine(@"gizmo_extents = 3.99");
                }
                // --- END ROOM LOGIC ---
                else if (room == endRoom)
                {
                    // Apply Green Color
                    nodes.AppendLine(@"surface_material_override/0 = SubResource(""End_Room_Color"")");

                    // Add Exit Area (Area3D)
                    nodes.AppendLine($@"[node name=""ExitArea"" type=""Area3D"" parent=""MazeSection/Rooms/Floor_{r}""]");
                    nodes.AppendLine(@"transform = Transform3D(0.5, 0, 0, 0, 0.5, 0, 0, 0, 0.5, 0, 1.0, 0)");
                    nodes.AppendLine($@"[node name=""Collision"" type=""CollisionShape3D"" parent=""MazeSection/Rooms/Floor_{r}/ExitArea""]");
                    nodes.AppendLine(@"shape = SubResource(""Exit_Trigger_Shape"")");
                }

                bool shouldPlaceTorch = room.IsLit;
                //experiment - only place torches at the vertical drop or start and end
                if (shouldPlaceTorch)
                {
                    float torchX = gx + (Program.horizontalSize / 2.0f);
                    float torchY = floorY + 2.0f;
                    float torchZ = gz + (Program.horizontalSize / 2.0f);
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