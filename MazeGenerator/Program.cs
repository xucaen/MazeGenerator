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
            int width = 11;  // Must be odd for the wall-path algorithm
            int height = 11; // Must be odd
            //char wallChar = 'X'; // Use '$' for version 1.3 style
            int seed = 2112;


            MazeBuilder maze = new MazeBuilder(width, height, 'x', seed);
            string Filenamebase = "mazeColor";

            ///Create a new maze
            // maze.Generate();
            // maze.SaveToFile(@$"../../../{Filenamebase}.dat");


            //string msg = @"Edit the maze.dat file to make any changes to the maze...";
            //Console.WriteLine(msg);
            //Console.ReadKey();


            //////////////////////////////////////////////////////////////
            /////convert maze to json
            MazeData md = new MazeData();
            var grid = md.ReadGridFile(@$"../../../{Filenamebase}.dat");
            md.SaveToJson(grid, @$"../../../{Filenamebase}.json");


            ///////////////////////////////////////////////////////////
            ///show the map in raylib and convert to json

            MazeCompiler mc = new MazeCompiler();

            mc.LoadFromJson(@$"../../../{Filenamebase}.json");
            mc.RunPreview();
            mc.ExportToGodot(@$"../../../{Filenamebase}.tscn");
        }
    }

}