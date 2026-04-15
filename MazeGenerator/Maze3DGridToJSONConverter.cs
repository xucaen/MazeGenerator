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

            int totalRows = grid.Count;


            for (int z = 0; z < (int)CubeDimensions.Width; z++)
            {
                for (int y = 0; y < (int)CubeDimensions.Height; y++)
                {
                    int rowIndex = (z * (int)CubeDimensions.Height) + y;

                    // Safety check for calculated index
                    if (rowIndex >= grid.Count) continue;

                    StringBuilder currentRow = grid[rowIndex];

                    for (int x = 0; x < (int)CubeDimensions.Length; x++)
                    {
                        char cell = currentRow[x];

                        if (cell == ' ')
                        {
                            var room = new Maze3DMetaData(x, y, z, "Room");

                            // 1. Detect Surrounding Air
                            room.HasAirBelow = (z > 0 && grid[((z - 1) * (int)CubeDimensions.Height) + y][x] == ' ');
                            room.HasAirAbove = (z < (int)CubeDimensions.Width - 1 && grid[((z + 1) * (int)CubeDimensions.Height) + y][x] == ' ');

                            // 3. MERGED FLOOR LOGIC
                            // We delete the floor if:
                            // - It's the Entrance (so player falls in)
                            // - It's a vertical shaft (air above or below)
                            bool isVerticalPassage =  room.HasAirBelow || room.HasAirAbove;

                            // 4. Lighting
                            // Light the entrance and shafts so the player can see the drops
                            room.IsLit = (Rooms.Count % 4 == 0);

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