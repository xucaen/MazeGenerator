using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace MazeGenerator
{
    public class MazeHelixBuilder
    {
        public StringBuilder ResourceSB { get; } = new StringBuilder();
        public StringBuilder NodeSB { get; } = new StringBuilder();

        private readonly HashSet<string> resourceSet = new HashSet<string>();

        private int level;

        private const float Radius = 10.0f;
        private const float HeightStep = 25.0f;
        private const float RotationStep = 0.5f;

        public float FinalHelixHeight { get; private set; }

        public MazeHelixBuilder(int level)
        {
            this.level = level;
        }

        public void Build(int width, int height, int numberOfMazes, string torchResourceFile)
        {
            ResourceSB.Clear();
            NodeSB.Clear();
            resourceSet.Clear();

            // Base resources (deduplicated)
            AddResourceLine($"[ext_resource type=\"PackedScene\" path=\"res://assets/{torchResourceFile}\" id=\"1_scifi_torch\"]");
            AddResourceLine("[ext_resource type=\"Script\" path=\"res://scripts/level_transition.gd\" id=\"2_level_transition\"]");

            // Root node
            NodeSB.AppendLine($"[node name=\"Helix_Level_{level}\" type=\"Node3D\"]");
            NodeSB.AppendLine();

            for (int i = 0; i < numberOfMazes; i++)
            {
                // --- Generate Maze ---
                var gridMaker = new MazeGrideMaker(width, height, 'x', i + level, true);
                gridMaker.Generate();
                gridMaker.MakeMoreRooms();
                gridMaker.MakeObservationDeck();

                var converter = new MazeGridToJSONConverter();
                string jsonMaze = converter.ConvertGridToJson(gridMaker.GetFullGrid());

                var helper = new MazeSceneHelper();
                helper.LoadFromJson(jsonMaze);

                var parts = helper.MakeGodotScene(i);

                // --- Resources (deduped) ---
                AppendResourceBlock(parts.Resources);

                // --- Transform math ---
                float angle = i * RotationStep;

                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;
                float y = i * HeightStep;

                float cosA = MathF.Cos(angle);
                float sinA = MathF.Sin(angle);


                string transform =
                    $"Transform3D({cosA}, 0, {(sinA == 0.0f ? 0.0f : -sinA) }, 0, 1, 0, {sinA}, 0, {cosA}, {x}, {y}, {z})";

                // --- Node ---
                NodeSB.AppendLine($"[node name=\"MazeSection_{i}\" type=\"Node3D\" parent=\".\"]");
                NodeSB.AppendLine($"transform = {transform}");
                NodeSB.Append(parts.Nodes);

                FinalHelixHeight = y;
            }
        }

        // --- Dedup helpers ---
        private void AddResourceLine(string line)
        {
            if (resourceSet.Add(line))
                ResourceSB.AppendLine(line);
        }

        private void AppendResourceBlock(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
                return;

            // Normalize line endings
            var normalized = block.Replace("\r\n", "\n");

            // Split by resource blocks
            var chunks = normalized.Split("\n[");

            foreach (var chunk in chunks)
            {
                string trimmed = chunk.Trim();

                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // Restore missing '[' if split removed it
                if (!trimmed.StartsWith("["))
                    trimmed = "[" + trimmed;

                // Dedupe ONLY ext_resource lines
                if (trimmed.StartsWith("[ext_resource"))
                {
                    if (resourceSet.Add(trimmed))
                        ResourceSB.AppendLine(trimmed);
                }
                else
                {
                    // sub_resource or anything else → keep whole block intact
                    ResourceSB.AppendLine(trimmed);
                    ResourceSB.AppendLine();
                }
            }
        }

        public void Save(string fileName)
        {
            StringBuilder sb = new StringBuilder();

            int resourceCount =
                ResourceSB.ToString().Split("[ext_resource").Length - 1 +
                ResourceSB.ToString().Split("[sub_resource").Length - 1;

            sb.AppendLine($"[gd_scene load_steps={resourceCount + 1} format=3]");
            sb.AppendLine();

            sb.Append(ResourceSB);
            sb.AppendLine();

            sb.Append(NodeSB);

            File.WriteAllText($"../../../{fileName}", sb.ToString());
        }
    }
}