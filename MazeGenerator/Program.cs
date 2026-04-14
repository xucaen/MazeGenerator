using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGenerator
{

    class Program
    {
        public static float horizontalSize = 5.0f; // This makes rooms 5m wide/deep
        public static float verticalSize = 12.0f;   // This makes rooms 12m tall


        static void Main(string[] args)
        {
            ///just maming a single level for now
            //a level contains a single Helix of mazes plus alien megastructures surrounding it.
           // GameBuilder game = new GameBuilder();
            //game.Build();


            Maze3DGameBuilder game3d = new Maze3DGameBuilder();
            game3d.Build();




        }

        public static void PrintToConsole(char[,,] _grid)
        {
            int width = _grid.GetLength(0);
            int height = _grid.GetLength(1);
            int depth = _grid.GetLength(2);

            for (int z = 0; z < depth; z++)
            {
                Console.WriteLine($"--- LAYER (Z): {z} ---");

                // We iterate through Y (rows) then X (columns)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        char cell = _grid[x, y, z];
                        // Using a block character for walls makes it easier to read
                        if (cell == 'x' || cell == 'X')
                            Console.Write("█");
                        else
                            Console.Write(" ");
                    }
                    Console.WriteLine(); // New line after each row
                }
                Console.WriteLine(); // Gap between layers
            }
        }
    }

}