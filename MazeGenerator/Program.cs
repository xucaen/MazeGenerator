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
            int width = 9;  // Must be odd for the wall-path algorithm
            int height = 9; // Must be odd, 
            //char wallChar = 'X'; // Use '$' for version 1.3 style
            int seed = 1;
            List<string> levelNames = new List<string>();

            for (int s = seed; s < seed + 30; ++s)
            {

                string Filenamebase = $"MazeLevel{s}";

                ///Create a new maze
                bool indicateStartEndCells = true;
                MazeBuilder maze = new MazeBuilder(width, height, 'x', s, indicateStartEndCells);
                maze.Generate();
                maze.MakeMoreRooms();
                maze.MakeObservationDeck();
                maze.SaveToFile(@$"../../../{Filenamebase}.dat");


                //string msg = @"Edit the maze.dat file to make any changes to the maze...";
                //Console.WriteLine(msg);
                //Console.ReadKey();


                //////////////////////////////////////////////////////////////
                /////convert maze to json
                MazeGridToJSONConverter md = new MazeGridToJSONConverter();
                var grid = maze.GetFullGrid();
                md.SaveToJson(grid, @$"../../../{Filenamebase}.json");


                ///////////////////////////////////////////////////////////
                ///show the map in raylib and convert to json

                MazeCompiler mc = new MazeCompiler();

                mc.LoadFromJson(@$"../../../{Filenamebase}.json");
                //  mc.RunPreview();
                levelNames.Add(mc.ExportToGodot(@$"../../../{Filenamebase}.tscn", s));

                if(File.Exists(@$"../../../{Filenamebase}.tscn"))
                {
                    File.Delete(@$"../../../{Filenamebase}.json");
                    File.Delete(@$"../../../{Filenamebase}.dat");
                }


            }


            ///Now make the world

            //TODO: let worldBuilder build the  main_world_node.tscn
            if (levelNames != null)
            {

                //TOdo: generate a scifi megastructure world
                MazeHelixBuilder world = new MazeHelixBuilder();
                world.Init();
                world.AddLevels(levelNames);
                world.Finish();
            }
        }
    }

}