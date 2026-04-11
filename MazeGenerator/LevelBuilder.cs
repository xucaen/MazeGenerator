using System;
using System.Text;
using System.IO;

namespace MazeGenerator
{
    public class LevelBuilder
    {
        private StringBuilder sb;
        public int NumberOfLevels { get; }
        public int NumberOfMazesPerLevel { get; }

        public LevelBuilder(StringBuilder sb, int NumberOfLevels, int NumberOfMazesPerLevel)
        {
            this.sb = sb;
            this.NumberOfLevels = NumberOfLevels;
            this.NumberOfMazesPerLevel = NumberOfMazesPerLevel;
        }

        internal void Init()
        {
            sb.AppendLine("[gd_scene format=3]");
            sb.AppendLine();
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://player.tscn\" id=\"player\"]");
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://world.tscn\" id=\"world\"]");
            // Add the megastructure resource link

            // Dynamic Resources: Load each Helix and Megastructure generated for each level
            for (int level = 0; level < NumberOfLevels; level++)
            {
                sb.AppendLine($"[ext_resource type=\"PackedScene\" path=\"res://Megastructure_Lvl{level}.tscn\" id=\"mega_lvl_{level}\"]");
            }
            sb.AppendLine();


            // TSCN requires exactly one scene root
            sb.AppendLine("[node name=\"Labyrinth\" type=\"Node3D\"]");
        }

        public void Build()
        {
            // FIRST PASS: Generate external files and write resource links
            // This must happen before the root node is declared in the TSCN
            for (int level = 0; level < this.NumberOfLevels; ++level)
            {
                string helixName = $"Helix_Level_{level}.tscn";

                // Construct the Helix file
                MazeHelixBuilder helixBuilder = new MazeHelixBuilder(level);
                helixBuilder.Build(9, 9, this.NumberOfMazesPerLevel);
                helixBuilder.Save(helixName);

                // Construct the Megastructure file (linking the helix inside)
                MegastructureBuilder megaBuilder = new MegastructureBuilder(helixName, helixBuilder.FinalHelixHeight);
                megaBuilder.Build(level);

                // Add resource link to the main string buffer
                sb.AppendLine($"[ext_resource type=\"PackedScene\" path=\"res://Megastructure_Lvl{level}.tscn\" id=\"mega_lvl_{level}\"]");
            }
        }

        public void Finish()
        {

            double angleStep = (2 * Math.PI) / this.NumberOfLevels;

            for (int level = 0; level < this.NumberOfLevels; ++level)
            {
                // Calculate current angle
                double angle = level * angleStep;

                // Radius is 5000m
                float radius = 1500f;

                // Circular coordinates
                float xPos = (float)(Math.Cos(angle) * radius);
                float zPos = (float)(Math.Sin(angle) * radius);
                int yPos = level * 100;

                sb.AppendLine($"[node name=\"Megastructure_{level}\" parent=\".\" instance=ExtResource(\"mega_lvl_{level}\")]");

                // Transform3D(x.x, x.y, x.z, y.x, y.y, y.z, z.x, z.y, z.z, origin.x, origin.y, origin.z)
                // origin.x is 10th, origin.y is 11th, origin.z is 12th
                sb.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {xPos}, {yPos}, {zPos})");
                sb.AppendLine();
            }
        }
    }
}