using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MazeGenerator
{
    public class Maze3DGridToJSONConverter
    {
        public string Convert3DGridToJson(List<List<StringBuilder>> cube)
        {
            List<Maze3DMetaData> Walls = new List<Maze3DMetaData>();
            List<Maze3DMetaData> Rooms = new List<Maze3DMetaData>();

            // Using the nested structure:
            // y = Floor (Height)
            // z = Row (Width/Depth)
            // x = Column (Length)

            for (int floorIdx = 0; floorIdx < cube.Count; floorIdx++) // Iterating through Floors
            {
                List<StringBuilder> currentFloor = cube[floorIdx];

                for (int rowIdx = 0; rowIdx < currentFloor.Count; rowIdx++) // Iterating through Rows
                {
                    StringBuilder currentRow = currentFloor[rowIdx];

                    for (int c = 0; c < currentRow.Length; c++) // Iterating through Characters
                    {
                        char cellValue = currentRow[c];

                        if (cellValue == ' ') // It's a Room/Air
                        {
                            var room = new Maze3DMetaData(c, floorIdx, rowIdx, "Room");

                            // 1. Detect Surrounding Air (Vertical check)
                            // Check the floor below (y - 1)
                            room.HasAirBelow = (floorIdx > 0 && cube[floorIdx - 1][rowIdx][c] == ' ');

                            // Check the floor above (y + 1)
                            room.HasAirAbove = (floorIdx < cube.Count - 1 && cube[floorIdx + 1][rowIdx][c] == ' ');

                            // 3. MERGED FLOOR LOGIC
                            bool isVerticalPassage = room.HasAirBelow || room.HasAirAbove;
                            // Note: You can add specific logic here to flag 'isVerticalPassage' in your metadata if needed.

                            // 4. Lighting
                            room.IsLit = (Rooms.Count % 4 == 0);

                            Rooms.Add(room);
                        }
                        else // It's a Wall
                        {
                            Walls.Add(new Maze3DMetaData(c, floorIdx, rowIdx, "Wall"));
                        }
                    }
                }
            }

            return JsonSerializer.Serialize(new { Walls, Rooms }, new JsonSerializerOptions { WriteIndented = true });
        }

    }
}