using System.Reflection.Emit;
using System.Text;
using System.Text.Json;

namespace MazeGenerator
{
    public class MazeSceneHelper
    {
        public record GridPoint(int X, int Y);

        private HashSet<MazeMetaData> Walls = new HashSet<MazeMetaData>();
        private HashSet<MazeMetaData> Rooms = new HashSet<MazeMetaData>();

        public void LoadFromJson(string jsonString)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<MazeDataDto>(jsonString, options);

            Walls.Clear();
            Rooms.Clear();

            if (data?.Walls != null)
                foreach (var p in data.Walls)
                    Walls.Add(p);

            if (data?.Rooms != null)
                foreach (var p in data.Rooms)
                    Rooms.Add(p);
        }

        public SceneParts MakeGodotScene(int MazeCount)
        {
            StringBuilder res = new StringBuilder();
            StringBuilder nodes = new StringBuilder();

            // Resources must be defined before they are referenced
            res.AppendLine($"[sub_resource type=\"BoxShape3D\" id=\"BoxShape3D_Exit\"]");
            res.AppendLine("size = Vector3(4, 4, 4)");
            res.AppendLine();

            res.AppendLine($"[sub_resource type=\"StandardMaterial3D\" id=\"Start_Room_Color\"]");
            res.AppendLine("albedo_color = Color(0, 0, 1, 1)");
            res.AppendLine();

            res.AppendLine($"[sub_resource type=\"StandardMaterial3D\" id=\"End_Room_Color\"]");
            res.AppendLine("albedo_color = Color(0, 1, 0, 1)");
            res.AppendLine();

            res.AppendLine($"[sub_resource type=\"BoxShape3D\" id=\"{MazeCount}_WallShape\"]");
            res.AppendLine("size = Vector3(4, 4, 4)");
            res.AppendLine();

            res.AppendLine($"[sub_resource type=\"BoxMesh\" id=\"{MazeCount}_WallMesh\"]");
            res.AppendLine("size = Vector3(4, 4, 4)");
            res.AppendLine();

            res.AppendLine(@$"[sub_resource type= ""StandardMaterial3D"" id = ""{MazeCount}_Wall_Color""]");
            float r = (MazeCount * 0.3f) % 1.0f;
            float g = (MazeCount * 0.6f) % 1.0f;
            float b = (MazeCount * 0.9f) % 1.0f;
            res.AppendLine(@$"albedo_color = Color({r:F3}, {g:F3}, {b:F3}, 1)");


            int i = 0;
            nodes.AppendLine($"[node name=\"Walls\" type=\"Node3D\" parent=\"MazeSection_{MazeCount}\"]");


            foreach (var wall in Walls)
            {
                float posX = wall.X * 4.0f;
                float posZ = wall.Y * 4.0f;

                nodes.AppendLine($"[node name=\"Wall_{i}\" type=\"StaticBody3D\" parent=\"MazeSection_{MazeCount}/Walls\"]");
                nodes.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, 2, {posZ})");

                nodes.AppendLine($"[node name=\"Mesh\" type=\"MeshInstance3D\" parent=\"MazeSection_{MazeCount}/Walls/Wall_{i}\"]");
                nodes.AppendLine($"mesh = SubResource(\"{MazeCount}_WallMesh\")");
                nodes.AppendLine($"surface_material_override/0 = SubResource(\"{MazeCount}_Wall_Color\")");

                nodes.AppendLine($"[node name=\"Col\" type=\"CollisionShape3D\" parent=\"MazeSection_{MazeCount}/Walls/Wall_{i}\"]");
                nodes.AppendLine($"shape = SubResource(\"{MazeCount}_WallShape\")");
                nodes.AppendLine();
                i++;
            }

            i = 0;
            nodes.AppendLine($"[node name=\"Rooms\" type=\"Node3D\" parent=\"MazeSection_{MazeCount}\"]");

            foreach (var room in Rooms)
            {
                float posX = room.X * 4.0f;
                float posZ = room.Y * 4.0f;
                string floorPath = $"MazeSection_{MazeCount}/Rooms/Floor_{i}";

                nodes.AppendLine($"\n[node name=\"Floor_{i}\" type=\"StaticBody3D\" parent=\"MazeSection_{MazeCount}/Rooms\"]");
                nodes.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, -0.05, {posZ})");

                nodes.AppendLine($"[node name=\"Col\" type=\"CollisionShape3D\" parent=\"{floorPath}\"]");
                nodes.AppendLine($"shape = SubResource(\"{MazeCount}_WallShape\")");

                nodes.AppendLine($"[node name=\"Mesh\" type=\"MeshInstance3D\" parent=\"{floorPath}\"]");
                nodes.AppendLine($"mesh = SubResource(\"{MazeCount}_WallMesh\")");

                if (room.Type.ToLowerInvariant().Equals("start"))
                {
                    nodes.AppendLine(@"surface_material_override/0 = SubResource(""Start_Room_Color"")");
                }
                else if (room.Type.ToLowerInvariant().Equals("end"))
                {
                    nodes.AppendLine(@"surface_material_override/0 = SubResource(""End_Room_Color"")");
                }

                if (room.Type.ToLowerInvariant().Equals("end"))
                {
                    nodes.AppendLine($"\n[node name=\"ExitArea\" type=\"Area3D\" parent=\"{floorPath}\"]");
                    nodes.AppendLine("script = ExtResource(\"2_level_transition\")");
                    nodes.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.1, 0)");


                    nodes.AppendLine($"\n[node name=\"CollisionShape3D\" type=\"CollisionShape3D\" parent=\"{floorPath}/ExitArea\"]");
                    nodes.AppendLine(@"shape = SubResource(""BoxShape3D_Exit"")");

                    nodes.AppendLine($@"[connection signal=""body_entered"" from=""{floorPath}/ExitArea"" to=""{floorPath}/ExitArea"" method=""_on_body_entered""]");
                }
                else if (room.Type.ToLowerInvariant().Equals("start"))
                {
                    nodes.AppendLine($"\n[node name=\"SpawnPoint\" type=\"Marker3D\" parent=\"{floorPath}\"]");
                    nodes.AppendLine(@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.1, 0)");
                    nodes.AppendLine(@"gizmo_extents = 3.99");
                }
                i++;
            }

            nodes.AppendLine($"[node name=\"Torches\" type=\"Node3D\" parent=\"MazeSection_{MazeCount}\"]");
            i = 0;
            foreach (var room in Rooms)
            {
                if (room.IsLit)
                {
                    float posX = room.X * 4.0f;
                    float posZ = room.Y * 4.0f;
                    float posY = 5.0f;
                    nodes.AppendLine($"[node name=\"ScifiTorch_{i}\" parent=\"MazeSection_{MazeCount}/Torches\" instance=ExtResource(\"1_scifi_torch\")]");
                    nodes.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {posX}, {posY}, {posZ})");
                }
                i++;
            }

            return new SceneParts { Resources = res.ToString(), Nodes = nodes.ToString() };
        }

        private class MazeDataDto
        {
            public List<MazeMetaData> Walls { get; set; }
            public List<MazeMetaData> Rooms { get; set; }
        }
    }

    public struct SceneParts
    {
        public string Resources;
        public string Nodes;
    }
}