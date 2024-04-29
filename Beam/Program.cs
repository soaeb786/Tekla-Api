using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing;
using System.Reflection;

namespace s1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Model model = new Model();
            if (model.GetConnectionStatus())
            {
                Point point = new Point();

                Beam beam = new Beam();
                beam.StartPoint = new Point(0, 0, 0);
                beam.EndPoint = new Point(3000, 0, 0);
                beam.Profile.ProfileString = "ISMB300";
                beam.Material.MaterialString = "IS2062";
                beam.Insert();
                beam.Name = "beam";
                model.CommitChanges();
            }
            Console.ReadLine();

        }
    }
}