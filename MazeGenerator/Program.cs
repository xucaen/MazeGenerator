using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            ///just maming a single level for now
            //a level contains a single Helix of mazes plus alien megastructures surrounding it.
            GameBuilder game = new GameBuilder();
            game.Build();
            
        }
    }

}