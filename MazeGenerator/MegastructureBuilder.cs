using System.Text;
using System.IO;

namespace MazeGenerator
{
    public class MegastructureBuilder
    {
        private StringBuilder sb = new StringBuilder();
        private string helixFileName;
        private float helixHeight;

        public MegastructureBuilder(string helixFileName, float finalHeight)
        {
            this.helixFileName = helixFileName;
            this.helixHeight = finalHeight;
        }

        public void Build(int level)
        {
            sb.AppendLine("[gd_scene format=3]");
            sb.AppendLine();

            sb.AppendLine($"[ext_resource type=\"PackedScene\" path=\"res://{helixFileName}\" id=\"helix_scene\"]");
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://alien_diamond.tscn\" id=\"diamond\"]");
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://alien_machine.tscn\" id=\"machine\"]");
            sb.AppendLine();

            sb.AppendLine($"[node name=\"Megastructure_Lvl{level}\" type=\"Node3D\"]");
            sb.AppendLine();

            sb.AppendLine("[node name=\"MazeHelix\" parent=\".\" instance=ExtResource(\"helix_scene\")]");
            sb.AppendLine();

            float diamondY = helixHeight + 30.0f;
            sb.AppendLine("[node name=\"AlienDiamond\" parent=\".\" instance=ExtResource(\"diamond\")]");
            sb.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, {diamondY}, 0)");
            sb.AppendLine();

            float circleRadius = 40.0f;
            for (int i = 0; i < 5; i++)
            {
                float angle = i * (MathF.PI * 2 / 5);
                float x = MathF.Cos(angle) * circleRadius;
                float z = MathF.Sin(angle) * circleRadius;

                sb.AppendLine($"[node name=\"AlienMachine_{i}\" parent=\".\" instance=ExtResource(\"machine\")]");
                sb.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {x}, 10.0, {z})");
                sb.AppendLine();
            }

            File.WriteAllText($"../../../Megastructure_Lvl{level}.tscn", sb.ToString());
        }
    }
}