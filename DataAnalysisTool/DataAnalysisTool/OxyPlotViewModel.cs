using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using OxyPlot.Annotations;
using System.Windows;

namespace DataAnalysisTool
{
    public class OxyPlotViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public PlotModel CScanModel { get; set; }
        public PlotModel DScanModel { get; set; }
        public MainWindowViewModel mainView = new MainWindowViewModel();

        public double[,] DefaultData;
        public double[,] CScanRawData;
        public double[,] DScanRawData;



        public double[,,] Original_data;
        public double[,] CircleMaxData;

        private HeatMapSeries CScanSeries;
        private HeatMapSeries DScanSeries;
        private WearSurfaceLine WearSurface;
        private List<WearSurfaceLine> WearSurfaces;

        private LinearAxis CScanXaxis;
        private LinearAxis CScanYaxis;

        private LinearAxis DScanXaxis;
        private LinearAxis DScanYaxis;

        private LinearColorAxis CScanPalette;
        private LinearColorAxis DScanPalette;

        private CircleCursor DataCursor;
        private GateCursor GateCursor1;
        private GateCursor GateCursor2;

        #region Constructor

        public OxyPlotViewModel()
        {
            Init();
            InitPlot();
        }

        #endregion

        #region Properties

        public Point CenterPoint { get; set; }

        public bool isMouseAction
        {
            get
            {
                return this.DataCursor.MousePressed || this.GateCursor1.MousePressed || this.GateCursor2.MousePressed;
            }
        }

        #endregion

        #region Data processing

        public void UpdateData()
        {
            this.UpdateCscanData();
            this.UpdateDscanData();
            this.CScanSeries.Data = CScanRawData;
            this.DScanSeries.Data = DScanRawData;
        }

        private void UpdateCscanData()
        {
            for (int x = 0; x < 100; ++x)
            {
                for (int y = 0; y < 100; ++y)
                {
                    int index = FindMaximumInGates(x, y, this.GateCursor1.Position, this.GateCursor2.Position);
                    CScanRawData[x, y] = Original_data[x, y, index];
                }
            }
        }

        private void UpdateDscanData()
        {

            DataPoint[] Points = this.DataCursor.RadiusPoints;
            this.DScanRawData = new double[(int)Math.Round(this.DataCursor.Radius), 100];

            double max = Double.MinValue;
            for (int r = 0; r < (int)Math.Round(this.DataCursor.Radius); ++r)
            {
                for (int z = 0; z < 100; ++z)
                {
                    for (int angle = 0; angle < 360; angle++)
                    {
                        // // double b = D_data[(int)Points[r].X, (int)Points[r].Y, z];
                        Point p = GetPositionOnCircle(this.DataCursor.CenterPoint, angle, r);
                        max = Math.Max(Original_data[(int)p.X, (int)p.Y, z], max);
                    }
                    DScanRawData[r, z] = max;
                }
            }
        }

        private Point GetPositionOnCircle(Point center, double angle, double radius)
        {
            int x = 0; int y = 0;
            x = Convert.ToInt32(Math.Round(center.X + (radius * Math.Cos(angle * Math.PI / 180))));
            y = Convert.ToInt32(Math.Round(center.Y + (radius * Math.Sin(angle * Math.PI / 180))));
            return new Point(x, y);
        }

        private double GetMinimumvalueInCircle()
        {
            double res = 0;




            return res;
        }
        
