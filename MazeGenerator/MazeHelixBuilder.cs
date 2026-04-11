using System.Text;
using System.IO;

namespace MazeGenerator
{
    public class MazeHelixBuilder
    {
        private StringBuilder resourceBucket = new StringBuilder();
        private StringBuilder nodeBucket = new StringBuilder();
        private int level;
        private const float Radius = 10.0f;
        private const float HeightStep = 25.0f;
        private const float RotationStep = 0.5f;

        public float FinalHelixHeight { get; private set; }

        public MazeHelixBuilder(int level) => this.level = level;

        public void Build(int width, int height, int NumberOfMazesInThisHelix)
        {
            resourceBucket.Clear();
            nodeBucket.Clear();

            resourceBucket.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://scifi_torch.tscn\" id=\"1_scifi_torch\"]");
            resourceBucket.AppendLine("[ext_resource type=\"Script\" path=\"res://scripts/level_transition.gd\" id=\"2_level_transition\"]");


            nodeBucket.AppendLine($"[node name=\"Helix_Level_{level}\" type=\"Node3D\"]");
            nodeBucket.AppendLine();

            for (int i = 0; i < NumberOfMazesInThisHelix; i++)
            {
                var gridMaker = new MazeGrideMaker(width, height, 'x', i + level, true);
                gridMaker.Generate();
                gridMaker.MakeMoreRooms();
                gridMaker.MakeObservationDeck();

                var converter = new MazeGridToJSONConverter();
                string jsonMaze = converter.ConvertGridToJson(gridMaker.GetFullGrid());

                var helper = new MazeSceneHelper();
                helper.LoadFromJson(jsonMaze);

                var parts = helper.MakeGodotScene(i);
                resourceBucket.Append(parts.Resources);

                float angle = i * RotationStep;
                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;
                float y = i * HeightStep;

                float cosA = MathF.Cos(angle);
                float sinA = MathF.Sin(angle);

                string transform = $"Transform3D({cosA}, 0, {-sinA}, 0, 1, 0, {sinA}, 0, {cosA}, {x}, {y}, {z})";

                nodeBucket.AppendLine($"[node name=\"MazeSection_{i}\" type=\"Node3D\" parent=\".\"]");
                nodeBucket.AppendLine($"transform = {transform}");
                nodeBucket.Append(parts.Nodes);

                this.FinalHelixHeight = y;
            }
        }

        public void Save(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            int resourceCountEstimate = resourceBucket.ToString().Split("[sub_resource").Length - 1
                                        + resourceBucket.ToString().Split("[ext_resource").Length - 1;

            sb.AppendLine($"[gd_scene load_steps={resourceCountEstimate + 1} format=3]");
            sb.AppendLine();
            sb.Append(resourceBucket.ToString());
            sb.AppendLine();
            sb.Append(nodeBucket.ToString());

            File.WriteAllText($"../../../{fileName}", sb.ToString());
        }
    }
}