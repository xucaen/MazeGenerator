using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MazeGenerator
{
    public class Maze3DLevelBuilder
    {
        private StringBuilder sb;
        private List<int> builtCubes = new List<int>(); // Track cubes for Finish()

        public Maze3DLevelBuilder(StringBuilder sb)
        {
            this.sb = sb;
           
        }

        public void Build(int cubeNumber)
        {
            Console.WriteLine($"---> Building 3D Maze Cube {cubeNumber}...");
            string mazeFileName = $"../../../3DMaze_Cube_{cubeNumber}.tscn";
            builtCubes.Add(cubeNumber);

            // 1. GENERATE DATA
            var gridMaker = new Maze3DGrideMaker('x', 42 + cubeNumber);
            gridMaker.Generate3DMaze(1, 1, 1);



            gridMaker.Print();

            var converter = new Maze3DGridToJSONConverter();
            string jsonMaze = converter.Convert3DGridToJson(gridMaker.GetFullGrid());

            // 2. CONVERT TO GODOT SCENE
            var helper = new Maze3DSceneHelper();
            helper.LoadFromJson(jsonMaze);

            // Pass the cubeNumber so helper can modulo the Torch list
            var parts = helper.MakeGodotScene(cubeNumber);

            // 3. SAVE THE CUBE CONTENT
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



            // 4. REGISTER Resource in main scene
            sb.AppendLine($@"[ext_resource type=""PackedScene"" path=""res://3DMaze_Cube_{cubeNumber}.tscn"" id=""maze_cube_{cubeNumber}""]");
        }

        public void Finish()
        {
            float verticalStep = CubeDimensions.Height * CubeDimensions.VerticalSize;
            foreach (int id in builtCubes)
            {
                float yOffset = id * verticalStep;
                sb.AppendLine($@"[node name=""CubeInstance_{id}"" parent=""."" instance=ExtResource(""maze_cube_{id}"")]");
                sb.AppendLine($@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, {yOffset}, 0)");
                sb.AppendLine();
            }
        }
    }
}