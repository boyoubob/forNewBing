using DS.GraphFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.DredgerArea
{
    public class DigParser
    {
        sgEditObjectGroup elements = null;

        public sgEditObjectGroup Elements
        {
            get
            {
                if (elements == null)
                {
                    elements = new sgEditObjectGroup();
                    foreach (sgObject one in objs)
                    {
                        elements.Add(one);
                    }
                }

                return elements;
            }
        }

        List<sgObject> objs = new List<sgObject>();

        private IEnumerator<string> lines;

        public void LoadContent(string fileName)
        {
            string content = System.IO.File.ReadAllText(fileName, Encoding.Default);

            IEnumerable<string> o = content.Split('\r', '\n');

            lines = o.GetEnumerator();

            Parse();
        }

        public bool NextLine()
        {
            while (lines.MoveNext())
            {
                if (lines.Current.Trim() != "") return true;
            }

            return false;
        }
        private List<sgObject> Parse()
        {
            lines.Reset();
            sgPolyline gpl = null;
            while (NextLine())
            {
                try
                {
                    string line = lines.Current;
                    string[] sts = line.Split(' ');

                    #region 直线
                    if (sts[0] == "X")
                    {
                        objs.Add(GetBeeLine(sts));
                        continue;
                    }

                    #endregion
                    #region 多段线
                    if (sts[0] == "I")
                    {
                        if (gpl == null)
                        {
                            gpl = new sgPolyline();
                            gpl.ColorMode = ObColorMode.OBCM_RGB;
                            if (sts.Length >= 5) gpl.Line_stipple_index = GetLineStippleIndex(sts[4]);
                            if (sts.Length >= 7)
                            {
                                float linew = float.Parse(sts[6]);
                                gpl.Line_width = linew == 0.0 ? 1 : linew;
                            }
                        }
                        sgPoint theP = GetPoint(sts);
                        gpl.Color = theP.Color;
                        gpl.Points.Add(new Vertex(theP.Point));
                        continue;
                    }

                    if (sts.Length >= 4)
                    {
                        if (sts[0] == "E" && sts[1] == "0" && sts[2] == "0" && sts[3] == "END")
                        {
                            if (gpl != null)
                            {
                                sgPolyline thePolyline = new sgPolyline(gpl.Points);
                                thePolyline.ColorMode = ObColorMode.OBCM_RGB;
                                thePolyline.Color = gpl.Color;
                                thePolyline.Line_width = gpl.Line_width;
                                thePolyline.Line_stipple_index = gpl.Line_stipple_index;
                                objs.Add(thePolyline);
                                gpl = null;
                            }
                            continue;
                        }
                    }
                    #endregion
                    #region 多段线S
                    if (sts[0] == "S")
                    {
                        if (gpl == null)
                        {
                            gpl = new sgPolyline();
                            gpl.ColorMode = ObColorMode.OBCM_RGB;
                            gpl.Line_stipple_index = 0;
                            gpl.Line_width = 1;
                        }
                        sgPoint theP = GetPoint_S(sts);
                        gpl.Color = theP.Color;
                        gpl.Points.Add(new Vertex(theP.Point));
                        continue;
                    }

                    if (sts.Length >= 4)
                    {
                        if (sts[0] == "E" && sts[1] == "0" && sts[2] == "0" && sts[3] == "END")
                        {
                            if (gpl != null)
                            {
                                sgPolyline thePolyline = new sgPolyline(gpl.Points);
                                thePolyline.ColorMode = ObColorMode.OBCM_RGB;
                                thePolyline.Color = gpl.Color;
                                thePolyline.Line_width = gpl.Line_width;
                                thePolyline.Line_stipple_index = gpl.Line_stipple_index;
                                objs.Add(thePolyline);
                                gpl = null;
                            }
                            continue;
                        }
                    }
                    #endregion
                    #region 矩形
                    if (sts[0] == "O")
                    {
                        objs.Add(GetRectangle(sts));
                        continue;
                    }
                    #endregion
                    #region ARC
                    if (sts[0] == "A")
                    {
                        objs.Add(GetArc(sts));
                        continue;
                    }
                    #endregion
                    #region  circle
                    if (sts[0] == "C")
                    {
                        objs.Add(GetCircle(sts));
                        continue;
                    }
                    #endregion
                    #region Z 带字体的文字
                    if (sts[0] == "Z")//
                    {
                        objs.Add(GetText1(line, sts));
                        continue;
                    }
                    #endregion
                    #region T 只有坐标和文本信息
                    if (sts[0] == "T")//
                    {
                        objs.Add(GetText2(sts));
                        continue;
                    }
                    #endregion
                    #region 浮筒
                     
                    if (sts[0] == "M")
                    {
                        objs.Add(GetGreenBuoy(sts));
                        continue;

                    }
                    if (sts[0] == "F")
                    {
                        objs.Add(GetRedBuoy(sts));
                        continue;

                    }
                    if (sts[0] == "x")
                    {
                        objs.Add(GetYellowBuoy(sts));
                        continue;
                    }
                    if (sts[0] == "b")
                    {
                        objs.Add(GetLightShip(sts));
                        continue;
                    } 
                    if (sts[0] == "f")
                    {
                        objs.Add(GetLightTower(sts));
                        continue;
                    } 
                    if (sts[0] == "V")
                    {
                        objs.Add(GetFish(sts));
                        continue;
                    } 
                    #endregion 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return objs;
        }

        Color GetColor(int color)
        {
            int r = (int)((byte)(color & 0xFF));
            int g = (int)(byte)((color >> 8) & 0xFF);
            int b = (int)(byte)((color >> 16) & 0xFF);
            return Color.FromArgb(r, g, b);
        }
         
        int GetLineStippleIndex(string name)
        {
            switch (name)
            {
                case "0":
                    return 0;
                case "1":
                    return 3;//PenStype.PS_DASH; 
                case "2":
                    return 1;//PenStype.PS_DOT; 
                case "3":
                    return 4;//PenStype.PS_DASHDOT; 
                case "4":
                    return 4;//PenStype.PS_DASHDOTDOT;
                case "5":
                    return 0;//PenStype.PS_NULL;
                default:
                    return 0;//PenStype.PS_SOLID;
            }
        }

        private sgLine GetBeeLine(string[] sts)
        {
            sgLine gbl = new sgLine();

            Vertex v0 = new Vertex();
            Vertex v1 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            v1.Y = double.Parse(sts[3]);
            v1.X = double.Parse(sts[4]);
            v1.Z = 0.0;

            gbl.Line_stipple_index = GetLineStippleIndex(sts[5]);
            int color = int.Parse(sts[6]);
            if (color != 0)
            {
                gbl.ColorMode = ObColorMode.OBCM_RGB;
                gbl.Color = GetColor(color);
            }
            float linew = float.Parse(sts[7]);
            gbl.Line_width = linew == 0.0 ? 1 : linew;
            gbl.P0 = v0;
            gbl.P1 = v1;
            return gbl;
        }

        private sgPolyline GetRectangle(string[] sts)
        {
            sgPolyline grect = new sgPolyline();

            Vertex v0 = new Vertex();//left toop 
            Vertex v1 = new Vertex();//left down
            Vertex v2 = new Vertex();//right down
            Vertex v3 = new Vertex();//right top
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            double len = double.Parse(sts[3]);
            double width = double.Parse(sts[4]);
            double angle = double.Parse(sts[5]);

            v1.X = v0.X - width * Math.Sin(angle * Math.PI / 180);
            v1.Y = v0.Y - width * Math.Cos(angle * Math.PI / 180);
            v1.Z = 0.0;
            v3.X = v0.X + len * Math.Cos(angle * Math.PI / 180);
            v3.Y = v0.Y - len * Math.Sin(angle * Math.PI / 180);
            v3.Z = 0.0;
            v2.X = v3.X - width * Math.Sin(angle * Math.PI / 180);
            v2.Y = v3.Y - width * Math.Cos(angle * Math.PI / 180);
            v2.Z = 0.0;
            grect.Line_stipple_index = GetLineStippleIndex(sts[6]);
            int color = int.Parse(sts[7]);
            if (color != 0)
            {
                grect.ColorMode = ObColorMode.OBCM_RGB;
                grect.Color = GetColor(color);
            }

            float linewith = float.Parse(sts[8]);
            grect.Line_width = linewith == 0.0 ? 1 : linewith;

            grect.Points.Add(v0);
            grect.Points.Add(v1);
            grect.Points.Add(v2);
            grect.Points.Add(v3);

            return grect;

        }


        private sgPoint GetPoint(string[] sts)
        {
            sgPoint gp = new sgPoint();
            Vertex v = new Vertex();
            v.X = double.Parse(sts[1]);
            v.Y = double.Parse(sts[2]);
            v.Z = 0.0;
            int color = int.Parse(sts[3]);
            if (color != 0)
            {
                gp.ColorMode = ObColorMode.OBCM_RGB;

                gp.Color = GetColor(color);
            }
            float linew = float.Parse(sts[6]);
            gp.Line_width = linew == 0.0 ? 1 : linew;
            gp.Point = v;
            return gp;
        }
        private sgPoint GetPoint_S(string[] sts)
        {
            sgPoint gp = new sgPoint();
            Vertex v = new Vertex();
            v.X = double.Parse(sts[1]);
            v.Y = double.Parse(sts[2]);
            v.Z = 0.0;

            gp.ColorMode = ObColorMode.OBCM_RGB;
            gp.Color = Color.White;
            float linew = 1.0f;
            gp.Line_width = linew;
            gp.Point = v;
            return gp;
        }
        private sgCircle GetCircle(string[] sts)
        {
            sgCircle gcircle = new sgCircle();

            Vertex basevertex = new Vertex();
            basevertex.X = double.Parse(sts[1]);
            basevertex.Y = double.Parse(sts[2]);
            basevertex.Z = 0.0;
            gcircle.Center = basevertex;
            gcircle.R = double.Parse(sts[3]);
            gcircle.Line_stipple_index = GetLineStippleIndex(sts[4]);
            int color = int.Parse(sts[5]);
            if (color != 0)
            {
                gcircle.ColorMode = ObColorMode.OBCM_RGB;

                gcircle.Color = GetColor(color);
            }
            return gcircle;
        } 
        //只有坐标和文本信息
        private sgText GetText2(string[] sts)
        {
            sgText gtext = new sgText();
            gtext.DrawAtbottomLeft = false;
            Vertex basevertex = new Vertex();
            basevertex.X = double.Parse(sts[1]);
            basevertex.Y = double.Parse(sts[2]);
            basevertex.Z = 0.0;
            gtext.Position = basevertex;
            string text = "";
            for (int i = 3; i < sts.Length; i++)
            {
                text = text + " " + sts[i];
            }
            gtext.Text = text.Trim();

            return gtext;
        }

        private sgText GetText1(string line, string[] sts)
        {
            sgText gtext = new sgText();
            gtext.DrawAtbottomLeft = false;

            // 坐标
            Vertex basevertex = new Vertex();
            basevertex.X = double.Parse(sts[1]);
            basevertex.Y = double.Parse(sts[2]);
            basevertex.Z = -0.3;
            gtext.Position = basevertex;

            //  文字角度
            double angle = double.Parse(sts[3]);
            gtext.Text_angle = -angle;
            int color = int.Parse(sts[4]);
            if (color != 0)
            {
                gtext.ColorMode = ObColorMode.OBCM_RGB;
                gtext.Color = GetColor(color);
            }

            gtext.Font_size = float.Parse(sts[5]);

            string text = "";

            if (sts.Length == 9)
            {
                gtext.Font_name = "宋体";
                gtext.Text = sts[8].Trim();

                return gtext;
            }
            else
            {
                int nu = 0;

                for (int i = 8; i < sts.Length - 1; i++)
                {
                    string s = sts[i];
                    if (s == "") nu++;
                    else
                    {
                        for (int j = 0; j < nu; j++)
                        {
                            text = text + " ";
                        }

                        text = text + " ";
                        nu = 0;
                        text = text + s;
                    }
                }
                if (sts[sts.Length - 1] == "")
                {
                    gtext.Text = text.Trim();
                    gtext.Font_name = "宋体";
                    return gtext;
                }
            }

            // 分析字体
            // 系统字体
            System.Drawing.FontFamily[] fs = System.Drawing.FontFamily.Families;
            string[] families = new string[fs.Length];
            for (int i = 0, size = fs.Length; i < size; i++)
            {
                families[i] = fs[i].Name;
            }
            string subfamily = sts[sts.Length - 1];
            if (subfamily.Length > 0)
            {
                if (subfamily[0] == '@')
                {
                    if (families.Contains(subfamily.Substring(1)))
                    {
                        gtext.Font_name = subfamily.Substring(1);
                    }
                    else
                    {
                        gtext.Font_name = "宋体";
                    }
                }
                else
                {
                    if (families.Contains(subfamily))
                    {
                        gtext.Font_name = subfamily;
                    }
                    else
                    {
                        if (subfamily == "Roman")
                        {
                            text = text.Substring(0, text.IndexOf("Times New"));

                            gtext.Font_name = "Times New Roman";
                        }
                        else
                        {
                            gtext.Font_name = "宋体";
                        }
                    }
                }
            }

            gtext.Text = text.Trim();

            return gtext;
        }

        private sgArc GetArc(string[] sts)
        {
            sgArc garc = new sgArc();

            Vertex basevertex = new Vertex();
            basevertex.X = double.Parse(sts[1]);
            basevertex.Y = double.Parse(sts[2]);
            basevertex.Z = 0.0;
            garc.BaseVertex = basevertex;
            double endangle = double.Parse(sts[3]);
            double begangle = double.Parse(sts[4]);
            if (begangle > 90 && endangle > 90 && endangle > begangle)
            {
                garc.BeginAngle = 360 - begangle + 90;
                garc.EndAngle = 360 - endangle + 90;

            }
            else
            {
                if (begangle <= 90)
                {
                    garc.BeginAngle = 90 - begangle;
                }
                else
                {
                    garc.BeginAngle = 360 - begangle + 90;
                }

                if (endangle <= 90 && endangle > 0)
                {
                    garc.EndAngle = 90 - endangle;
                }
                else
                {
                    garc.EndAngle = -(endangle - 90);
                }
            }
            garc.Radius = double.Parse(sts[5]);
            garc.Line_stipple_index = GetLineStippleIndex(sts[6]);
            int color = int.Parse(sts[7]);
            if (color != 0)
            {
                garc.ColorMode = ObColorMode.OBCM_RGB;

                garc.Color = GetColor(color);
            }

            return garc;
        }

        #region 浮标
        private FBRedbouy GetRedBuoy(string[] sts)
        {
            FBRedbouy gbuoy = new FBRedbouy();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3].ToLower().StartsWith("buoy-") ? "" : sts[3];
            return gbuoy;
        }

        private FBGreenbouy GetGreenBuoy(string[] sts)
        {
            FBGreenbouy gbuoy = new FBGreenbouy();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.3;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3].ToLower().StartsWith("buoy-") ? "" : sts[3];
            return gbuoy;
        }

        private FBYellowbouy GetYellowBuoy(string[] sts)
        {
            FBYellowbouy gbuoy = new FBYellowbouy();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = -0.3;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3].ToLower().StartsWith("buoy-") ? "" : sts[3];
            return gbuoy;
        }
        private FBFish GetFish(string[] sts)
        {
            FBFish gbuoy = new FBFish();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3];
            return gbuoy;
        }

        private FBAnchor GetAnchor(string[] sts)
        {
            FBAnchor gbuoy = new FBAnchor();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3];
            return gbuoy;
        }

        private FBLightShip GetLightShip(string[] sts)
        {
            FBLightShip gbuoy = new FBLightShip();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3];
            return gbuoy;
        }

        private FBLightTower GetLightTower(string[] sts)
        {
            FBLightTower gbuoy = new FBLightTower();
            Vertex v0 = new Vertex();
            v0.X = double.Parse(sts[1]);
            v0.Y = double.Parse(sts[2]);
            v0.Z = 0.0;
            gbuoy.Pos = v0;
            if (sts.Length == 4) gbuoy.Number = sts[3];
            return gbuoy;
        }




        #endregion

    }
}
