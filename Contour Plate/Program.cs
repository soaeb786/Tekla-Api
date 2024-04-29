using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tekla.Structures.Geometry3d;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using System.Reflection;

namespace Trial27_8_
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Model model = new Model();

            //// Connection trial 1
            if (model.GetConnectionStatus())
            {
                //Picker picker = new Picker();
                //ModelObject Column = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick a Column");
                //ModelObject Beam = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick a Beam");

                //Beam beam_Column = Column as Beam;
                //Beam beam_Beam = Beam as Beam;


                ContourPoint point1 = new ContourPoint(new Point(-70, 0, 0), null);
                ContourPoint point2 = new ContourPoint(new Point(70, 0, 0), null);
                ContourPoint point3 = new ContourPoint(new Point(70, 300, 0), null);
                ContourPoint point4 = new ContourPoint(new Point(-70, 300, 0), null);

                ContourPlate CP = new ContourPlate();

                CP.AddContourPoint(point1);
                CP.AddContourPoint(point2);
                CP.AddContourPoint(point3);
                CP.AddContourPoint(point4);
                CP.Finish = "FOO";
                CP.Profile.ProfileString = "PLT10";
                CP.Material.MaterialString = "Steel_Undefined";

                bool Result = false;
                Result = CP.Insert();

                model.CommitChanges();

            }
            else
            {
                Console.WriteLine("Tekla is not connected");
            }
            Console.ReadLine();
        }

    }
}