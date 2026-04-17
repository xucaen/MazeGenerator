using Raylib_cs;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace MazeGenerator
{
    public class MazeGameBuilder
    {
        public void Build()
        {
            // 1. INIT BUILDERS (no shared buffers!)
            var cubeBuilder = new Maze3DLevelBuilder();
            var levelBuilder = new LevelBuilder();

            // 2. RUN BUILD PHASE (generate files + resources)
            int numberOfCubes = 1;
            for (int i = 0; i < numberOfCubes; i++)
            {
                cubeBuilder.Build(i);
            }

            cubeBuilder.Finish();

            levelBuilder.Build();
            levelBuilder.Finish();

            // 3. ASSEMBLE FINAL SCENE (STRICT ORDER)
            StringBuilder final = new StringBuilder();

            // HEADER
            final.AppendLine("[gd_scene format=3]");
            final.AppendLine();

            // BASE RESOURCES
            final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://Vehicles/space_fighter_3w_lights.tscn"" id = ""space_fighter""]");
            final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://player.tscn"" id=""player""]");
            final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://world.tscn"" id=""world""]");

            // BUILDER RESOURCES (ALL of them together)
            final.Append(cubeBuilder.ResourceSB);
            final.Append(levelBuilder.ResourceSB);

            final.AppendLine();

            // ROOT NODE
            final.AppendLine(@"[node name=""Labyrinth3D"" type=""Node3D""]");
            final.AppendLine();

            // CORE INSTANCES
            final.AppendLine(@"[node name=""SpaceFighter"" parent=""."" instance=ExtResource(""space_fighter"")]");
            final.AppendLine(@"transform=Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 10.045872, 129.33089, 17.900475)");
            final.AppendLine();
            final.AppendLine(@"[node name=""Player"" parent=""."" instance=ExtResource(""player"")]");
            final.AppendLine(@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 27, 127 , 37)");
            final.AppendLine(@"[node name=""Environment"" parent=""."" instance=ExtResource(""world"")]");
            final.AppendLine();

            // BUILDER NODES (ALL of them together)
            final.Append(cubeBuilder.NodeSB);
            final.Append(levelBuilder.NodeSB);

            // 4. SAVE
            string gameFileName = "../../../Labyrinth3D2.tscn";

            try
            {
                File.WriteAllText(gameFileName, final.ToString());
                Console.WriteLine($"Scene successfully generated at {gameFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }
}