using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
namespace MazeGenerator
{

    public class MazeData
    {
        public void SaveToJson(char[,] grid, string filePath)
        {
            List<Point> Walls = new List<Point>();
            List<Point> Rooms = new List<Point>();


            // grid.GetLength(0) is Height, GetLength(1) is Width
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char cell = grid[y, x];
                    // Check for 'x' or 'X' or whatever your wallChar is
                    if (char.ToLower(cell) == 'x')
                    {
                        Walls.Add(new Point(x, y));
                    }
                    else
                    {
                        Rooms.Add(new Point(x, y));
                    }
                }
            }
           


            // Combine and Serialize into one JSON file
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(new
            {
                Walls = Walls.Select(p => new { p.X, p.Y }),
                Rooms = Rooms.Select(p => new { p.X, p.Y })
            }, options);

            File.WriteAllText(filePath, jsonString);

        }

        public void SaveToGrid(char[,] grid)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);
            string filePath = @"../../../maze.dat";
            // Use StringBuilder to accumulate the maze text for the file
            StringBuilder sb = new StringBuilder();


            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Check the character and print X for walls, O for rooms
                    sb.Append(char.ToLower(grid[y, x]));
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        public char[,] ReadGridFile(string gridFile)
        {
            // Read all lines from the file into an array of strings
            string[] lines = File.ReadAllLines(gridFile);

            if (lines.Length == 0)
            {
                throw new Exception("The maze file is empty.");
            }

            int height = lines.Length;       // 99
            int width = lines[0].Length;    // 99
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