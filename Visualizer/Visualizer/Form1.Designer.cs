using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Visualizer
{
    partial class Form1
    {

       public float travere, Xcut, Ycut, Wcut, Vcut;
        String prevCut = "";


        Timer timer1;
        List<Line> mCoords = new List<Line>();
        List<Figure> mFigures = new List<Figure>();
        private float mHeight, mWidth, mScale;
        SqlCommand mSqlCom;
        SqlCommand mFigureCon;
        SqlDependency sqlDep;
        int counter = 1;

        public const string DATABASE_CONNECTION_STRING = "Server=192.168.0.3;Database=;UID=vz;PWD=vz911;";

        public const string LINES_QUERY = "SELECT X1, Y1, X2, Y2, X, Y FROM dbo.ZakazPozSklo WHERE idList = 217876";
        public const string FIGURE_QUERY = "SELECT Description FROM dbo.SForm WHERE idList = 217876";
        public const string STRAIGHT_LINE_TYPE = "[GERADE]";
        public const string CURVE_LINE_TYPE = "[BOGEN]";
        private System.ComponentModel.IContainer components = null;


        public void InitializeComponent()
        {
            this.Text = "Drawing";
            this.Size = new System.Drawing.Size(600, 400);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.KeyDown += onKeyPress;
            
            //SqlDependency.Start(DATABASE_CONNECTION_STRING);
            SqlConnection sqlConnection = new SqlConnection(DATABASE_CONNECTION_STRING);
            mSqlCom = new SqlCommand(LINES_QUERY, sqlConnection);
            mFigureCon = new SqlCommand(FIGURE_QUERY, sqlConnection);
            mFigureCon.Notification = null;
            mSqlCom.Notification = null;
            //sqlDep = new SqlDependency(mSqlCom);

            //sqlDep.OnChange += OnChange;


            sqlConnection.Open();
            readData(mSqlCom.ExecuteReader());

            readFigures(mFigureCon.ExecuteReader());

            timer1 = new Timer();
            timer1.Tag = "timer1";
            timer1.Interval = 1;
            timer1.Tick += timer_Tick;
            timer1.Start();
        }
      
        protected override void Dispose(bool disposing)
        {
            SqlDependency.Stop(DATABASE_CONNECTION_STRING);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var Canvas = this.CreateGraphics();

            mWidth = this.Width;
            mHeight = this.Height;
            mScale = (float) 0.9 * (mWidth) / 6000;

            if (mCoords.Count < counter) {

                counter = mCoords.Count;
            }
            for (int i = 0; i < counter; i++)
            {
                mCoords[i].drawLine(Canvas, mWidth - 50, mHeight - 50, mScale);

            }

            for (int i = 0; i < mFigures.Count; i++) {
                mFigures[i].drawFigure(Canvas, mWidth - 50, mHeight - 50, mScale);
            }

            // graphicsPath.AddArc(100,100,500,300, 0, -180);
            // graphicsPath.AddLine(600,250,600,600);
            // graphicsPath.AddLine(600,600,100,600);
            // graphicsPath.AddLine(100,600,100,250);
            //Canvas.FillPath(Brushes.Red, graphicsPath);

            //	drawFigure(Canvas, 0, 100, 400, 300);
            //drawFigure(Canvas, 400, 400, 100, 300);
            //drawLine(Canvas, 400,0,400,800);

            timer1.Stop();
        }

        private void OnChange(Object sender, SqlNotificationEventArgs e)
        {
            readData(mSqlCom.ExecuteReader());
            timer1.Start();
        }

        private void onKeyPress(Object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Right) {
                counter++;
                timer1.Start();
            }
        }
        private void readData(SqlDataReader dataReader) {
            
            mCoords.Clear();

            mCoords.Add(new Line(0f, 0f, 6000f, 0f, 0f, 0f));
            mCoords.Add(new Line(0f, 0f, 0f, 3200f, 0f, 0f));
            /*            mCoords.Add(new Line(0f, 3200f, 6000f, 3200f, 0f, 0f));
                        mCoords.Add(new Line(6000f, 0f, 6000f, 3200f, 0f, 0f));*/
            float counter = 0;

            while (dataReader.Read())
            {
                double x1 = Convert.ToDouble(dataReader.GetValue(0));
                double y1 = Convert.ToDouble(dataReader.GetValue(1));
                double x2 = Convert.ToDouble(dataReader.GetValue(2));
                double y2 = Convert.ToDouble(dataReader.GetValue(3));
                double x = Convert.ToDouble(dataReader.GetValue(4));
                double y = Convert.ToDouble(dataReader.GetValue(5));
                mCoords.Add(new Line(
                     (float)x1,
                     (float) y1,
                     (float) x2,
                     (float) y2,
                     (float) x,
                     (float) y));
        
            };
            dataReader.Close();
        }

        private void readFigures(SqlDataReader dataReader) {

            while (dataReader.Read()) {
                String figureString = dataReader.GetString(0);
                Regex regex = new Regex(@"\[(\w*)\][\d;\.]*:");
                MatchCollection matches = regex.Matches(figureString);

                if (matches.Count > 0)
                {
                    Figure figure = new Figure();
                    foreach (Match match in matches)
                    {
                        regex = new Regex(@"[\d\.]+");
                        Regex lineReg = new Regex(@"\[\w+\]");                                            
                        MatchCollection coordinates = regex.Matches(match.Value);
                        Match type = lineReg.Match(match.Value);
                        FigurePart figurePart = new FigurePart(type.Value, 000f, 1420f);
                        if (coordinates.Count > 0)
                        {
                            foreach (Match coord in coordinates)
                            {
                                figurePart.addCoord(coord.Value);

                            }
         
                        }
                        figure.addPart(figurePart);
                    }

                    mFigures.Add(figure);
                }
                    //"[GERADE];91.700;00;00;70.900:[GERADE];183.400;70.900;91.700;00:[GERADE];183.400;173.000;183.400;70.900:[GERADE];00;173.000;183.400;173.000:[GERADE];00;70.900;00;173.000:"
                }
            dataReader.Close();
        }


        #region Windows Form Designer generated code
    
    }

        #endregion

