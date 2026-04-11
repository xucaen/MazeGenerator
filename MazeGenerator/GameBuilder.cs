using System;
using System.Text;
using System.IO;

namespace MazeGenerator
{
    public class GameBuilder
    {
        private StringBuilder sb = new StringBuilder();
        public int NumberOfLevels { get; }
        public int NumberOfMazesPerLevel { get; }

        public GameBuilder(int NumberOfLevels, int NumberOfMazesPerLevel)
        {
            this.NumberOfLevels = NumberOfLevels;
            this.NumberOfMazesPerLevel = NumberOfMazesPerLevel;
        }

        internal void Init()
        {
            sb.AppendLine("[gd_scene format=3]");
            sb.AppendLine();
            sb.AppendLine("[ext_resource type=\"Script\" path=\"res://main.gd\" id=\"main_gd\"]");
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://player.tscn\" id=\"player\"]");
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://world.tscn\" id=\"world\"]");
            // Add the megastructure resource link
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://Megastructure_Lvl0.tscn\" id=\"megastructure\"]");
            sb.AppendLine();

            // TSCN requires exactly one scene root
            sb.AppendLine("[node name=\"Labyrinth\" type=\"Node3D\"]");
        }

        public void Build()
        {
            Init();
            for (int level = 0; level < this.NumberOfLevels; ++level)
            {
                string helixName = $"Helix_Level_{level}.tscn";
                MazeHelixBuilder helixBuilder = new MazeHelixBuilder(level);
                helixBuilder.Build(9, 9, this.NumberOfMazesPerLevel);
                helixBuilder.Save(helixName);

                MegastructureBuilder megaBuilder = new MegastructureBuilder(helixName, helixBuilder.FinalHelixHeight);
                megaBuilder.Build(level);

                Finish(helixName, level);
            }
        }

        private void Finish(string helixName, int level)
        {
            // Nodes are parented to the root defined in Init()
            sb.AppendLine("[node name=\"Player\" parent=\".\" instance=ExtResource(\"player\")]");
            sb.AppendLine("[node name=\"World\" parent=\".\" instance=ExtResource(\"world\")]");
            sb.AppendLine("[node name=\"Megastructure\" parent=\".\" instance=ExtResource(\"megastructure\")]");

            sb.AppendLine();

            string GameFileName = $"Labyrinth3D.tscn";
            try
            {
                File.WriteAllText($"../../../{GameFileName}", sb.ToString());
                Console.WriteLine($"Scene successfully generated at {GameFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }
}