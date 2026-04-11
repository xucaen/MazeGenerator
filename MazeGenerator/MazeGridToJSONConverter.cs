using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
namespace MazeGenerator
{

    public class MazeGridToJSONConverter
    {
        public string ConvertGridToJson(List<StringBuilder> grid)
        {
            List<MazeMetaData> Walls = new List<MazeMetaData>();
            List<MazeMetaData> Rooms = new List<MazeMetaData>();

            int height = grid.Count();
            int width = grid[0].Length;

            int stepsSinceLastTorch = 0;
            int torchInterval = 3;


            Point startPoint = new Point(-1, -1);
            Point endPoint = new Point(-1, -1);

            //using for loops for x and y coords
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char cell = grid[y][x];
                    // Check for 'x' or 'X' or whatever your wallChar is
                    ++stepsSinceLastTorch;
                    switch (char.ToLower(cell))
                    {
                        case 'x':
                        Walls.Add(new MazeMetaData(x, y, "Wall"));
                        break;
                        
                        case ' ':
                        MazeMetaData mmd = new MazeMetaData(x, y, "Rooms");
                        if (stepsSinceLastTorch > torchInterval)
                        {
                            mmd.IsLit = true;
                            stepsSinceLastTorch = 0;
                        }
                        Rooms.Add(mmd);
                        break;
                        
                        case 's':
                        startPoint = new Point(x, y);
                        Rooms.Add(new MazeMetaData(x, y, "Start"));
                        break;
                        
                        case 'e':
                        endPoint = new Point(x, y);
                        Rooms.Add(new MazeMetaData(x, y, "End"));
                        break;

                    }

     
                }
            }


            // Combine and Serialize into one JSON file
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(new
            {
                Walls = Walls.Select(p => new { p.X, p.Y, p.Type}),
                Rooms = Rooms.Select(p => new
                {
                    p.X,
                    p.Y,
                    p.IsLit,
                    Type = (p.X == startPoint.X && p.Y == startPoint.Y) ? "Start" :
                (p.X == endPoint.X && p.Y == endPoint.Y) ? "End" : "Floor"
                }),

            }, options);

            return jsonString;

        }

        public char[,] ReadGridFile(string gridFile)
        {
            // Read all lines from the file into an array of strings
            string[] lines = File.ReadAllLines(gridFile);

            if (lines.Length == 0)
            {
                throw new Exception("The maze file is empty.");
            }

            int height = lines.Length;       
            int width = lines[0].Length;    
            char[,] maze = new char[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Map the character from the string array to the 2D char array
                    maze[y, x] = lines[y][x];
                }
            }

            return maze;
        }
    }
}