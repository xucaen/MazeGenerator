using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    public class Maze3DMegastructureBuilder
    {
        private StringBuilder sb = new StringBuilder();
        private string helixFileName;
        private float helixHeight;

        public Maze3DMegastructureBuilder(string helixFileName, float finalHeight)
        {
            this.helixFileName = helixFileName.Replace($@"../../../", "");
            this.helixHeight = finalHeight;
        }

        public void Build(int level)
        {
            sb.Clear();
            sb.AppendLine("[gd_scene format=3]");
            sb.AppendLine();

            sb.AppendLine($@"[ext_resource type=""PackedScene"" path=""res://{helixFileName}"" id=""helix_scene""]");
            sb.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://alien_diamond.tscn"" id=""diamond""]");
            sb.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://alien_machine.tscn"" id=""machine""]");
            sb.AppendLine();

            sb.AppendLine($@"[node name=""Megastructure3D_Lvl{level}"" type=""Node3D""]");
            sb.AppendLine();

            sb.AppendLine(@"[node name=""MazeHelix"" parent=""."" instance=ExtResource(""helix_scene"")]");
            sb.AppendLine();

            // Place the Alien Diamond at the top of the 3D maze stack
            float diamondY = helixHeight + 100.0f;
            sb.AppendLine(@"[node name=""AlienDiamond"" parent=""."" instance=ExtResource(""diamond"")]");
            sb.AppendLine($@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, {diamondY}, 0)");
            sb.AppendLine();

            // Replace the machine loop to remove 'i' and spiral logic
            sb.AppendLine(@"[node name=""AlienMachine_Alpha"" parent=""."" instance=ExtResource(""machine"")]");
            sb.AppendLine(@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 50, 0, 50)");

            sb.AppendLine(@"[node name=""AlienMachine_Beta"" parent=""."" instance=ExtResource(""machine"")]");
            sb.AppendLine(@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, -50, 0, -50)");

            string fileName = $"../../../Megastructure3D_Lvl{level}.tscn";
            File.WriteAllText(fileName, sb.ToString());
        }
    }
}
