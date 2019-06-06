using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using OxyPlot;

namespace DataAnalysisTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OxyPlotViewModel oxyPlot = new OxyPlotViewModel();
        private MainWindowViewModel mainView = new MainWindowViewModel();

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new
            {
                oxyPlot,
                mainView
            };
            oxyPlot.mainView = mainView;
            this.mainView.ParameterChanged += new EventHandler(UpdateNudParameters);
        }
        #endregion

        #region Window Size
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var plotView = (OxyPlot.Wpf.PlotView)this.FindName("CscanView");

            plotView.LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            var plotView = (OxyPlot.Wpf.PlotView)this.FindName("CscanView");

            if (plotView.Model != null)
            {
                if (plotView.Model.Axes != null && plotView.Model.Axes.Count != 0)
                {
                    var widthAdjustment = plotView.Model.PlotArea.Width - plotView.Model.PlotArea.Height;

                    plotView.Width = plotView.ActualWidth - widthAdjustment;
                }
            }
        }
        #region UNHANDLED
        private bool isDataLoaded = false;
        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            oxyPlot.UpdateView();
        }

        private void btnSaveData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOthers_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool isWithinCircleCursor(double centerX, double centerY, double mouseX, double mouseY, double radius)
        {
            double diffX = centerX - mouseX;
            double diffY = centerY - mouseY;
            return (diffX * diffX + diffY * diffY) <= radius * radius;
        }

        private double WindowHeight = 0;
        private double WindowWidth = 0;

        private void CscanView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OxyPlot.Wpf.PlotView p = (sender as OxyPlot.Wpf.PlotView);

            if (isDataLoaded)
            {

                double windowRatio = p.Model.PlotArea.Width / p.Model.PlotArea.Height;
                WindowWidth = 100;
                WindowHeight = 100 / windowRatio;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftShift) | (Keyboard.IsKeyDown(Key.RightShift))))
            {

            }
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyUp(Key.LeftShift) | (Keyboard.IsKeyUp(Key.RightShift))))
            {

            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        #endregion UNHANDLED

        #endregion WindowSize

        #region Wear Surfaces
        string path1 = string.Empty;
        string path2 = string.Empty;
        string path3 = string.Empty;
        private int WearSurfaceIndex = 0;



        StackPanel stackpanel;
        CheckBox chkBox;
        Button removeButton;
        TextBlock txbWearFacePath;
        private void AddWearSurfacesUIs()
        {
            stackpanel = new StackPanel();
            stackpanel.Orientation = Orientation.Horizontal;
            stackpanel.Background = Brushes.Green;

            chkBox = new CheckBox();
          //  chkBox.VerticalAlignment = VerticalAlignment.Center;
          //  chkBox.HorizontalAlignment = HorizontalAlignment.Center;
            chkBox.IsChecked = true;
            chkBox.Checked += ChkBox_Checked;
            chkBox.Unchecked += ChkBox_Unchecked;

            removeButton = new Button();
            Image img = new Image();
            string relativePath = @"Image/delete.png";
            img.Source = new BitmapImage(new Uri(relativePath, UriKind.Relative));
            StackPanel stackPnl = new StackPanel();
            stackPnl.Orientation = Orientation.Vertical;
            stackPnl.Children.Add(img);
            removeButton.Content = stackPnl;
            removeButton.Width = 14;
            removeButton.Height = 14;
            removeButton.Click += RemoveButton_Click;
            
            txbWearFacePath = new TextBlock();
            txbWearFacePath.Text = path3;
            txbWearFacePath.ToolTip = path3;

            stackpanel.Children.Add(removeButton);
            stackpanel.Children.Add(chkBox);
            stackpanel.Children.Add(txbWearFacePath);

            this.stpWearSurfaces.Children.Add(stackpanel);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

            int index = this.stpWearSurfaces.Children.IndexOf((sender as Button).Parent as StackPanel);
            this.stpWearSurfaces.Children.Remove(((sender as Button).Parent as StackPanel));
            this.oxyPlot.DeleteWearSurface(index);
            this.WearSurfaceIndex--;
        }

        private void ChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            int index = this.stpWearSurfaces.Children.IndexOf((sender as CheckBox).Parent as StackPanel);
            (((sender as CheckBox).Parent as StackPanel)).Background = Brushes.Red;
            this.oxyPlot.SetWearSurfaceVisibility(index, false);
        }

        private void ChkBox_Checked(object sender, RoutedEventArgs e)
        {
            int index = this.stpWearSurfaces.Children.IndexOf((sender as CheckBox).Parent as StackPanel);
            (((sender as CheckBox).Parent as StackPanel)).Background = Brushes.Green;
            this.oxyPlot.SetWearSurfaceVisibility(index, true);
        }

        private void ReadCSVData(string fileName)
        {
            string path = fileName;
            List<DataPoint> listA = new List<DataPoint>();
           
            using (var reader = new StreamReader(path))
            {
                Point p = new Point();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    p.X = Convert.ToDouble(values[0].Remove(0, 2));
                    p.Y = Convert.ToDouble(values[1].Remove(values[1].Length - 2, 2));
                    listA.Add(new DataPoint(p.X, p.Y));
                }
            }
            if (this.mainView.WearSurfacePointLists == null)
                this.mainView.WearSurfacePointLists = new List<List<DataPoint>>();

            this.mainView.WearSurfacePointLists.Add(listA);
            this.WearSurfaceIndex++;
        }

        #endregion Wear Surfaces

        #region Event

        #region CheckBoxEvent
        private void OnCircleCursorEnableChecked(object sender, RoutedEventArgs e)
        {
            this.mainView.IsCircleCursorEnable = this.chkCircleCursorEnable.IsChecked == true ? true : false;
        }


        #endregion

        #region NUDEvent

        private void OnNudTextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((sender as System.Windows.Forms.Control).Text))
                (sender as System.Windows.Forms.Control).Text = (sender as System.Windows.Forms.NumericUpDown).Value.ToString();
        }

        private void OnCircleCursorCenterXChanged(object sender, EventArgs e)
        {
            this.mainView.CircleCursorCenterX = Convert.ToDouble(this.nudCircleCursorCenterX.Value);
            this.oxyPlot.UpdateCursors();
            this.oxyPlot.UpdateData();
            this.oxyPlot.UpdatePlot();
        }

        private void OnCircleCursorCenterYChanged(object sender, EventArgs e)
        {
            this.mainView.CircleCursorCenterY = Convert.ToDouble(this.nudCircleCursorCenterY.Value);
            this.oxyPlot.UpdateCursors();
            this.oxyPlot.UpdateData();
            this.oxyPlot.UpdatePlot();
        }

        private void OnCircleCursorRadiusChanged(object sender, EventArgs e)
        {
            if (!this.oxyPlot.isMouseAction)
            {
                this.mainView.CircleCursorRadius = Math.Max(Convert.ToDouble(nudCircleCursorRadius.Value * 2), 0.00);
                this.oxyPlot.UpdateCursors();
                this.oxyPlot.UpdateAxis();
                this.oxyPlot.UpdateData();
                this.oxyPlot.UpdatePlot();
            }
        }

        private void OnCircleCursorAngleChanged(object sender, EventArgs e)
        {
            if (Convert.ToDouble(nudCircleCursorAngle.Value) >= 360)
            {
                nudCircleCursorAngle.Value = 0;
            }
            if (Convert.ToDouble(nudCircleCursorAngle.Value) < 0)
            {
                nudCircleCursorAngle.Value = 360 + nudCircleCursorAngle.Value;
            }
            if (!this.oxyPlot.isMouseAction)
            {
                this.mainView.CircleCursorAngle = Convert.ToDouble(nudCircleCursorAngle.Value);
                this.oxyPlot.UpdateCursors();
                this.oxyPlot.UpdateData();
                this.oxyPlot.UpdatePlot();
            }
        }

        private void OnGate1ValueChanged(object sender, EventArgs e)
        {
            this.mainView.Gate1Position = Math.Max(Convert.ToDouble(nudGate1Value.Value), 0.00);
            this.oxyPlot.UpdateCursors();
            this.oxyPlot.UpdateData();
            this.oxyPlot.UpdatePlot();
            this.mainView.GateDataRange = Math.Max(
                Math.Abs(this.mainView.Gate2Position - this.mainView.Gate1Position), 0.00).ToString("f2");
        }

        private void OnGate2ValueChanged(object sender, EventArgs e)
        {
            this.mainView.Gate2Position = Math.Max(Convert.ToDouble(nudGate2Value.Value), 0.00);
            this.oxyPlot.UpdateCursors();
            this.oxyPlot.UpdateData();
            this.oxyPlot.UpdatePlot();
            this.mainView.GateDataRange = Math.Max(
                Math.Abs(this.mainView.Gate2Position - this.mainView.Gate1Position), 0.00).ToString("f2");
        }

        #endregion NUDEvent

        #region TextBoxEvent
        #endregion TextBoxEvent

        #region ButtonEvent
        private void OnAddWearSurface(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = Properties.Resources.LOAD_SURFACE_FILE,
                Filter = Properties.Resources.CSV_FILE_EXT,
                Multiselect = true
            };
            if (ofd.ShowDialog().Value)
            {
                path1 = System.IO.Path.GetDirectoryName(ofd.FileName);//File directory
                path2 = ofd.FileName;//File full path
                path3 = ofd.SafeFileName;//File name
                this.ReadCSVData(path2);
                this.AddWearSurfacesUIs();
                this.oxyPlot.AddWearSurface();
            }
        }

        #endregion ButtonEvent

        #region Other Events
        public void UpdateNudParameters(object sender, EventArgs e)
        {
            this.nudCircleCursorCenterX.Value = Math.Max(this.nudCircleCursorCenterX.Minimum,
                        Math.Min((decimal)this.mainView.CircleCursorCenterX, this.nudCircleCursorCenterX.Maximum));
            this.nudCircleCursorCenterY.Value = Math.Max(this.nudCircleCursorCenterY.Minimum,
                        Math.Min((decimal)this.mainView.CircleCursorCenterY, this.nudCircleCursorCenterY.Maximum));
            //this.nudCircleCursorRadius.Value = Math.Max(this.nudCircleCursorRadius.Minimum,
            //           Math.Min((decimal)this.mainView.CircleCursorRadius, this.nudCircleCursorRadius.Maximum));
            this.nudGate1Value.Value = Math.Max(this.nudGate1Value.Minimum,
                        Math.Min((decimal)this.mainView.Gate1Position, this.nudGate1Value.Maximum));
            this.nudGate2Value.Value = Math.Max(this.nudGate2Value.Minimum,
                       Math.Min((decimal)this.mainView.Gate2Position, this.nudGate2Value.Maximum));
        }
        #endregion

        #endregion Event

    }
}
