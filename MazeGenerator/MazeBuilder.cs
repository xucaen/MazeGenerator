using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGenerator
{

    public class MazeBuilder
    {
   
        // Also helpful to add these so LevelGenerator doesn't have to hardcode 801/601
        public int Width => _width;
        public int Height => _height;

        private readonly int _width;
        private readonly int _height;
        private readonly char[,] _grid;
        private readonly char _wall;
        private readonly char _path = ' ';
        private readonly Random _rng;

        public char[,] GetFullGrid()
        {
            return _grid;
        }

        public MazeBuilder(int width, int height, char wallChar, int seed)
        {
            _width = width;
            _height = height;
            _wall = wallChar;
            _grid = new char[height, width];
            _rng = new Random(seed);

            // Initialize with walls
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    _grid[y, x] = _wall;
        }

        public void Generate()
        {

            // Instead of recursion, we use a manual Stack on the heap
            GenerateIterative(1, 1);

        }


        public void SaveToFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    for (int y = 0; y < _height; y++)
                    {
                        // Create a temporary buffer for the row to write it all at once
                        char[] row = new char[_width];
                        for (int x = 0; x < _width; x++)
                        {
                            row[x] = _grid[y, x];
                        }
                        writer.WriteLine(row);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred while saving the maze: {ex.Message}");
            }
        }


        private void GenerateIterative(int startX, int startY)
        {
            Stack<(int x, int y)> history = new Stack<(int x, int y)>();

            // Mark starting cell as path and push to stack
            _grid[startY, startX] = _path;
            history.Push((startX, startY));

            while (history.Count > 0)
            {
                var (currX, currY) = history.Peek();

                // Find unvisited neighbors 2 spaces away
                var neighbors = GetUnvisitedNeighbors(currX, currY);

                if (neighbors.Count > 0)
                {
                    // Pick a random direction
                    var (nextX, nextY, dx, dy) = neighbors[_rng.Next(neighbors.Count)];

                    // Remove wall between current and neighbor
                    _grid[currY + (dy / 2), currX + (dx / 2)] = _path;

                    // Mark neighbor as path
                    _grid[nextY, nextX] = _path;

                    // Move to neighbor
                    history.Push((nextX, nextY));
                }
                else
                {
                    // Backtrack
                    history.Pop();
                }
            }
        }

        private List<(int nx, int ny, int dx, int dy)> GetUnvisitedNeighbors(int x, int y)
        {
            var results = new List<(int nx, int ny, int dx, int dy)>();

            // Directions: Up, Down, Left, Right (2 units away)
            int[] dxs = { 0, 0, -2, 2 };
            int[] dys = { -2, 2, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dxs[i];
                int ny = y + dys[i];

                if (IsInBounds(nx, ny) && _grid[ny, nx] == _wall)
                {
                    results.Add((nx, ny, dxs[i], dys[i]));
                }
            }
            return results;
        }

        private bool IsInBounds(int x, int y) => x > 0 && x < _width - 1 && y > 0 && y < _height - 1;

    }
}