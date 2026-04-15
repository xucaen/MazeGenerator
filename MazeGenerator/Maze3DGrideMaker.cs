using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGenerator
{
    public class Maze3DGrideMaker
    {

        // Representing the 3D grid as a list of rows. 
        // Index = (z * _height) + y
        private List<StringBuilder> _grid;



        private readonly char _wall;
        private readonly char _path = ' ';
        private readonly Random _RandomGenerator;
        private bool[,,] _visited;

        public Maze3DGrideMaker(char wallChar, int seed)
        {
            _visited = new bool[(int)CubeDimensions.Length, (int)CubeDimensions.Height, (int)CubeDimensions.Width];
            _wall = wallChar;
            _RandomGenerator = new Random(seed);

            // Initialize Grid: depth * height = total number of rows
            _grid = new List<StringBuilder>((int)CubeDimensions.Width * (int)CubeDimensions.Height);


            for (int i = 0; i < CubeDimensions.Width * CubeDimensions.Height; i++)
            {
                // Each StringBuilder is one horizontal line (Width)
                _grid.Add(new StringBuilder(new string(_wall, (int)CubeDimensions.Length)));
            }

            // 2. Now, replace the logic that "sets" the initial state 
            // by calling your SetCell method for every coordinate:
            for (int z = 0; z < (int)CubeDimensions.Width; z++)
            {
                for (int y = 0; y < (int)CubeDimensions.Height; y++)
                {
                    for (int x = 0; x < (int)CubeDimensions.Length; x++)
                    {
                        SetCell(x, y, z, _wall);
                    }
                }
            }
        }

        // Helper to handle the math of mapping 3D coords to the List index
        private void SetCell(int x, int y, int z, char value)
        {
            Console.WriteLine($@"SetCell: {(value == _wall ? "WALL" : "ROOM")} ({x}, {y}, {z})");
            int rowIndex = (z * (int)CubeDimensions.Height) + y;
            var yyyyyy = _grid[rowIndex][x];

            _grid[rowIndex][x] = value;
        }

        public void Generate3DMaze(int startX, int startY, int startZ)
        {
            _visited = new bool[(int)CubeDimensions.Length, (int)CubeDimensions.Height, (int)CubeDimensions.Width];

            // 2. PROTECT THE ENTRANCE: 
            // Mark it visited NOW so the loops and Carve functions 
            // leave this specific spot alone.
            _visited[5, (int)CubeDimensions.Height - 1, 5] = true;


            int x = startX % 2 == 0 ? startX + 1 : startX;
            int y = startY % 2 == 0 ? startY + 1 : startY;
            int z = startZ % 2 == 0 ? startZ + 1 : startZ;

            Carve(x, y, z);

            for (int cz = 1; cz < CubeDimensions.Width; cz += 2)
            {
                for (int cy = 1; cy < CubeDimensions.Height; cy += 2)
                {
                    for (int cx = 1; cx < CubeDimensions.Length; cx += 2)
                    {
                        if (!_visited[cx, cy, cz])
                        {
                            // Instead of guessing where to punch, we just Carve.
                            // To make it connect, we ensure the carve STARTING POINT 
                            // is set to path, and we let it naturally hit an old path.
                            Carve(cx, cy, cz);

                            // After carving an island, we punch ONE wall to link it back
                            if (cx > 1) SetCell(cx - 1, cy, cz, _path);
                        }
                    }
                }
            }
            // Punch the hole at the very top
            Console.WriteLine(@$"Creating the ENTRANCE at {(5, (int)CubeDimensions.Height-1, 5)}");
            SetCell(5, (int)CubeDimensions.Height-1, 5, _path);




        }



        private void Carve(int x, int y, int z)
        {
            SetCell(x, y, z, _path);
            _visited[x, y, z] = true;

            var directions = new List<(int dx, int dy, int dz)>
            {
                (0, 1, 0), (0, -1, 0), (1, 0, 0), (-1, 0, 0), (0, 0, 1), (0, 0, -1)
            };

            // Shuffle
            for (int i = directions.Count - 1; i > 0; i--)
            {
                int k = _RandomGenerator.Next(i + 1);
                var temp = directions[i];
                directions[i] = directions[k];
                directions[k] = temp;
            }

            foreach (var d in directions)
            {
                int nx = x + (d.dx * 2);
                int ny = y + (d.dy * 2);
                int nz = z + (d.dz * 2);

                if (IsInBounds(nx, ny, nz) && !_visited[nx, ny, nz])
                {
                    if (d.dz == 0)
                    {
                        SetCell(x + d.dx, y + d.dy, z, _path);
                    }
                    else
                    {
                        SetCell(x, y, z + d.dz, _path);
                    }

                    Carve(nx, ny, nz);
                }
            }
        }

        private bool IsInBounds(int x, int y, int z)
        {
            return x > 0 && x < CubeDimensions.Length -1 &&
                   y > 0 && y < CubeDimensions.Height -1 &&
                   z > 0 && z < CubeDimensions.Width -1;
        }

        public List<StringBuilder> GetFullGrid() => _grid;

        internal void Print()
        {
            // 1. Loop through HEIGHT (Y) first. 
            // This ensures we print Level 0, Level 1, etc., up to the top.
            for (int y = 0; y < (int)CubeDimensions.Height; y++)
            {
                Console.WriteLine($"--- Vertical Level (Y): {y} ---");

                // 2. Inside each Level, we loop through the depth (Z)
                for (int z = 0; z < (int)CubeDimensions.Width; z++)
                {
                    // 3. FIX THE MATH: 
                    // We need to find the specific row in our List<StringBuilder>.
                    // Since SetCell uses (z * Height) + y, we must match that exactly.
                    int rowIndex = (z * (int)CubeDimensions.Height) + y;

                    if (rowIndex < _grid.Count)
                    {
                        Console.WriteLine(_grid[rowIndex].ToString());
                    }
                }
                // Add a blank line between levels for readability
                Console.WriteLine();
            }
        }


    }
}