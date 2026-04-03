using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Parameters based on your "lab_big.conf" and "lab.conf" examples
            int width = 99;  // Must be odd for the wall-path algorithm
            int height = 99; // Must be odd
            //char wallChar = 'X'; // Use '$' for version 1.3 style
            int seed = 10;


            MazeBuilder maze = new MazeBuilder(width, height, 'x', seed);


            ///Create a new maze
            // maze.Generate();
            // maze.SaveToFile("../../../maze.dat");




            //////////////////////////////////////////////////////////////
            /////convert maze to json
            MazeData md = new MazeData();
            var grid = md.ReadGridFile("../../../maze.dat");
            md.SaveToJson(grid, "../../../maze.json");


            ///////////////////////////////////////////////////////////
            ///show the map in raylib and convert to json

            MazeCompiler mc = new MazeCompiler();

            mc.LoadFromJson("../../../maze.json");
            mc.RunPreview();
            mc.ExportToGodot("../../../maze.tscn");
        }
    }

}