class Line
{

    private float X1, X2, Y1, Y2, X, Y;
    public Line(float X1, float Y1, float X2, float Y2, float X, float Y)
    {
        this.X1 = X1;
        this.Y1 = Y1;
        this.X2 = X2;
        this.Y2 = Y2;
        this.X = X;
        this.Y = Y;
    }


    public void drawLine(Graphics drawer, float width, float height, float scale, )
    {
           
        float x1 = (width - this.X1 * scale);
        float y1 = (height - this.Y1 * scale);     
        float x2 = (width - this.X2 * scale);
        float y2 = (height - this.Y2 * scale);
        drawer.DrawLine(Pens.Green, x1, y1, x2, y2);
    }
}

    public class FigurePart {

        public const int BOGEN_TYPE = 0;
        public const int GERADE_TYPE = 1;

        int mType;
        private float X1, Y1, X2, Y2, Angle;
        private String nextToAdd;
        private float startX, startY;
        public FigurePart(String type, float startX, float startY) {
            mType = type.Equals("[BOGEN]") ? BOGEN_TYPE : GERADE_TYPE;
            nextToAdd = "X1";
            this.startX = startX;
            this.startY = startY;
        }

        public void addCoord(String coord) {
            float number = 0.0f;
            try
            {
                coord = coord.Replace(".", ",");
                number = float.Parse(coord) * 10;
            }
            catch (FormatException e) {
                Console.WriteLine(e);
            }
            switch (nextToAdd) {
                case "X1": X1 = number + startX; nextToAdd = "Y1"; break;
                case "Y1": Y1 = number + startY; nextToAdd = "X2"; break;
                case "X2": X2 = number + startX; nextToAdd = "Y2"; break;
                case "Y2": Y2 = number + startY; nextToAdd = "Angle"; break;
                case "Angle": Angle = number; nextToAdd = ""; break;
            }
        }

        public float[] getLine() {
            return new float[] { X1, Y1, X2, Y2 };
        }
    }
    public class Figure {

        List<FigurePart> mParts = new List<FigurePart>();

        public void drawFigure(Graphics drawer, float width, float height, float scale)
        {
            float[][] coordsList = new float[mParts.Count][];
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.FillMode = FillMode.Winding;

            float endX = 0f, endY = 0f;
            for (int i = mParts.Count - 1; i >= 0; i--){

                float[] line = mParts[i].getLine();
                float x1 = (width - line[0] * scale);
                float y1 = (height - line[1] * scale);
                float x2 = (width - line[2] * scale);
                float y2 = (height - line[3] * scale);             
                graphicsPath.AddLine(x1, y1, x2, y2);
                coordsList[i] = new float[2];
                coordsList[i][0] = x1;
                coordsList[i][1] = y1;
            }


            drawer.FillPath(Brushes.Red, graphicsPath);

            var font = new System.Drawing.Font("Arial", 15);
            var color = new SolidBrush(Color.White);
            //var rectCenter = { x: X1 + X / 2, y: Y1 + Y / 2}
            var drawFormat = new StringFormat();
            drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Center;

            for (int i = 0; i < coordsList.Length; i++) {

                drawer.DrawString("" + coordsList[i][0] + "; " + coordsList[i][1] , font, color, coordsList[i][0], coordsList[i][1], drawFormat);
            }
        }

        public void addPart(FigurePart part) {
            mParts.Add(part);
        }

    }
}