        /// <summary>
        /// Find Highest Value between Gate 1 and Gate 2
        /// </summary>
        private int FindMaximumInGates(int x, int y, double gate1Position, double gate2Position)
        {
            int index = 0;
            int gate1 = Convert.ToInt32(Math.Floor(Math.Min(gate1Position, gate2Position)));
            int gate2 = Convert.ToInt32(Math.Ceiling(Math.Max(gate1Position, gate2Position)));
            double max = Double.MinValue;
            for (int i = gate1; i < gate2; ++i)
            {
                if (Original_data[x, y, i] > max)
                {
                    max = Original_data[x, y, i];
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// Find Lowest Value between Gate 1 and Gate 2
        /// </summary>
        private int FindMinimumInGates(int x, int y, double gate1Position, double gate2Position)
        {
            int index = 0;
            int gate1 = Convert.ToInt32(Math.Floor(gate1Position));
            int gate2 = Convert.ToInt32(Math.Ceiling(gate2Position));
            double Value = Original_data[x, y, gate1];
            double min = Double.MaxValue;
            for (int i = gate1; i < gate2; ++i)
            {
                if (Original_data[x, y, i] < min)
                {
                    min = Original_data[x, y, i];
                    index = i;
                }
            }
            return index;
        }

        public void SetData()
        {
            try
            {
                // generate 1d normal distribution
                double[] singleData = new double[100];
                for (int x = 0; x < 100; ++x)
                {
                    singleData[x] = Math.Exp((-1.0 / 2.0) * Math.Pow(((double)x - 50) / 20.0, 2.0));
                }

                // generate 2d normal distribution
                this.CScanRawData = new double[100, 100];
                //this.DScanRawData = new double[100, 100];
                this.Original_data = new double[100, 100, 100];
                Random random = new Random();

                for (int x = 0; x < 100; ++x)
                {
                    for (int y = 0; y < 100; ++y)
                    {
                        // data[x, y] = singleData[x] * singleData[(y) % 100] * 100;
                        for (int z = 0; z < 100; ++z)
                        {
                            Original_data[x, y, z] = random.Next(0, 100); //singleData[x] * singleData[(y) % 100] * 100;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("SetData()");
                return;
            }
        }
        #endregion

        #region Display processing

        public void UpdateView()
        {
            try
            {
                this.SetData();
                this.UpdateData();
                this.UpdateCursors();
                this.UpdatePlot();
                this.RefreshPlotModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("UpdateView()");
                return;
            }
        }

        public void Init()
        {
            this.CScanModel = new PlotModel
            {
                Title = "C-Scan",
                TitleFontSize = 30
            };
            this.DScanModel = new PlotModel
            {
                Title = "D-Scan",
                TitleFontSize = 30
            };
            this.DataCursor = new CircleCursor(this.CScanModel);

            this.GateCursor1 = new GateCursor(this.DScanModel);
            this.GateCursor2 = new GateCursor(this.DScanModel);

            this.DataCursor.CircleCursorParameterChanged += new EventHandler(UpdateDataCursorParameters);

            this.GateCursor1.GateCursorParameterChanged += new EventHandler(UpdateGateCursorParameters);
            this.GateCursor2.GateCursorParameterChanged += new EventHandler(UpdateGateCursorParameters);

            this.WearSurfaces = new List<WearSurfaceLine>();
        }

        private void UpdateDataCursorParameters(object sender, EventArgs e)
        {
            this.mainView.CircleCursorCenterX = this.DataCursor.GetCircleCursor().X;
            this.mainView.CircleCursorCenterY = this.DataCursor.GetCircleCursor().Y;
        }

        private void UpdateGateCursorParameters(object sender, EventArgs e)
        {
            this.mainView.Gate1Position = this.GateCursor1.GetGateCursor().Y;
            this.mainView.Gate2Position = this.GateCursor2.GetGateCursor().Y;
        }

        public void UpdateCursors()
        {
            //Update Gate Cursor
            if (!this.GateCursor1.MousePressed && !this.GateCursor2.MousePressed && !this.isMouseAction)
            {
                this.GateCursor1.SetParameters(LineAnnotationType.Horizontal, this.mainView.Gate1Position, OxyColors.Blue);
                this.GateCursor2.SetParameters(LineAnnotationType.Horizontal, this.mainView.Gate2Position, OxyColors.Red);
                this.GateCursor1.UpdateGateCursor();
                this.GateCursor2.UpdateGateCursor();
            }
            //Update CircleCursor
            if (!this.DataCursor.MousePressed && !this.isMouseAction)
            {
                this.DataCursor.SetParameters(
                    new Point(Convert.ToDouble(this.mainView.CircleCursorCenterX), this.mainView.CircleCursorCenterY),
                    this.mainView.CircleCursorRadius,
                    this.mainView.CircleCursorAngle);
                this.DataCursor.UpdateCursor();
            }
        }

        public void InitPlot()
        {
            this.CScanPalette = new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Maximum = 100,
                Minimum = 0
            };

            this.DScanPalette = new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Maximum = 100,
                Minimum = 0
            };

            CScanSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 100,
                Y0 = 0,
                Y1 = 100,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                //  Data = CScanRawData
            };

            DScanSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 100,
                Y0 = 0,
                Y1 = 100,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                // Data = DScanRawData
            };

            CScanYaxis = new LinearAxis
            {
                Minimum = 0,
                Maximum = 100,
                MinimumPadding = 0,
                MaximumPadding = 0,
                Angle = 45,
                Position = AxisPosition.Left
            };

            DScanYaxis = new LinearAxis
            {
                Minimum = 0,
                Maximum = 100,
                MinimumPadding = 0,
                MaximumPadding = 0,
                Angle = 45,
                Position = AxisPosition.Left
            };

            CScanXaxis = new LinearAxis
            {
                Minimum = 0,
                Maximum = 100,
                MinimumPadding = 0,
                MaximumPadding = 0,
                Angle = 45,
                Position = AxisPosition.Bottom
            };

            DScanXaxis = new LinearAxis
            {
                Minimum = 0,
                Maximum = 100,
                MinimumPadding = 0,
                MaximumPadding = 0,
                Angle = 45,
                Position = AxisPosition.Bottom
            };
        }
        
        public void ClearPlot()
        {
            CScanModel.Axes.Clear();
            DScanModel.Axes.Clear();
            CScanModel.Series.Clear();
            DScanModel.Series.Clear();
        }

        public void UpdateAxis()
        {
            this.DScanXaxis.Maximum = this.DataCursor.Radius;
        }

      
        public void AddWearSurface()
        {
            this.WearSurface = new WearSurfaceLine(DScanModel);
       
            this.WearSurface.UpdateData(this.mainView.WearSurfacePointLists[this.mainView.WearSurfaceCount-1]);
            this.WearSurfaces.Add(this.WearSurface);
            this.UpdateWearSurfaces();
        }

        public void DeleteWearSurface(int index)
        {
            DScanModel.Series.RemoveAt(index + 1);
            this.RefreshPlotModel();
        }

        public void SetWearSurfaceVisibility(int index, bool visible)
        {

            DScanModel.Series[index+1].IsVisible = visible;
            this.RefreshPlotModel();
        }
        //private List<DataPoint> templist = new List<DataPoint>();
        //public void INITTEMPSURFACEPOINTS(int index)
        //{
        //    templist.Clear();
        //    if (index == 0)
        //    {
        //        templist.Add(new DataPoint(0, 10));
        //        templist.Add(new DataPoint(10, 20));
        //        templist.Add(new DataPoint(30, 30));
        //        templist.Add(new DataPoint(70, 40));
        //    }
        //    else
        //    {
        //        templist.Add(new DataPoint(0, 20));
        //        templist.Add(new DataPoint(20, 30));
        //        templist.Add(new DataPoint(40, 40));
        //        templist.Add(new DataPoint(60, 50));
        //    }
        //}

        public void UpdateWearSurfaces()
        {
            DScanModel.Series.Add(this.WearSurface.WearSurface);
            this.RefreshPlotModel();
        }
        
        public void UpdatePlot()
        {
            this.ClearPlot();
            CScanModel.Axes.Add(CScanXaxis);
            CScanModel.Axes.Add(CScanYaxis);
            DScanModel.Axes.Add(DScanXaxis);
            DScanModel.Axes.Add(DScanYaxis);
            CScanModel.Axes.Add(CScanPalette);
            DScanModel.Axes.Add(DScanPalette);

            CScanModel.Series.Add(CScanSeries);
            DScanModel.Series.Add(DScanSeries);
            
            CScanModel.Series.Add(this.DataCursor.GetRadius());

            if (CScanModel.Annotations.Count == 0)
            {
                CScanModel.Annotations.Add(this.DataCursor.GetCircleCursor());
            }
            else
            {
                CScanModel.Annotations[0] = this.DataCursor.GetCircleCursor();
            }
            if (DScanModel.Annotations.Count == 0)
            {
                DScanModel.Annotations.Add(this.GateCursor1.GetGateCursor());
            }
            else
            {
                DScanModel.Annotations[0] = this.GateCursor1.GetGateCursor();
            }
            if (DScanModel.Annotations.Count == 1)
            {
                DScanModel.Annotations.Add(this.GateCursor2.GetGateCursor());
            }
            else
            {
                DScanModel.Annotations[1] = this.GateCursor2.GetGateCursor();
            }
            this.RefreshPlotModel();
        }

        private void RefreshPlotModel()
        {
            CScanModel.InvalidatePlot(true);
            DScanModel.InvalidatePlot(true);
        }

        #region Cursors


        #endregion

        #endregion

        #region Others

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void OxyPlotNotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private double DistanceTo(DataPoint p1, DataPoint p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        #endregion
    }

    /// <summary>
    /// Gate Cursor Class
    /// </summary>
    public class GateCursor
    {
        private PlotModel model;
        private LineAnnotation Cursor;
        public event EventHandler GateCursorParameterChanged;

        #region Constructor
        public GateCursor(PlotModel model)
        {
            this.model = model;
            this.MousePressed = false;
            Cursor = new LineAnnotation();
            this.InitGateCursorEvent();
        }
        #endregion

        #region Properties

        public LineAnnotationType Type
        { set; get; }
        public double Position
        { set; get; }
        public OxyColor Color
        { set; get; }
        public bool MousePressed
        { set; get; }
        #endregion

        #region Public Functions
        public LineAnnotation GetGateCursor()
        {
            return this.Cursor;
        }

        public void SetParameters(LineAnnotationType type, double position, OxyColor color)
        {
            this.Type = type;
            this.Position = position;
            this.Color = color;
        }

        public void UpdateGateCursor()
        {
            model.Annotations.Clear();

            if (this.Type == LineAnnotationType.Horizontal)
            {
                this.Cursor.Y = this.Position;
            }
            else if (this.Type == LineAnnotationType.Vertical)
            {
                this.Cursor.X = this.Position;
            }

            this.Cursor.Type = this.Type;
            this.Cursor.Color = this.Color;
            this.Cursor.ClipByYAxis = false;
            this.Cursor.StrokeThickness = 2;
            this.Cursor.Text = this.Position.ToString("f2");
        }
        #endregion

        #region Private Functions
        private void InitGateCursorEvent()
        {
            this.Cursor.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                { return; }
                this.MousePressed = true;
                this.Cursor.StrokeThickness *= 2.5;
                model.InvalidatePlot(false);
                e.Handled = true;
            };
            this.Cursor.MouseMove += (s, e) =>
            {
                if (this.Type == LineAnnotationType.Horizontal)
                {
                    this.Position = Convert.ToDouble(this.Cursor.InverseTransform(e.Position).Y.ToString("f2"));
                    this.Cursor.Y = this.Position;
                }
                else if (this.Type == LineAnnotationType.Vertical)
                {
                    this.Position = Convert.ToDouble(this.Cursor.InverseTransform(e.Position).X.ToString("f2"));
                    this.Cursor.X = this.Position;
                }

                this.Cursor.Text = this.Position.ToString("f2");
                this.Cursor.TextColor = this.Color;
                if (this.GateCursorParameterChanged != null)
                    this.GateCursorParameterChanged(this, null);
                model.InvalidatePlot(false);
                e.Handled = true;
            };
            this.Cursor.MouseUp += (s, e) =>
            {
                this.Cursor.StrokeThickness /= 2.5;
                this.MousePressed = false;
                model.InvalidatePlot(false);
                e.Handled = true;
            };
        }
        #endregion
    }

    /// <summary>
    /// Circle Cursor Class
    /// </summary>
    public class CircleCursor
    {
        private PlotModel model;
        private EllipseAnnotation ellipse;
        private EllipseAnnotation dataEllipse;
        private LineSeries radiusLine;
        public event EventHandler CircleCursorParameterChanged;
        #region Constrcutor
        public CircleCursor(PlotModel model)
        {
            this.model = model;
            this.ellipse = new EllipseAnnotation();
            this.radiusLine = new LineSeries();
            this.MousePressed = false;
            this.InitCircleCursorEvent();
        }
        #endregion

        #region Properties
        public double X
        {
            set; get;
        }
        public double Y
        {
            set; get;
        }
        public double Width
        {
            set; get;
        }
        public double Height
        {
            set; get;
        }
        public OxyColor Fill
        {
            set; get;
        }
        public OxyColor Stroke
        {
            set; get;
        }
        public double StrokeThickness
        {
            set; get;
        }
        public double RadiusAngle
        {
            set; get;
        }
        public Point CenterPoint
        {
            set; get;
        }
        public bool MousePressed
        {
            set; get;
        }
        public double Radius
        {
            get
            {
                double radius = 0;
                if (radiusLine != null && radiusLine.Points.Count > 0)
                {
                    radius = Math.Sqrt(Math.Pow(radiusLine.Points[0].X - radiusLine.Points[1].X, 2) + Math.Pow(radiusLine.Points[0].Y - radiusLine.Points[1].Y, 2));
                }
                return radius;
            }
        }
        public DataPoint[] RadiusPoints
        {
            get
            {
                DataPoint[] points = null;
                if (radiusLine != null && radiusLine.Points.Count > 0)
                {
                    return this.GetRadiusPoints(this.radiusLine.Points[0], this.radiusLine.Points[1], (int)Math.Round(this.Radius));
                }
                return points;
            }
        }
        #endregion

        #region Public Functions

        public EllipseAnnotation GetCircleCursor()
        {
            return this.ellipse;
        }

        public LineSeries GetRadius()
        {
            return this.radiusLine;
        }

        public void SetParameters(Point center, double radius, double radiusAngle)
        {
            this.ellipse.X = center.X;
            this.ellipse.Y = center.Y;
            this.ellipse.Width = radius;
            this.ellipse.Height = radius;
            this.CenterPoint = center;
            this.RadiusAngle = radiusAngle;
            this.UpdateRadiusLine();
        }

        /// <summary>
        /// Add Ellipse
        /// </summary>
        /// <returns></returns>
        public void UpdateCursor()
        {
            model.Annotations.Clear();
            this.ellipse.Fill = OxyColors.Transparent;
            this.ellipse.Stroke = OxyColors.Black;
            this.ellipse.StrokeThickness = 3;
        }


        #endregion

        #region Private Functions
        private void InitCircleCursorEvent()
        {
            ellipse.MouseDown += (s, e) =>
            {
                if (e.ChangedButton != OxyMouseButton.Left)
                {
                    return;
                }
                MousePressed = true;
                ellipse.StrokeThickness = 1;
                model.InvalidatePlot(false);
                e.Handled = true;
            };
            ellipse.MouseMove += (s, e) =>
            {
                ellipse.X = ellipse.InverseTransform(e.Position).X;
                ellipse.Y = ellipse.InverseTransform(e.Position).Y;

                this.UpdateRadiusLine();
                if (this.CircleCursorParameterChanged != null)
                    this.CircleCursorParameterChanged(this, null);
                model.InvalidatePlot(false);
                e.Handled = true;
            };
            ellipse.MouseUp += (s, e) =>
            {
                MousePressed = false;
                ellipse.StrokeThickness = 3;
                e.Handled = false;
            };
        }

        private void UpdateRadiusLine()
        {
            radiusLine.Points.Clear();
            double x = 0; double y = 0;
            x = this.ellipse.X + (this.ellipse.Height / 2 * Math.Cos(RadiusAngle * Math.PI / 180));
            y = this.ellipse.Y + (this.ellipse.Height / 2 * Math.Sin(RadiusAngle * Math.PI / 180));
            radiusLine.Points.Add(new DataPoint(this.ellipse.X, this.ellipse.Y));
            radiusLine.Points.Add(new DataPoint(x, y));
        }

        private DataPoint[] GetRadiusPoints(DataPoint p1, DataPoint p2, int quantity)
        {

            DataPoint[] points = new DataPoint[quantity];
            double ydiff = p2.Y - p1.Y, xdiff = p2.X - p1.X;
            double slope = (double)(p2.Y - p1.Y) / (p2.X - p1.X);
            double x, y;
            --quantity;
            for (double i = 0; i < quantity; i++)
            {
                y = slope == 0 ? 0 : ydiff * (i / quantity);
                x = slope == 0 ? xdiff * (i / quantity) : y / slope;
                double xx = (int)Math.Round(x) + p1.X;
                double yy = (int)Math.Round(y) + p1.Y;
                if (xx < 0)
                    xx = 0;
                if (yy < 0)
                    yy = 0;

                points[(int)i] = new DataPoint(xx, yy);
            }
            points[quantity] = p2;
            return points;
        }
        #endregion

    }

    /// <summary>
    /// Wear Surface Line Class
    /// </summary>
    public class WearSurfaceLine
    {
        private PlotModel model;
        private LineSeries wearSurface;
        private List<DataPoint> dataPoints;
        public event EventHandler WearSurfaceParameterChanged;

        #region Constructor
        public WearSurfaceLine(PlotModel model)
        {
            this.model = model;
            this.dataPoints = new List<DataPoint>();
            this.InitWearSurface();
        }
        #endregion

        #region Properties

        public LineSeries WearSurface
        {
            get
            {
                return wearSurface;
            }
        }

        //public string Name
        //{
        //    set;
        //    get;
        //}

        //public int Index
        //{
        //    set;
        //    get;
        //}
        #endregion

        #region Public functions

        public void UpdateData(List<DataPoint> data)
        {
            ClearSurface();
           //update with data
            dataPoints = data;
            for (int i = 0; i < data.Count; ++i)
            {
                this.wearSurface.Points.Add(data[i]);
            }
        }

        public void ClearSurface()
        {
            this.wearSurface.Points.Clear();
        }
        
        #endregion


        #region Private Functions

        private void InitWearSurface()
        {
            wearSurface = new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.None,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5
            };
        }



        #endregion
    }
}