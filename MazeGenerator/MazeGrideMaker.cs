using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MazeGenerator
{

    public class MazeGrideMaker
    {

        // Also helpful to add these so LevelGenerator doesn't have to hardcode 801/601
        public int Width => _width;
        public int Height => _height;

        private int _width;
        private int _height;



        private readonly List<StringBuilder> _grid;
        private readonly char _wall;
        private readonly char _path = ' ';

        private readonly char _start = 's';
        private readonly char _end = 'e';

        private readonly Random _rng;

        private bool _indicateStartEndCells;
        public List<StringBuilder> GetFullGrid()
        {
            return _grid;
        }

        public MazeGrideMaker(int width, int height, char wallChar, int seed, bool indicateStartEndCells)
        {
            _width = width;
            _height = height;
            _wall = wallChar;
            _grid = new List<StringBuilder>();
            _rng = new Random(seed);
            _indicateStartEndCells = indicateStartEndCells;

            // Initialize with walls
            for (int y = 0; y < _height; y++)
            {
                _grid.Add(new StringBuilder(new string(_wall, _width)));
            }
        }

        public void Generate()
        {

            // Instead of recursion, we use a manual Stack on the heap
            GenerateIterative(1, 1);

        }


        private void GenerateIterative(int startX, int startY)
        {
            Stack<(int x, int y)> history = new Stack<(int x, int y)>();
            (int x, int y) endPoint = (startX, startY);
            int maxPathLength = 0;

            // Mark starting cell as start and push to stack
            _grid[startY][startX] = _indicateStartEndCells ? _start : _path;
            history.Push((startX, startY));


            while (history.Count > 0)
            {
                var (currX, currY) = history.Peek();


                // Track the furthest point reached from the start
                if (history.Count > maxPathLength)
                {
                    maxPathLength = history.Count;
                    endPoint = (currX, currY);
                }

                // Find unvisited neighbors 2 spaces away
                var neighbors = GetUnvisitedNeighbors(currX, currY);

                if (neighbors.Count > 0)
                {
                    // Pick a random direction
                    var (nextX, nextY, dx, dy) = neighbors[_rng.Next(neighbors.Count)];
                    // Remove wall between current and neighbor
                    _grid[currY + (dy / 2)][currX + (dx / 2)] = _path;
                    // Mark neighbor as path
                    _grid[nextY][nextX] = _path;

                    // Move to neighbor
                    history.Push((nextX, nextY));
                }
                else
                {
                    // Backtrack
                    history.Pop();
                }
            }

            //mark the end point
            if (_indicateStartEndCells)
            {
                _grid[endPoint.y][endPoint.x] = _end;
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

                if (IsInBounds(nx, ny) && _grid[ny][nx] == _wall)
                {
                    results.Add((nx, ny, dxs[i], dys[i]));
                }
            }
            return results;
        }

        private bool IsInBounds(int x, int y) => x > 0 && x < _width - 1 && y > 0 && y < _height - 1;

        public void MakeMoreRooms()
        {
            bool indicateStartEndCells = false;
            MazeGrideMaker tmp = new MazeGrideMaker(Width, Height, _wall, _rng.Next(), indicateStartEndCells);
            tmp.Generate();
            var secondGrid = tmp.GetFullGrid();
            // 3. Overlay the second maze onto the primary _grid
            // We skip the outer boundaries to respect the wall boundaries
            for (int y = 1; y < _height - 1; y++)
            {
                for (int x = 1; x < _width - 1; x++)
                {
                    char c = secondGrid[y][x];
                    if (secondGrid[y][x] == _path)
                    {
                        //only overlay wall chacaters
                        char tmp_char = _grid[y][x];
                        if (tmp_char == _wall)
                        {
                            _grid[y][x] = _path;
                        }
                    }
                }
            }
        }

        public void MakeObservationDeck()
        {
            //observation deck should be two rows os _space beneath the wall
            //this requires changing the structure of the grid, which is allowed since this is a maze builder.
            //1. space out three centermost walls at the bottom of the grid
            //2. then add 2 rows of _space beneath the wall
            // 1. Find the center at the bottom wall
            int midX = _width / 2;
            int bottomY = _height - 1;

            // 2. Open "windows" in the bottom wall
            // Using the StringBuilder indexer directly: _grid[y][x]
            _grid[bottomY][midX - 1] = _path;
            _grid[bottomY][midX] = _path;
            _grid[bottomY][midX + 1] = _path;

            // 3. Add 2 new rows of space beneath the wall
            // This is where List shines over char[,]
            for (int i = 0; i < 2; i++)
            {
                _grid.Add(new StringBuilder(new string(_path, _width)));
            }

            // Update height to reflect the new rows
            _height = _grid.Count;

        }



    }
}