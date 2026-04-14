using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MazeGenerator
{
    public class Maze3DGridToJSONConverter
    {
        public string Convert3DGridToJson(List<StringBuilder> grid)
        {

            List<Maze3DMetaData> Walls = new List<Maze3DMetaData>();
            List<Maze3DMetaData> Rooms = new List<Maze3DMetaData>();

            // Calculate dimensions from the list structure
            // Width (x) is the length of one StringBuilder
            int width = grid[0].Length;

            // Total number of rows in the list
            int totalRows = grid.Count;

            // Since the grid is flattened (z * height + y), 
            // and usually width, height, and depth are similar in these generators:
            // We assume height = width based on standard 3D grid conventions 
            // unless your specific implementation uses different ratios.
            int height = width;
            int depth = totalRows / height;

            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    int rowIndex = (z * height) + y;

                    // Safety check for calculated index
                    if (rowIndex >= grid.Count) continue;

                    StringBuilder currentRow = grid[rowIndex];

                    for (int x = 0; x < width; x++)
                    {
                        char cell = currentRow[x];

                        if (cell == ' ')
                        {
                            var room = new Maze3DMetaData(x, y, z, "Room");

                            bool hasAirBelow = (z > 0 && grid[((z - 1) * height) + y][x] == ' ');
                            bool hasAirAbove = (z < depth - 1 && grid[((z + 1) * height) + y][x] == ' ');
                            bool isStart = (x == 1 && y == 1 && z == 1);
                            bool isEnd = (x == width - 2 && y == height - 2 && z == depth - 2);

                            if (isStart) room.Type = "Start";
                            if (isEnd) room.Type = "End";
                            bool yes = false;

                            //  We only flag it as a "Passage" if it's the TOP of a hole.
                            // If there is air below, but solid ground above, it's a jump point.
                            if(isStart)
                            {
                                yes = false;
                            }
                            else if(isEnd)
                            {
                                yes = false;
                            }
                            else if (hasAirBelow)
                            {
                                yes = true;
                            }
                            else if (hasAirAbove)
                            {
                                yes = true;
                            }
                           
                            
                            
                            Console.WriteLine($"Room type = {room.Type}");
                           
                            room.IsVerticalPassage = yes;
                            room.IsLit = room.IsLit = yes || (Rooms.Count % 4 == 0);
                            Rooms.Add(room);
                        }
                        else
                        {
                            Walls.Add(new Maze3DMetaData(x, y, z, "Wall"));
                        }
                    }
                }
            }

            return JsonSerializer.Serialize(new { Walls, Rooms }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}