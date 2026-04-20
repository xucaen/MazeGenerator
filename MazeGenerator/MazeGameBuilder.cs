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
            //final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://Vehicles/space_fighter_3w_lights.tscn"" id = ""space_fighter""]");
            //final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://Vehicles/cycle_2.tscn"" id = ""cycle""]");
            //final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://Vehicles/propeller_pack_2.tscn"" id = ""prop_pack""]");
            //final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://player.tscn"" id=""player""]");
            //final.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://world.tscn"" id=""world""]");

            // BUILDER RESOURCES (ALL of them together)
            final.Append(cubeBuilder.ResourceSB);
            final.Append(levelBuilder.ResourceSB);

            final.AppendLine();

            // ROOT NODE
            final.AppendLine(@"[node name=""Generated"" type=""Node3D""]");
            final.AppendLine();

            // 1. CALCULATE THE TOP OF THE MAZE
            // Since your Maze3DGrideMaker adds a top floor, the roof is at total height
            float roofY = CubeDimensions.Height * CubeDimensions.VerticalSize;

            // 2. DEFINE SPAWN MARKERS
//            // We use your existing height variables and ratios but turn them into Markers
//            var spawnConfig = new[] {
//    (Name: "PlayerSpawn", XRatio: 3.0f, ZRatio: 2.0f, HeightOffset: 0.5f), // Dropped from 1.0
//    (Name: "PropPackSpawn", XRatio: 3.0f, ZRatio: 3.0f, HeightOffset: 0.5f), // Dropped from 1.25
//    (Name: "CycleSpawn", XRatio: 3.0f, ZRatio: 4.0f, HeightOffset: 0.5f),    // Dropped from 1.5
//    (Name: "SpaceFighterSpawn", XRatio: 3.0f, ZRatio: 6.0f, HeightOffset: 1.0f) // Dropped from 7.0!
//};

//            foreach (var spawn in spawnConfig)
//            {
//                float x = (CubeDimensions.Width * CubeDimensions.HorizontalSize) / spawn.XRatio;
//                float z = (CubeDimensions.Length * CubeDimensions.HorizontalSize) / spawn.ZRatio;
//                float y = roofY + spawn.HeightOffset;

//                final.AppendLine($@"[node name=""{spawn.Name}"" type=""Marker3D"" parent="".""]");
//                final.AppendLine($"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {x}, {y}, {z})");
//                final.AppendLine();
//            }

//            // 1. Add the Player node
//            final.AppendLine(@"[node name=""Player"" parent=""."" instance=ExtResource(""player"")]");
//            final.AppendLine();

//            // 2. Add the Vehicle nodes (using the names expected by GameManager)
//            final.AppendLine(@"[node name=""Cycle"" parent=""."" instance=ExtResource(""cycle"")]");
//            final.AppendLine();

//            final.AppendLine(@"[node name=""SpaceFighter"" parent=""."" instance=ExtResource(""space_fighter"")]");
//            final.AppendLine();

//            final.AppendLine(@"[node name=""PropellerPack"" parent=""."" instance=ExtResource(""prop_pack"")]");
//            final.AppendLine();
//            // WORLD node
//            final.AppendLine(@"[node name=""Environment"" parent=""."" instance=ExtResource(""world"")]");
//            final.AppendLine();

//            // BUILDER NODES (ALL of them together)
            final.Append(cubeBuilder.NodeSB);
            final.Append(levelBuilder.NodeSB);

            // 4. SAVE
            string gameFileName = "../../../Generated.tscn";

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