using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    public class GameBuilder
    {
        private StringBuilder sb = new StringBuilder();
        public void Build()
        {
            int totalLevels = 7;
            int mazesPerLevel = 30;

            LevelBuilder levels = new LevelBuilder(sb, totalLevels, mazesPerLevel);

            // SECTION 1: HEADER & RESOURCES
            sb.AppendLine("[gd_scene format=3]");
            sb.AppendLine();

            // Global Resources
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://player.tscn\" id=\"player\"]");
            sb.AppendLine("[ext_resource type=\"PackedScene\" path=\"res://world.tscn\" id=\"world\"]");

            // Level-specific Resources
            levels.Build();

            sb.AppendLine();

            // SECTION 2: SCENE ROOT
            // Every node defined after this with parent="." becomes a child of Labyrinth
            sb.AppendLine("[node name=\"Labyrinth\" type=\"Node3D\"]");
            sb.AppendLine();

            // SECTION 3: GLOBAL NODES
            sb.AppendLine("[node name=\"Player\" parent=\".\" instance=ExtResource(\"player\")]");
            sb.AppendLine("[node name=\"World\" parent=\".\" instance=ExtResource(\"world\")]");
            sb.AppendLine();

            // SECTION 4: LEVEL NODES
            levels.Finish();

            // SECTION 5: PERSISTENCE
            string GameFileName = "../../../Labyrinth3D.tscn";
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
