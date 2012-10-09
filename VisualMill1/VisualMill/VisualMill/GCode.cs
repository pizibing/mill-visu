using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;


namespace VisualMill
{
    class GCode
    {
        //public enum eGCmd
        //{
        //    G00_Rapid = 0,
        //    G01_Move = 1,
        //    G02_CW = 2,
        //    G03_CCW = 3,
        //    G90_ABS = 90,
        //    G91_INC = 91
        //}
        //public struct tGData
        //{
        //    public int g;
        //    public double x;
        //    public double y;
        //    public double z;
        //    public double q;
        //    public double w;
        //    public double a;
        //    public double b;
        //    public double c;
        //}

        //public List<tGData> g_sGData;

        public GCode()
        {      
        }

        public Vector3 Max, Min;

        public List<Vector3> ImportGCodeVector3(string filename)
        {
            Max = new Vector3();
            Min = new Vector3();

            if (!System.IO.File.Exists(filename))
            {
                throw new Exception("Error reading Image Files");
            } 

            System.IO.StreamReader sr = new System.IO.StreamReader(filename);
            Regex mRegSubs = new Regex("\\([^\\(\\)]*\\)|[/\\*].*\\n|\\n|[A-Z][-+]?[0-9]*\\.?[0-9]*");

            List<Vector3> Vectors = new List<Vector3>();
            Vector3 CurrentVector = new Vector3();

            foreach (Match ma in mRegSubs.Matches(sr.ReadToEnd()))
            {
                sr.Close();
            
                string line = ma.ToString();
                int start = Convert.ToChar(line.Substring(0, 1));

                switch (start)
                {
                    case 'X':
                        CurrentVector.X = Convert.ToSingle(ma.ToString().Substring(1).Replace('.', ','));
                        break;
                    case 'Y':
                        CurrentVector.Y = Convert.ToSingle(ma.ToString().Substring(1).Replace('.', ','));
                        break;
                    case 'Z':
                        CurrentVector.Z = Convert.ToSingle(ma.ToString().Substring(1).Replace('.', ','));
                        break;
                    case '\n':

                        Vectors.Add(CurrentVector);
                        break;
                    case '(':
                        //Comment
                        break;

                }
            }
            return Vectors;
        }

//        public static void CreateMeasurePathNC(List<Vector3> MeasureVectors, List<Vector3> BridgePoints)
//        {
//            System.IO.StreamWriter NCFile = new System.IO.StreamWriter(Properties.Settings.Default.DestFilename, false);

//            //List<Vector3> MeasurePath = new List<Vector3>();
//            NCFile.Write(GCode.CreateMeasureHeader(new Vector3(451.5, 182, -162), 1000, Properties.Settings.Default.MeasureDist, 10000));

//            //NCFile.Write(GCode.CreateMessCycle(MeasureVectors[0], MeasureVectors[1], 100));
//            //anfahren
//            NCFile.Write(CreateG01(MeasureVectors[0]));

//            for (int VectorIndex = 2; VectorIndex < MeasureVectors.Count-2; VectorIndex += 2)
//            {
//                NCFile.Write(GCode.CreatePositionPath(BridgePoints.GetRange(VectorIndex / 2 - 1, 1)));
//                NCFile.Write(GCode.CreateMessCycle(MeasureVectors[VectorIndex], MeasureVectors[VectorIndex + 1], 100 + VectorIndex / 2));
//            }


//            //NCFile.Write(CreateG01(MeasureVectors[0]));

//            NCFile.Write(GCode.CreateMeasureEnd());

//            NCFile.Flush();
//            NCFile.Close();
//        }
//        public static string CreateMeasureHeader(Vector3 BasisOffset, int MeasureSpeed, double MeasureDistance, int GOOSpeed)
//        {
//            StringBuilder CodeString = new StringBuilder();
//            string xStart, yStart, zStart;
          
//            xStart = BasisOffset.X.ToString("F3").Replace(',', '.');
//            yStart = BasisOffset.Y.ToString("F3").Replace(',', '.');
//            zStart = BasisOffset.Z.ToString("F3").Replace(',', '.');

//            CodeString.AppendFormat("!Makro Datei ; Erzeugt am 19.07.12 - 16:13 V9.64B!\r\n");
//            CodeString.AppendFormat("!!\r\n");
//            CodeString.AppendFormat("\r\n");
//            CodeString.AppendFormat("_sprache 0;\r\n");
//            CodeString.AppendFormat("\r\n");
//            CodeString.AppendFormat("Dimension 1;\r\n");
//            CodeString.AppendFormat("\r\n");
//            CodeString.AppendFormat("Mservice 1, 35, 2, 0, 0, 0, 0, 0, 0;\r\n");

//            CodeString.AppendFormat("_N (**************************** INIT BLOCK *****************************)\r\n"); 
//            CodeString.AppendFormat("_N M112            ( Mess activate          )\r\n");
//            CodeString.AppendFormat("_N P1 = {0}        ( Measure Speed F        )\r\n", MeasureSpeed.ToString());
//            CodeString.AppendFormat("_N P2 = {0}        ( Measure Distance       )\r\n", MeasureDistance.ToString("f3").Replace(',', '.'));
//            CodeString.AppendFormat("_N P3 = {0}        ( G00 Speed F            )\r\n", GOOSpeed.ToString());
//            CodeString.AppendFormat("_N G92 X{0} Y{1} Z{2}   (Start Offset Measuring  )\r\n", xStart, yStart, zStart);
//            CodeString.AppendFormat("_N (************************ END INIT BLOCK *****************************)\r\n");
  
//            CodeString.AppendFormat("_N G90\r\n");
//            CodeString.AppendFormat("_N G17\r\n");
//            CodeString.AppendFormat("_N #SET SPLINETYPE AKIMA\r\n");
//            CodeString.AppendFormat("_N #SET ASPLINE MODE[2, 2]\r\n");

//            CodeString.AppendFormat("_N M108 (Mess down)\r\n");
//            CodeString.AppendFormat("_N M112 (Mess activate)\r\n");
            

//            return CodeString.ToString();
//        }

//        public static string CreateMeasureEnd()
//        {
//            StringBuilder CodeString = new StringBuilder();
//            //CodeString.AppendFormat("_N G91 \r\n");
//            CodeString.AppendFormat("_N G00 X0 Y0 Z50 \r\n");
//            //CodeString.AppendFormat("_N G90 \r\n");    
//            CodeString.AppendFormat("_N M113 (Mess ddeactivate)\r\n");
//            CodeString.AppendFormat("_N M109 (Mess up)\r\n");
//            return CodeString.ToString();
//        }
  
//        public static string CreateMessCycle(Vector3 StartPoint, Vector3 EndPoint, int MeasurePointNumber)
//        {
//            StringBuilder CodeString = new StringBuilder();
//            string xStart, yStart, zStart;
//            string xEnd, yEnd, zEnd;

//            xStart= StartPoint.X.ToString("F3").Replace(',', '.');
//            yStart=StartPoint.Y.ToString("F3").Replace(',', '.');
//            zStart= StartPoint.Z.ToString("F3").Replace(',', '.');

//            xEnd= EndPoint.X.ToString("F3").Replace(',', '.');
//            yEnd = EndPoint.Y.ToString("F3").Replace(',', '.');
//            zEnd = EndPoint.Z.ToString("F3").Replace(',', '.');

//            CodeString.AppendFormat("_N (*********************** MEASURE CYCLE{0} *****************************)\r\n", MeasurePointNumber.ToString());
//            CodeString.AppendFormat("_N G01 X{0} Y{1} Z{2} FP3      (Backmove)\r\n", xStart, yStart, zStart);
//            CodeString.AppendFormat("_N G100 X{0} Y{1} Z{2} F1000     (StartMeasure)\r\n", xEnd, yEnd, zEnd);
//            //CodeString.AppendFormat("_N P{3} = SQRT[SQR[V.A.MESS.X - {0}] + SQR[V.A.MESS.Y - {1}] + SQR[V.A.MESS.Z - {2}]] - P2  (Calc error Sum)\r\n", xStart, yStart, zStart, MeasurePointNumber.ToString());
//            CodeString.AppendFormat("_N G01 X{0} Y{1} Z{2} FP3      (Backmove)\r\n", xStart, yStart, zStart);

//            return CodeString.ToString();
//        }

//        public static string CreatePositionPath(List<Vector3> Path)
//        {
//            StringBuilder CodeString = new StringBuilder();
//            string xPos, yPos, zPos;

//            CodeString.AppendFormat("_N G151                (Spline ON       )\r\n");
//            foreach (Vector3 Position in Path)
//            {
//                xPos = Position.X.ToString("F3").Replace(',', '.');
//                yPos = Position.Y.ToString("F3").Replace(',', '.');
//                zPos = Position.Z.ToString("F3").Replace(',', '.');
//                CodeString.AppendFormat("_N G01 X{0} Y{1} Z{2} FP3\r\n", xPos, yPos, zPos);
//            }

//            CodeString.AppendFormat("_N G150                (Spline OFF      )\r\n");        
//            return CodeString.ToString();
//        }

//        public static string CreateG01(Vector3 Path)
//        {
//            StringBuilder CodeString = new StringBuilder();
//            string xPos, yPos, zPos;

//            xPos = Path.X.ToString("F3").Replace(',', '.');
//            yPos = Path.Y.ToString("F3").Replace(',', '.');
//            zPos = Path.Z.ToString("F3").Replace(',', '.');
//            CodeString.AppendFormat("_N G01 X{0} Y{1} Z{2} FP3\r\n", xPos, yPos, zPos);
      
//            return CodeString.ToString();
//        }

//        public static string CreateMCRfromVector(List<Vector3> Path)
//        {
//            StringBuilder CodeString = new StringBuilder();
//            string X,Y,Z;
//            //Axyz 0 ,5.925 ,9.94201 ,.95741 , 0 , 0 ;
//            foreach (Vector3 CurrentPos in Path)
//            {
//                X = CurrentPos.X.ToString("f5").Replace(',', '.');
//                Y = CurrentPos.Y.ToString("f5").Replace(',', '.');
//                Z = CurrentPos.Z.ToString("f5").Replace(',', '.');
//                CodeString.AppendFormat("Axyz 0, {0}, {1}, {2}, 0 ,0 ;\r\n", X,Y,Z);
//            }
//            return CodeString.ToString();
//        }

//        public static string CreateMCRHeader()
//        {
//            return @"!Makro Datei ; Erzeugt am 19.07.12 - 09:17 V9.64B!
//!Makroprojekt1 Beschreibung!
//
//_sprache 0;
//
//Dimension 1;
//
//Variable Test, $Test, Px, Yweg, Ymess;
//" + CreateKommentsSubmakro(Properties.Settings.Default.StatusText) + 
//@"; !Test;!
//Glaettung_hsc 1, 0.03, 0.03, 5, 45;
//; !Spindel ein!
//Drehzahl 1, 30, 0, 30;
//Position 23, 2;
//Axyz 1, Xp, Yp, 70, 0, 0;
//Axyz 1, 64, 10, Zp, 0, 0;
//Axyz 1, 64, 10, 20, 0, 0;
//Fkomp 1, 0.5, 0, 1, 1;
//Vorschub 2, 2, 2, 2;
//";
//        }

//        public static string CreateMCREnd()
//        {
//            return @"Axyz 1, -64, 10, 70, 0, 0;
//Axyz 1, -64, 30, 70, 0, 0;
//Fkomp 0, 0, 0, 1, 1;
//; !Spindel aus#!
//; Drehzahl 5, 30, 0, 30;
//";
//        }

//        public static string CreateKommentsSubmakro(string Text)
//        {
//            Text = "Smakros Description;\r\n\r\n(\r\n ; !" + Text.Replace("\n", "!\r\n  ; !"); ;
         
//            Text += "!\r\n";
//            Text += ") Description;\r\n";
//            return Text;
//        }


  
    }
}
