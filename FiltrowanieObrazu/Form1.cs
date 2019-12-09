using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FiltrowanieObrazu
{
    public partial class Form1 : Form
    {
        Color[,] constBitmap, originalBitmap;
        int[] Red;
        int[] Green;
        int[] Blue;
        int[,] changed;
        int width, height;
        int tryb = 1; //0-dodaj wielokat;1-pedzel kolowy;2-usun wielokat
        int filtr = 1;
        bool moving = false; int pom = int.MinValue, pointMoving = -1;
        int jasnosc = 127;
        double gamma = 1, kontrast = 0;
        Point[] punkty = new Point[18];
        int[] punktyzwykresu = new int[256];
        string filepath;
        Point clickedPoint;
        MultiPolygon multiPolygon = new MultiPolygon();
        (int, int, int)[] colors = new (int, int, int)[27];

        public Form1()
        {
            InitializeComponent();
            AddDefaultPoints();
            width = pictureBox1.Width;
            height = pictureBox1.Height;
            constBitmap = new Color[width, height];
            originalBitmap = new Color[width, height];
            Red = new int[256];
            Green = new int[256];
            Blue = new int[256];
            changed = new int[width, height];

            using (var constbmp = new Bitmap("kwiat.jpg"))
            {
                for (int i = 0; i < pictureBox1.Width; i++)
                {
                    for (int j = 0; j < pictureBox1.Height; j++)
                    {
                        constBitmap[i, j] = constbmp.GetPixel(i, j);
                        originalBitmap[i, j] = constbmp.GetPixel(i, j);
                        Red[constBitmap[i, j].R]++;
                        Green[constBitmap[i, j].G]++;
                        Blue[constBitmap[i, j].B]++;
                    }
                }
            }
        }

        private void GenerateColors()
        {
            colors[0] = (0, 0, 0);
            colors[1] = (0, 0, 127);
            colors[2] = (0, 0, 255);
            colors[3] = (0, 127, 0);
            colors[4] = (0, 255, 0);
            colors[5] = (0, 127, 127);
            colors[6] = (0, 127, 255);
            colors[7] = (0, 255, 127);
            colors[8] = (0, 255, 255);
            colors[9] =  (127, 0, 0);
            colors[10] = (127, 0, 127);
            colors[11] = (127, 0, 255);
            colors[12] = (127, 127, 0);
            colors[13] = (127, 255, 0);
            colors[14] = (127, 127, 127);
            colors[15] = (127, 127, 255);
            colors[16] = (127, 255, 127);
            colors[17] = (127, 255, 255);
            colors[18] = (255, 0, 0);
            colors[19] = (255, 0, 127);
            colors[20] = (255, 0, 255);
            colors[21] = (255, 127, 0);
            colors[22] = (255, 255, 0);
            colors[23] = (255, 127, 127);
            colors[24] = (255, 127, 255);
            colors[25] = (255, 255, 127);
            colors[26] = (255, 255, 255);

        }
        private void AddDefaultPoints()
        {
            punkty[0] = new Point(0, 0);
            punkty[1] = new Point(10, 10);
            punkty[2] = new Point(35, 40);
            punkty[3] = new Point(44, 80);
            punkty[4] = new Point(50, 100);
            punkty[5] = new Point(55, 90);
            punkty[6] = new Point(65, 55);
            punkty[7] = new Point(75, 40);
            punkty[8] = new Point(90, 20);
            punkty[9] = new Point(110, 15);
            punkty[10] = new Point(130, 45);
            punkty[11] = new Point(150, 70);
            punkty[12] = new Point(160, 75);
            punkty[13] = new Point(170, 85);
            punkty[14] = new Point(200, 130);
            punkty[15] = new Point(210, 140);
            punkty[16] = new Point(240, 200);
            punkty[17] = new Point(255, 255);
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            using (Bitmap actualBmp = new Bitmap(pictureBox1.Width, pictureBox1.Height))
            {
                unsafe
                {
                    BitmapData bmpData = actualBmp.LockBits(new Rectangle(0, 0, actualBmp.Width, actualBmp.Height), ImageLockMode.ReadWrite, actualBmp.PixelFormat);

                    byte* firstPixel = (byte*)bmpData.Scan0;

                    int heightpx = bmpData.Height;
                    int bytespx = System.Drawing.Bitmap.GetPixelFormatSize(actualBmp.PixelFormat) / 8;


                    int widthbytes = bmpData.Width * bytespx;

                    Parallel.For(0, heightpx, x =>
                    {
                        byte* currentLine = firstPixel + (x * bmpData.Stride);

                        for (int j = 0; j < widthbytes; j += bytespx)
                        {
                            currentLine[j] = constBitmap[j / 4, x].B;
                            currentLine[j + 1] = constBitmap[j / 4, x].G;
                            currentLine[j + 2] = constBitmap[j / 4, x].R;
                            currentLine[j + 3] = constBitmap[j / 4, x].A;
                        }
                    }
                    );

                    actualBmp.UnlockBits(bmpData);
                }
         
                e.Graphics.DrawImage(actualBmp, 0, 0);
            }

            DrawWielokat(e);
            CreateFunctionChart();

            DataTable dtR = new DataTable();
            dtR.Columns.Add("X_Value", typeof(int));
            dtR.Columns.Add("Y_Value", typeof(int));
            DataTable dtG = new DataTable();
            dtG.Columns.Add("X_Value", typeof(int));
            dtG.Columns.Add("Y_Value", typeof(int));
            DataTable dtB = new DataTable();
            dtB.Columns.Add("X_Value", typeof(int));
            dtB.Columns.Add("Y_Value", typeof(int));

            for (int i=0; i<=255; i++)
            {
                dtR.Rows.Add(i, Red[i]);
                dtG.Rows.Add(i, Green[i]);
                dtB.Rows.Add(i, Blue[i]);
            }

            chart1.DataSource = dtR;
            chart1.Series["Series1"].Color = Color.Red;
            chart2.DataSource = dtG;
            chart2.Series["Series1"].Color = Color.Green;
            chart3.DataSource = dtB;
            chart3.Series["Series1"].Color = Color.Blue;

            SetChart(chart1);
            SetChart(chart2);
            SetChart(chart3);
        }

        private int Silnia(int n, int a)
        {
            if (a == 0) return 1;
            long s = 1;
            long s1 = 1;

            for (int i=1; i<=a; i++)
            {
                s *= (n - a + i);
            }
            for (int i = 1; i <= a; i++)
            {
                s1 *= i;
            }
            return (int)(s / s1);

        }
        private void CreateFunctionChart()
        {
            DataTable wykres = new DataTable();
            wykres.Columns.Add("X_Value", typeof(int));
            wykres.Columns.Add("Y_Value", typeof(int));
            wykresFunkcji.Series["Series1"].Points.Clear();

            for (int i = 0; i < 18; i++)
            {
                wykresFunkcji.Series["Series1"].Points.AddXY(punkty[i].X, punkty[i].Y);
                wykres.Rows.Add(punkty[i].X, punkty[i].Y);
            }
            for (int i=0; i<=300; i++)
            {
                double t = i / 300.0;
                int sumax = 0, sumay=0;
                for(int j=0; j<18; j++)
                {
                    int x = Silnia(17, j);
                    sumax = sumax + (int)(punkty[j].X* x *Math.Pow(t, j)* Math.Pow((1.0-t), (17-j)));
                    sumay = sumay + (int)(punkty[j].Y * x * Math.Pow(t, j) * Math.Pow((1.0 - t), (17 - j)));
                }
                punktyzwykresu[sumax] = sumay;
            }

            wykresFunkcji.DataSource = wykres;

            wykresFunkcji.ChartAreas[0].AxisX.Minimum = 0;
            wykresFunkcji.ChartAreas[0].AxisX.Interval = 255;
            wykresFunkcji.ChartAreas[0].AxisY.Interval = 255;
            wykresFunkcji.Series["Series2"].XValueMember = "X_Value";
            wykresFunkcji.Series["Series2"].YValueMembers = "Y_Value";
            wykresFunkcji.Legends[0].Enabled = false;
            wykresFunkcji.Series["Series1"].ChartType = SeriesChartType.Point;
            wykresFunkcji.Series["Series2"].ChartType = SeriesChartType.Line;
            wykresFunkcji.ChartAreas[0].AxisY.LabelStyle.Format = "";
        }
        private void DrawWielokat(PaintEventArgs e)
        {
            foreach (var polygon in multiPolygon.multiPolygons)
            {
                foreach (var ver in polygon.vertices)
                {
                    if (ver.edges.end != new Point(-1, -1))
                    {
                        e.Graphics.DrawLine(new Pen(Color.Black), ver.edges.start.X, ver.edges.start.Y, ver.edges.end.X, ver.edges.end.Y);
                    }

                    e.Graphics.DrawRectangle(new Pen(ver.color, 4), ver.middle.X - (float)2, ver.middle.Y - (float)2, 4, 4);
                }
            }
        }
        private void SetChart(Chart chart)
        {
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Interval = 50;
            chart.ChartAreas[0].AxisY.Interval = 4000;
            
            chart.Series["Series1"].XValueMember = "X_Value";
            chart.Series["Series1"].YValueMembers = "Y_Value";
            chart.Legends[0].Enabled = false;
            chart.Series["Series1"].ChartType = SeriesChartType.Column;
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "";
        }

        private List<Edges> GetEdges(Polygon p)
        {
           List<Edges> edges = new List<Edges>();

            for (int i = 0; i <p.vertices.Count-1; i++)
            {
                var a = p.vertices[i].edges.start;
                var b = p.vertices[i].edges.end;
                var e = new Edges(b, a);
                if(a.Y<b.Y)
                    e = new Edges(a, b);

                edges.Add(e);
            }

            var a1 = p.vertices[p.vertices.Count - 1].edges.start;
            var b1 = p.vertices[0].edges.start;
            var e1 = new Edges(b1, a1);
            if (a1.Y < b1.Y)
                e1 = new Edges(a1, b1);
            edges.Add(e1);

            return edges;
        }

        private void FillWielokat()
        {
            Parallel.ForEach(multiPolygon.multiPolygons, t =>
            {
                if (t.niepomalowany || t.doUsuniecia)
                {
                    int ymax = 0, ymin = pictureBox1.Height;

                    foreach (var e in GetEdges(t))
                    {
                        if (e.end.Y > ymax)
                            ymax = e.end.Y;
                        if (e.start.Y < ymin)
                            ymin = e.start.Y;
                    }

                    List<List<Edges>> listET = new List<List<Edges>>();

                    for (int i = ymin; i <= ymax; i++)
                        listET.Add(new List<Edges>());

                    foreach (var e in GetEdges(t))
                    {
                        if (e.start.Y != e.end.Y) // pomijamy poziome
                            listET[e.start.Y - ymin].Add(e);
                    }

                    int y = ymin;
                    List<Edges> listAET = new List<Edges>();
                    while ((y <= ymax))
                    {
                        foreach (var e in listET[y - ymin])
                        {
                            e.x = e.start.X + (double)((double)((double)(e.end.X - e.start.X) / (double)(e.end.Y - e.start.Y)) * (double)(y - e.start.Y)); //punkt przeciecia
                            listAET.Add(e);
                        }

                        listAET = listAET.OrderBy(e => e.x).ToList();
                        for (int i = 0; i < listAET.Count() - 1; i += 2)
                        {
                            for (int j = (int)Math.Floor(listAET[i].x); j <= Math.Ceiling(listAET[i + 1].x); j++)
                            {
                                if (filtr == 2)
                                    Negacja(j, y);
                                if (filtr == 3)
                                    Jasnosc(j, y);
                                if (filtr == 4)
                                    KorekcjaGamma(j, y);
                                if (filtr == 5)
                                    Kontrast(j, y);
                                if (filtr == 6)
                                    WlasnaFunkcja(j, y);
                                if (filtr == 1 || t.doUsuniecia)
                                    constBitmap[j, y] = originalBitmap[j, y];
                            }
                        }

                        listAET.RemoveAll(e => e.end.Y == y + 1);

                        y++;

                        foreach (var e in listAET)
                        {
                            e.x += (double)((double)(e.end.X - e.start.X) / (double)(e.end.Y - e.start.Y));
                        }
                    }
                    t.niepomalowany = false;
                }
            });
            return;
        }

        private void ConutHistogram()
        {
            Red = new int[256];
            Blue = new int[256];
            Green = new int[256];
            for (int i = 0; i < pictureBox1.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Height; j++)
                {
                    Red[constBitmap[i, j].R]++;
                    Green[constBitmap[i, j].G]++;
                    Blue[constBitmap[i, j].B]++;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            tryb = 1;
            label2.Text = button2.Text;
            button2.BackColor = Color.FromArgb(153, 180, 209);
            button1.BackColor = Color.FromKnownColor(KnownColor.Control);
            button3.BackColor = Color.FromKnownColor(KnownColor.Control);
            button4.BackColor = Color.FromKnownColor(KnownColor.Control);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tryb = 0;
            label2.Text = button1.Text;
            button1.BackColor = Color.FromArgb(153, 180, 209);
            button2.BackColor = Color.FromKnownColor(KnownColor.Control);
            button3.BackColor = Color.FromKnownColor(KnownColor.Control);
            button4.BackColor = Color.FromKnownColor(KnownColor.Control);
        }

        private void Negacja(int i, int j)
        {
            constBitmap[i, j] = Color.FromArgb(255 - constBitmap[i, j].R, 255 - constBitmap[i, j].G, 255 - constBitmap[i, j].B);
        }
        private void Jasnosc(int i, int j)
        {
            int r = constBitmap[i, j].R + jasnosc;
            int g = constBitmap[i, j].G + jasnosc;
            int b = constBitmap[i, j].B + jasnosc;

            constBitmap[i, j] = Color.FromArgb(CheckRange(r), CheckRange(g), CheckRange(b));
        }
        private void KorekcjaGamma(int i, int j)
        {
            int r = (int)(255.0 * Math.Pow((constBitmap[i, j].R / 255.0), 1.0 / gamma));
            int g = (int)(255.0 * Math.Pow((constBitmap[i, j].G / 255.0), 1.0 / gamma));
            int b = (int)(255.0 * Math.Pow((constBitmap[i, j].B / 255.0), 1.0 / gamma));

            constBitmap[i, j] = Color.FromArgb(CheckRange(r), CheckRange(g), CheckRange(b));
        }
        private void Kontrast(int i, int j)
        {
            int r = (int)(kontrast * (constBitmap[i, j].R - 127) + 127);
            int g = (int)(kontrast * (constBitmap[i, j].G - 127) + 127);
            int b = (int)(kontrast * (constBitmap[i, j].B - 127) + 127);

            constBitmap[i, j] = Color.FromArgb(CheckRange(r), CheckRange(g), CheckRange(b));
        }
        private void WlasnaFunkcja(int i, int j)
        {
            constBitmap[i, j] = Color.FromArgb(punktyzwykresu[constBitmap[i, j].R], punktyzwykresu[constBitmap[i, j].G], punktyzwykresu[constBitmap[i, j].B]);
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(moving)
            {
                if (filtr == 2) //negacja
                {
                    for (int i = 0; i < pictureBox1.Width; i++)
                    {
                        for (int j = 0; j < pictureBox1.Height; j++)
                        {
                            if (DlugoscOdcinka(e.X, e.Y, i, j) <= 20 && changed[i, j]!=2)
                            {
                                Negacja(i, j);
                                changed[i, j] = 2;
                            }
                        }
                    }
                }
                if (filtr == 1) //brak filtru
                {
                    for (int i = 0; i < pictureBox1.Width; i++)
                    {
                        for (int j = 0; j < pictureBox1.Height; j++)
                        {
                            if (DlugoscOdcinka(e.X, e.Y, i, j) <= 20)
                            {
                                constBitmap[i, j] = originalBitmap[i, j];
                                changed[i, j] = 1;
                            }
                        }
                    }
                }
                for (int i = 0; i < pictureBox1.Width; i++)
                {
                    for (int j = 0; j < pictureBox1.Height; j++)
                    {
                        if (DlugoscOdcinka(e.X, e.Y, i, j) <= 20 && changed[i, j] != pom)
                        {
                            if (filtr == 3)
                            {
                                Jasnosc(i, j);
                                changed[i, j] = pom;
                            }

                            if (filtr == 4)
                            {
                                KorekcjaGamma(i, j);
                                changed[i, j] = pom;
                            }
                            if (filtr == 5)
                            {
                                Kontrast(i, j);
                                changed[i, j] = pom;
                            }
                            if(filtr == 6)
                            {
                                WlasnaFunkcja(i, j);
                                changed[i, j] = pom;
                            }
                        }
                    }
                }

            }
        }

        private double DlugoscOdcinka(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        private int CheckRange(int c)
        {
            if (c > 255)
                c = 255;
            if (c < 0)
                c = 0;
            return c;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            pom++;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                filtr = 1;//brak filtra
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ConutHistogram();
            pictureBox1.Invalidate();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            jasnosc = trackBar1.Value;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                filtr = 4; //gamma
        }

        private void gamma_slider_ValueChanged(object sender, EventArgs e)
        {
            gamma = gamma_slider.Value / 100;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton3.Checked)
                filtr = 3; //jasnosc
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            if(trackBar2.Value <= 0)
            {
                kontrast = 1.0 + (trackBar2.Value / 256.0);
            }
            else
            {
                kontrast = 256.0 / Math.Pow(2, Math.Log(257 - trackBar2.Value, 2));
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
                filtr = 5; //kontrast
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Point clickedPoint = new Point(e.X, e.Y);

            if (tryb==0)
            {
                Polygon polygon = new Polygon();
                if (multiPolygon.multiPolygons.Count == 0)
                    multiPolygon.multiPolygons.Add(polygon);
                polygon = this.multiPolygon.multiPolygons.Last();

                if (polygon.newPolygon && polygon.vertices.Count > 0)
                {
                    this.multiPolygon.multiPolygons.Add(new Polygon());
                    polygon = this.multiPolygon.multiPolygons.Last();
                }

                polygon.AddVertex(clickedPoint);
            }
            if(tryb==2)
            {
                DeletePolygon(clickedPoint);
            }
        }

        private void DeletePolygon(Point clickedPoint)
        {
            if (multiPolygon.FindVertexInPolygons(clickedPoint).Item1 != -1)
            {
                var (p, v) = multiPolygon.FindVertexInPolygons(clickedPoint);
                multiPolygon.multiPolygons[p].doUsuniecia = true;
                FillWielokat();
                multiPolygon.multiPolygons.RemoveAt(p);
            }
            else if (multiPolygon.FindEdge(clickedPoint, false).Item1 != -1)
            {
                var (p, v) = multiPolygon.controlEdge;
                multiPolygon.multiPolygons[p].doUsuniecia = true;
                FillWielokat();
                multiPolygon.multiPolygons.RemoveAt(p);
                multiPolygon.controlEdge = (-1, -1);
            }
        }

        private void wykresFunkcji_MouseDown(object sender, MouseEventArgs e)
        {
            clickedPoint = new Point(e.X, e.Y);
            var z =  wykresFunkcji.HitTest(e.X, e.Y);
            if (z.PointIndex != -1)
            {
                pointMoving = z.PointIndex;
            }
        }

        private void zastosuj_Click(object sender, EventArgs e)
        {
            label2.Text = button4.Text;
            button4.BackColor = Color.FromArgb(153, 180, 209);
            button1.BackColor = Color.FromKnownColor(KnownColor.Control);
            button3.BackColor = Color.FromKnownColor(KnownColor.Control);
            button2.BackColor = Color.FromKnownColor(KnownColor.Control);
            if(multiPolygon.multiPolygons[0].vertices.Count() > 0)
                FillWielokat();
        }

        private void wykresFunkcji_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - clickedPoint.X;
            int dy = e.Y - clickedPoint.Y;
            if(pointMoving != -1 && pointMoving < 18)
            {
                try
                {
                    int xx = (int)wykresFunkcji.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X);
                    int yy = (int)wykresFunkcji.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y);
                    punkty[pointMoving] = new Point(xx, yy);
                }
                catch { }
            }
        }

        private void wykresFunkcji_MouseUp(object sender, MouseEventArgs e)
        {
            pointMoving = -1;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton6.Checked)
                filtr = 6; //wlasna funkcja
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GenerateColors();
            Wygeneruj();
        }
        private void Wygeneruj()
        {
            int s = (pictureBox1.Width / 2) / 27;
            for (int i = 0; i < (pictureBox1.Width / 2); i++)
            {
                for (int j = 0; j < pictureBox1.Height / 2; j++)
                {
                    int k = i / s ;
                    if (k >= 27) k = 26;
                    constBitmap[i, j] = Color.FromArgb(colors[k].Item1, colors[k].Item2, colors[k].Item3);
                    originalBitmap[i, j] = Color.FromArgb(colors[k].Item1, colors[k].Item2, colors[k].Item3);
                    Red[constBitmap[i, j].R]++;
                    Green[constBitmap[i, j].G]++;
                    Blue[constBitmap[i, j].B]++;
                }
            }
            for (int i = 0; i < (pictureBox1.Width / 2); i++)
            {
                for (int j = 0; j < pictureBox1.Height / 2; j++)
                {
                    int k = i / s;
                    if (k >= 27) k = 26;
                    int c = (int)((colors[k].Item1  + colors[k].Item2  + colors[k].Item3)/3.0);
                    constBitmap[i, j+ pictureBox1.Height / 2] = Color.FromArgb(c, c, c);
                    originalBitmap[i, j+ pictureBox1.Height / 2] = Color.FromArgb(c, c, c);
                    Red[constBitmap[i, j+ pictureBox1.Height / 2].R]++;
                    Green[constBitmap[i, j+ pictureBox1.Height / 2].G]++;
                    Blue[constBitmap[i, j+ pictureBox1.Height / 2].B]++;
                }
            }
            for (int i = 0; i < (pictureBox1.Width / 2); i++)
            {
                for (int j = 0; j < pictureBox1.Height / 2; j++)
                {
                    int k = i / s;
                    if (k >= 27) k = 26;
                    int max1 = Math.Max(colors[k].Item1, colors[k].Item2);
                    int max = Math.Max(max1, colors[k].Item3);
                    int min1 = Math.Min(colors[k].Item1, colors[k].Item2);
                    int min = Math.Min(min1, colors[k].Item3);
                    constBitmap[i + pictureBox1.Width / 2, j + pictureBox1.Height / 2] = Color.FromArgb((max + min) / 2, (max + min) / 2, (max + min) / 2);
                    originalBitmap[i+ pictureBox1.Width / 2, j + pictureBox1.Height / 2] = Color.FromArgb((max + min) / 2, (max + min) / 2, (max + min) / 2);
                    Red[constBitmap[i+ pictureBox1.Width / 2, j + pictureBox1.Height / 2].R]++;
                    Green[constBitmap[i+ pictureBox1.Width / 2, j + pictureBox1.Height / 2].G]++;
                    Blue[constBitmap[i+ pictureBox1.Width / 2, j + pictureBox1.Height / 2].B]++;
                }
            }
            for (int i = 0; i < (pictureBox1.Width / 2); i++)
            {
                for (int j = 0; j < pictureBox1.Height / 2; j++)
                {   
                    int k = i / s;
                    if (k >= 27) k = 26;
                    int c = (int)(colors[k].Item1 * 0.299 + colors[k].Item2 * 0.587 + colors[k].Item3 * 0.114);
                    constBitmap[i + pictureBox1.Width / 2, j] = Color.FromArgb(c, c, c);
                    originalBitmap[i + pictureBox1.Width / 2, j] = Color.FromArgb(c,c,c);
                    Red[constBitmap[i + pictureBox1.Width / 2, j].R]++;
                    Green[constBitmap[i + pictureBox1.Width / 2, j].G]++;
                    Blue[constBitmap[i + pictureBox1.Width / 2, j].B]++;
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog.Filter = "jpg files (*.jpg)|*.jpg";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filepath = openFileDialog.FileName;
                    using (var constbmp = new Bitmap(filepath))
                    {
                        for (int i = 0; i < pictureBox1.Width; i++)
                        {
                            for (int j = 0; j < pictureBox1.Height; j++)
                            {
                                constBitmap[i, j] = constbmp.GetPixel(i, j);
                                originalBitmap[i, j] = constbmp.GetPixel(i, j);
                                Red[constBitmap[i, j].R]++;
                                Green[constBitmap[i, j].G]++;
                                Blue[constBitmap[i, j].B]++;
                            }
                        }
                    }
                }
            }
        }

        private void negacjaButton_CheckedChanged(object sender, EventArgs e)
        {
            if (negacjaButton.Checked)
                filtr = 2; //negacja
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (tryb == 1)
                 moving = true; 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tryb = 2;
            label2.Text = button4.Text;
            button3.BackColor = Color.FromArgb(153, 180, 209);
            button1.BackColor = Color.FromKnownColor(KnownColor.Control);
            button4.BackColor = Color.FromKnownColor(KnownColor.Control);
            button2.BackColor = Color.FromKnownColor(KnownColor.Control);
        }
    }
}
