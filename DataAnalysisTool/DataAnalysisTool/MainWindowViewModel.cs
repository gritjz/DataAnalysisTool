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
        //update nud
        public event EventHandler ParameterChanged;

        #region Properties

       


        #region G-Cursor
        private bool isGeoCursorEnable;
        private double geoCursorCenterX;
        private double geoCursorCenterY;
        private double geoCursorRadius;
        private double geoCursorAngle;
        public bool IsGeoCursorEnable
        {
            set
            {
                this.isGeoCursorEnable = value;

            }
            get
            {
                return this.isGeoCursorEnable;
            }
        }
        public double GeoCursorCenterX
        {
            set
            {
                this.geoCursorCenterX = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.geoCursorCenterX;
            }
        }

        public double GeoCursorCenterY
        {
            set
            {
                this.geoCursorCenterY = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.geoCursorCenterY;
            }
        }

        public double GeoCursorRadius
        {
            set
            {
                this.geoCursorRadius = value;
            }
            get
            {
                return this.geoCursorRadius;
            }
        }

        public double GeoCursorAngle
        {
            set
            {
                this.geoCursorAngle = value;
            }
            get
            {
                return this.geoCursorAngle;
            }
        }
        #endregion D-Cursor

        #region D-Cursor
        private bool isDCursorEnable;
        private double dCursorCenterX;
        private double dCursorCenterY;
        private double dCursorRadius;
        private double dCursorAngle;
        public bool IsDCursorEnable
        {
            set
            {
                this.isDCursorEnable = value;

            }
            get
            {
                return this.isDCursorEnable;
            }
        }
        public double DCursorCenterX
        {
            set
            {
                this.dCursorCenterX = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.dCursorCenterX;
            }
        }

        public double DCursorCenterY
        {
            set
            {
                this.dCursorCenterY = value;
                if (this.ParameterChanged != null)
                    this.ParameterChanged(this, null);
            }
            get
            {
                return this.dCursorCenterY;
            }
        }

        public double DCursorRadius
        {
            set
            {
                this.dCursorRadius = value;
            }
            get
            {
                return this.dCursorRadius;
            }
        }

        public double DCursorAngle
        {
            set
            {
                this.dCursorAngle = value;
            }
            get
            {
                return this.dCursorAngle;
            }
        }
        #endregion D-Cursor



        #region Gate

        private double gate1Position;
        private double gate2Position;
        private string gateDataRange;

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
        #endregion Gate

        #region Wear Surfaces
        
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
