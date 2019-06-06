using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataAnalysisTool
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ParameterChanged;

        #region Properties
        private bool isCircleCursorEnable;
        private double circleCursorCenterX;
        private double circleCursorCenterY;
        private double circleCursorRadius;
        private double circleCursorAngle;
        private double gate1Position;
        private double gate2Position;
        private string gateDataRange;
        private List<List<Point>> wearSurfaceLists;
        public bool IsCircleCursorEnable
        {
            set
            {
                this.isCircleCursorEnable = value;

            }
            get
            {
                return this.isCircleCursorEnable;
            }
        }
        public double CircleCursorCenterX
        {
            set
            {
                this.circleCursorCenterX = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.circleCursorCenterX;
            }
        }

        public double CircleCursorCenterY
        {
            set
            {
                this.circleCursorCenterY = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.circleCursorCenterY;
            }
        }

        public double CircleCursorRadius
        {
            set
            {
                this.circleCursorRadius = value;
            }
            get
            {
                return this.circleCursorRadius;
            }
        }

        public double CircleCursorAngle
        {
            set
            {
                this.circleCursorAngle = value;
            }
            get
            {
                return this.circleCursorAngle;
            }
        }

        public double Gate1Position
        {
            set
            {
                this.gate1Position = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.gate1Position;
            }
        }
        public double Gate2Position
        {
            set
            {
                this.gate2Position = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.gate2Position;
            }
        }
        public string GateDataRange
        {
            set
            {
                this.gateDataRange = value;
                MainWindowNotifyPropertyChanged("GateDataRange");
            }
            get
            {
                return this.gateDataRange;
            }
        }
        #region Wear Surfaces
        public WearSurfaceLine l { get; set; }

        public List<List<DataPoint>> WearSurfacePointLists
        {
            set;
            get;
        }

        public int WearSurfaceCount
        {
            get
            {
                if (WearSurfacePointLists == null||WearSurfacePointLists.Count<1)
                    return 0;
                else
                    return WearSurfacePointLists.Count;
            }
        }
        #endregion
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void MainWindowNotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        
    }
}
