using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MazeGenerator
{
    public class Maze3DLevelBuilder
    {
        public StringBuilder ResourceSB { get; } = new StringBuilder();
        public StringBuilder NodeSB { get; } = new StringBuilder();

        private List<int> builtCubes = new List<int>();

        public Maze3DLevelBuilder() { }

        public void Build(int cubeNumber)
        {
            Console.WriteLine($"---> Building 3D Maze Cube {cubeNumber}...");
            string mazeFileName = $"../../../3DMaze_Cube_{cubeNumber}.tscn";

            builtCubes.Add(cubeNumber);

            // 1. GENERATE DATA
            var cubeMaker = new Maze3DGrideMaker('x', 42 + cubeNumber);
            cubeMaker.Generate3DMaze();


            var converter = new Maze3DGridToJSONConverter();
            string jsonMaze = converter.Convert3DGridToJson(cubeMaker.GetFullGrid());

            // 2. CONVERT TO GODOT SCENE
            var helper = new Maze3DSceneHelper();
            helper.LoadFromJson(jsonMaze);

            var parts = helper.MakeGodotScene(cubeNumber);

            // 3. SAVE INDIVIDUAL CUBE FILE
            StringBuilder mazeContent = new StringBuilder();
            mazeContent.AppendLine("[gd_scene format=3]");
            mazeContent.Append(parts.Resources);
            mazeContent.AppendLine();

            mazeContent.AppendLine($@"[node name=""Maze_Cube_{cubeNumber}"" type=""Node3D""]");
            mazeContent.AppendLine(@"[node name=""MazeSection"" type=""Node3D"" parent="".""]");
            mazeContent.AppendLine(@"[node name=""Walls"" type=""Node3D"" parent=""MazeSection""]");
            mazeContent.AppendLine(@"[node name=""Rooms"" type=""Node3D"" parent=""MazeSection""]");
            mazeContent.AppendLine(@"[node name=""Torches"" type=""Node3D"" parent=""MazeSection""]");

            mazeContent.Append(parts.Nodes);

            File.WriteAllText(mazeFileName, mazeContent.ToString());

            // 4. REGISTER RESOURCE (ONLY here, and ONLY to ResourceSB)
            ResourceSB.AppendLine(
                $@"[ext_resource type=""PackedScene"" path=""res://3DMaze_Cube_{cubeNumber}.tscn"" id=""maze_cube_{cubeNumber}""]"
            );
        }

        public void Finish()
        {
            float verticalStep = CubeDimensions.Height * CubeDimensions.VerticalSize;

            foreach (int id in builtCubes)
            {
                float yOffset = id * verticalStep;

                // ONLY nodes go here
                NodeSB.AppendLine(
                    $@"[node name=""CubeInstance_{id}"" parent=""."" instance=ExtResource(""maze_cube_{id}"")]"
                );

                NodeSB.AppendLine(
                    $@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, {yOffset}, 0)"
                );

                NodeSB.AppendLine();
            }
        }
    }
}