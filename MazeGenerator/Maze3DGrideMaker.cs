using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGenerator
{
    public class Maze3DGrideMaker
    {
        private int _width;
        private int _height;
        private int _depth;

        // Representing the 3D grid as a list of rows. 
        // Index = (z * _height) + y
        private List<StringBuilder> _grid;



        private readonly char _wall;
        private readonly char _path = ' ';
        private readonly Random _rng;
        private bool[,,] _visited;

        public Maze3DGrideMaker(int width, int height, int depth, char wallChar, int seed)
        {
            _width = width % 2 == 0 ? width - 1 : width;
            _height = height % 2 == 0 ? height - 1 : height;
            _depth = depth % 2 == 0 ? depth : depth;

            _visited = new bool[_width, _height, _depth];
            _wall = wallChar;
            _rng = new Random(seed);

            // Initialize Grid: depth * height = total number of rows
            _grid = new List<StringBuilder>(_depth * _height);

            
            for (int i = 0; i < _depth * _height; i++)
            {
                // Each StringBuilder is one horizontal line (Width)
                _grid.Add(new StringBuilder(new string(_wall, _width)));
            }
        }

        // Helper to handle the math of mapping 3D coords to the List index
        private void SetCell(int x, int y, int z, char value)
        {
            int rowIndex = (z * _height) + y;
            _grid[rowIndex][x] = value;
        }

        public void Generate3DMaze(int startX, int startY, int startZ)
        {
            int x = startX % 2 == 0 ? startX + 1 : startX;
            int y = startY % 2 == 0 ? startY + 1 : startY;
            int z = startZ % 2 == 0 ? startZ + 1 : startZ;

            Carve(x, y, z);

            for (int cz = 1; cz < _depth; cz += 2)
            {
                for (int cy = 1; cy < _height; cy += 2)
                {
                    for (int cx = 1; cx < _width; cx += 2)
                    {
                        if (!_visited[cx, cy, cz]) Carve(cx, cy, cz);
                    }
                }
            }

            
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
                int k = _rng.Next(i + 1);
                var temp = directions[i];
                directions[i] = directions[k];
                directions[k] = temp;
            }

            foreach (var d in directions)
            {
                int nx = x + (d.dx * 2);
                int ny = y + (d.dy * 2);
                int nz = z + d.dz; // Z is carved step-by-step

                if (IsInBounds(nx, ny, nz) && !_visited[nx, ny, nz])
                {
                    if (d.dz == 0)
                        SetCell(x + d.dx, y + d.dy, z, _path);
                    else
                        SetCell(x, y, z + d.dz, _path);

                    Carve(nx, ny, nz);
                }
            }
        }

        private bool IsInBounds(int x, int y, int z)
        {
            return x > 0 && x < _width - 1 &&
                   y > 0 && y < _height - 1 &&
                   z > 0 && z < _depth - 1;
        }

        public List<StringBuilder> GetFullGrid() => _grid;

        internal void Print()
        {
            for (int z = 0; z < _depth; z++)
            {
                Console.WriteLine($"--- Layer {z} ---");
                for (int y = 0; y < _height; y++)
                {
                    int rowIndex = (z * _height) + y;
                    Console.WriteLine(_grid[rowIndex].ToString());
                }
            }
        }

        private char Get(int x, int y, int z)
        {
            return _grid[(z * _height) + y][x];
        }

        private void Set(int x, int y, int z, char value)
        {
            _grid[(z * _height) + y][x] = value;
        }
        private void CarveVertical()
        {
            for (int x = 1; x < _width - 1; x++)
            {
                for (int z = 1; z < _depth - 1; z++)
                {
                    for (int y = 1; y < _height - 1; y++)
                    {
                        if (Get(x, y, z) == _path)
                        {
                            Set(x, y, z, _path);
                        }
                    }
                }
            }
        }
    }
}