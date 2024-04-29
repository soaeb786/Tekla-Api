using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Flange_Web_connection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CreateConnection();
        }
        public static void CreateConnection()
        {
            Model model = new Model();
            //// Connection trial 1
            if (model.GetConnectionStatus())
            {
                Picker picker = new Picker();
                Beam Column = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick a Column") as Beam;
                Beam Beam = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick a Beam") as Beam;
                CoordinateSystem localCoordinateSystem = FindMyCoordinateSystem(Column, Beam);
                //CoordinateSystem localCoordinateSystem = Column.GetCoordinateSystem();

                WorkPlaneHandler workPlaneHandler = model.GetWorkPlaneHandler();
                TransformationPlane currentTransformationPlane = workPlaneHandler.GetCurrentTransformationPlane();
                TransformationPlane desiredTransformationPlane = new TransformationPlane(localCoordinateSystem);
                workPlaneHandler.SetCurrentTransformationPlane(desiredTransformationPlane);
                try
                {
                    DrawCoordinateSystem("CDS");
                    double w = 140;
                    double h = 300;
                    ContourPoint point1 = new ContourPoint(new Point(w / 2, -h / 2, 0), null);
                    ContourPoint point2 = new ContourPoint(new Point(w / 2, h / 2, 0), null);
                    ContourPoint point3 = new ContourPoint(new Point(-w / 2, h / 2, 0), null);
                    ContourPoint point4 = new ContourPoint(new Point(-w / 2, -h / 2, 0), null);

                    ContourPlate CP = new ContourPlate();

                    CP.AddContourPoint(point1);
                    CP.AddContourPoint(point2);
                    CP.AddContourPoint(point3);
                    CP.AddContourPoint(point4);
                    CP.Finish = "FOO";
                    CP.Profile.ProfileString = "PLT10";
                    CP.Material.MaterialString = "Steel_Undefined";
                    CP.Position.Depth = Position.DepthEnum.FRONT;

                    bool Result = false;
                    Result = CP.Insert();

                    Plane fittingPlane = new Plane();
                    fittingPlane.Origin = new Point(0, 0, 10);
                    fittingPlane.AxisX = new Vector(1, 0, 0);
                    fittingPlane.AxisY = new Vector(0, 1, 0);


                    Fitting fitting = new Fitting();
                    fitting.Plane = fittingPlane;
                    fitting.Father = Beam;
                    fitting.Insert();


                    BoltArray bolt = new BoltArray();
                    bolt.PartToBeBolted = CP;
                    bolt.PartToBoltTo = Column;

                    bolt.FirstPosition = new Point(0, h / 2, 0);
                    bolt.SecondPosition = new Point(0, -h / 2, 0);

                    bolt.BoltSize = 12;
                    bolt.Tolerance = 3.00;
                    bolt.BoltStandard = "8.8XOX";
                    bolt.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                    bolt.CutLength = 100;

                    bolt.Length = 100;
                    bolt.ExtraLength = 50;
                    bolt.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

                    bolt.Position.Depth = Position.DepthEnum.MIDDLE;
                    bolt.Position.Plane = Position.PlaneEnum.MIDDLE;
                    bolt.Position.Rotation = Position.RotationEnum.FRONT;

                    bolt.Bolt = true;
                    bolt.Washer1 = true;
                    bolt.Washer2 = true;
                    bolt.Washer3 = true;
                    bolt.Nut1 = true;
                    bolt.Nut2 = true;

                    bolt.Hole1 = true;
                    bolt.Hole2 = true;
                    bolt.Hole3 = true;
                    bolt.Hole4 = true;
                    bolt.Hole5 = true;

                    bolt.StartPointOffset.Dx = 50;
                    bolt.AddBoltDistX(100);
                    bolt.AddBoltDistX(100);

                    bolt.AddBoltDistY(50);

                    if (!bolt.Insert())
                        Console.WriteLine("BoltArray Insert failed!");

                    model.CommitChanges();
                }
                finally
                {
                    workPlaneHandler.SetCurrentTransformationPlane(currentTransformationPlane);
                }
            }
            else
            {
                Console.WriteLine("Tekla is not connected");
            }
            Console.ReadLine();
        }

        private static void DrawCoordinateSystem(string v)
        {
            double value = 1000;
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            LineSegment xline = new LineSegment(new Point(), (new Point() + xVector * value));
            LineSegment yline = new LineSegment(new Point(), (new Point() + yVector * value));
            new GraphicsDrawer().DrawText(new Point(), v, new Color());
            new GraphicsDrawer().DrawLineSegment(xline.Point1, xline.Point2, new Color(1, 0, 0));
            new GraphicsDrawer().DrawText(xline.Point2, "X", new Color(1, 0, 0));
            new GraphicsDrawer().DrawLineSegment(yline.Point1, yline.Point2, new Color(0, 1, 0));
            new GraphicsDrawer().DrawText(yline.Point2, "Y", new Color(0, 1, 0));
        }

        public static CoordinateSystem FindMyCoordinateSystem(Beam Column, Beam beam)
        {
            bool isOnTheFlange = false;

            ///find centerLIne of Beam
            ArrayList beamPoints = beam.GetCenterLine(true);
            Point firstPt = beamPoints[0] as Point;
            Point secondpt = beamPoints[1] as Point;
            Line beamCenterLine = new Line(firstPt, secondpt);
            Vector beamDirection = beamCenterLine.Direction;
            Vector webDirection = Column.GetCoordinateSystem().AxisX.Cross(Column.GetCoordinateSystem().AxisY).GetNormal();
            Vector flangeDirection = Column.GetCoordinateSystem().AxisY.GetNormal();


            if (Math.Round(webDirection.GetAngleBetween(beamDirection), 2) == 3.14 / 2)
            {
                isOnTheFlange = true;
            }

            ///find web plane inside plane of column
            FaceEnumerator faceenumerator = Column.GetSolid().GetFaceEnumerator();
            List<Face> lstFace = new List<Face>();
            while (faceenumerator.MoveNext())
            {
                var current = faceenumerator.Current;
                lstFace.Add(current);
            }

            ///parallla to beam faces
            List<Face> parllaltoBeam = new List<Face>();
            foreach (Face face in lstFace)
            {
                if (Tekla.Structures.Geometry3d.Parallel.VectorToVector(isOnTheFlange ? flangeDirection : webDirection, face.Normal))
                {
                    parllaltoBeam.Add(face);
                }
            }
            Point clmcp = GetCenterOfPart(Column);
            Point bmcp = GetCenterOfPart(beam);
            Vector columntoBeam = new Vector(bmcp - clmcp);

            List<Face> threeFaceList = new List<Face>();
            foreach (Face face in parllaltoBeam)
            {
                if (face.Normal.Dot(columntoBeam) > 0)
                {
                    threeFaceList.Add(face);
                }
            }
            List<double> perimeters = new List<double>();
            foreach (Face item in threeFaceList)
            {
                double curretntpremeter = FindPerimeterofFace(item);
                perimeters.Add(curretntpremeter);
            }

            Face webFace = FindMaxFacePerimeter(perimeters, threeFaceList);
            List<Point> webFacePts = FindPointsofFace(webFace);
            GeometricPlane webgeometricPlane = new GeometricPlane();
            webgeometricPlane.Normal = webFace.Normal;
            webgeometricPlane.Origin = webFacePts.FirstOrDefault();

            Point IntersectionPoint = Intersection.LineToPlane(beamCenterLine, webgeometricPlane);
            new GraphicsDrawer().DrawText(IntersectionPoint, "<<<<====Origin", new Color());

            //CoordinateSystem coordinateSystem = new CoordinateSystem();
            //coordinateSystem.Origin = IntersectionPoint;
            //coordinateSystem.AxisX = Column.GetCoordinateSystem().AxisY;
            //coordinateSystem.AxisY = beam.GetCoordinateSystem().AxisY;

            CoordinateSystem coordinateSystem = new CoordinateSystem();
            coordinateSystem.Origin = IntersectionPoint;
            coordinateSystem.AxisX = Column.GetCoordinateSystem().AxisY;
            coordinateSystem.AxisY = Column.GetCoordinateSystem().AxisX;

            return coordinateSystem;
        }
        private static Face FindMaxFacePerimeter(List<double> perimeters, List<Face> threeFaceList)
        {
            double max = 0;
            int maxperimeterIndex = 0;
            for (int i = 0; i < perimeters.Count; i++)
            {
                if (perimeters[i] > max)
                {
                    max = perimeters[i];
                    maxperimeterIndex = i;
                }
            }
            return threeFaceList[maxperimeterIndex];
        }

        public static double FindPerimeterofFace(Face face)
        {
            double perimeter = 0;
            List<Point> mypoints = new List<Point>();
            mypoints = FindPointsofFace(face);
            perimeter = FindPerimeterFromPoint(mypoints);
            return perimeter;

        }

        private static List<Point> FindPointsofFace(Face face)
        {
            LoopEnumerator loopEnum = face.GetLoopEnumerator();
            Loop myloop = null;
            while (loopEnum.MoveNext())
            {
                myloop = loopEnum.Current;
            }
            VertexEnumerator myvertexEnum = myloop.GetVertexEnumerator();
            List<Point> mypoints = new List<Point>();
            while (myvertexEnum.MoveNext())
            {
                Point currentPt = myvertexEnum.Current;
                mypoints.Add(currentPt);
            }
            return mypoints;
        }

        private static double FindPerimeterFromPoint(List<Point> mypoints)
        {
            double myPertimeter = 0;
            for (int i = 0; i < mypoints.Count; i++)
            {
                Point pt1 = mypoints[i];
                int nextIndext = 0;
                if (i == mypoints.Count - 1)
                {
                    nextIndext = 0;
                }
                else
                {
                    nextIndext = i + 1;
                }
                Point pt2 = mypoints[nextIndext];
                double currentDistance = Distance.PointToPoint(pt1, pt2);
                myPertimeter = myPertimeter + currentDistance;
            }
            return myPertimeter;
        }

        public static Point GetCenterOfPart(Beam part)
        {
            Solid solid = part.GetSolid();
            Point pt1 = solid.MinimumPoint;
            Point pt2 = solid.MaximumPoint;

            Point resultpt = new Point();
            resultpt.X = 0.5 * (pt1.X + pt2.X);
            resultpt.Y = 0.5 * (pt1.Y + pt2.Y);
            resultpt.Z = 0.5 * (pt1.Z + pt2.Z);

            return resultpt;
        }
    }
}