using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace MazeGenerator
{
    public class LevelBuilder
    {
        public StringBuilder ResourceSB { get; } = new StringBuilder();
        public StringBuilder NodeSB { get; } = new StringBuilder();

        public int NumberOfLevels { get; }
        public int NumberOfMazesPerLevel { get; }

        private readonly List<string> TorchTypes = new List<string>()
        {
            "scifi_torch_round.tscn",
            "scifi_torch_prism.tscn",
            "scifi_torch_box.tscn",
            "scifi_torch_capsule.tscn",
            "scifi_torch_cylinder.tscn",
            "scifi_torch_torus.tscn",
            "scifi_torch_tube.tscn"
        };

        public LevelBuilder(int numberOfLevels, int numberOfMazesPerLevel)
        {
            this.NumberOfLevels = numberOfLevels;
            this.NumberOfMazesPerLevel = numberOfMazesPerLevel;
        }

        public void Build()
        {
            // Generate external files + register resources
            for (int level = 0; level < this.NumberOfLevels; ++level)
            {
                string helixName = $"Helix_Level_{level}.tscn";

                // Build Helix
                MazeHelixBuilder helixBuilder = new MazeHelixBuilder(level);
                helixBuilder.Build(9, 9, this.NumberOfMazesPerLevel, TorchTypes[level % TorchTypes.Count]);
                helixBuilder.Save(helixName);

                // Build Megastructure
                MegastructureBuilder megaBuilder = new MegastructureBuilder(helixName, helixBuilder.FinalHelixHeight);
                megaBuilder.Build(level);

                //Resource goes ONLY into ResourceSB
                ResourceSB.AppendLine(
                    $"[ext_resource type=\"PackedScene\" path=\"res://Megastructure_Lvl{level}.tscn\" id=\"mega_lvl_{level}\"]"
                );
            }

            ResourceSB.AppendLine();
        }

        public void Finish()
        {
            double angleStep = (2 * Math.PI) / this.NumberOfLevels;

            for (int level = 0; level < this.NumberOfLevels; ++level)
            {
                double angle = level * angleStep;

                float radius = 1500f;
                float xPos = (float)(Math.Cos(angle) * radius);
                float zPos = (float)(Math.Sin(angle) * radius);
                int yPos = level * 100;

                // Nodes go ONLY into NodeSB
                NodeSB.AppendLine(
                    $"[node name=\"Megastructure_{level}\" parent=\".\" instance=ExtResource(\"mega_lvl_{level}\")]"
                );

                NodeSB.AppendLine(
                    $"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {xPos}, {yPos}, {zPos})"
                );

                NodeSB.AppendLine();
            }
        }
    }
}