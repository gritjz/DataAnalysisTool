using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDF5DotNet;
namespace DataAnalysisTool
{
    public enum AScanBitSize { e8Bits = 0, e12Bits, e16Bits, eLog8Bits };
    public static class ExtensionHelper
    {

        public static int GetSampleSize(this AScanBitSize resolution)
        {
            int result = 1;
            switch (resolution)
            {
                case AScanBitSize.e8Bits:
                case AScanBitSize.eLog8Bits:
                    result = 1;
                    break;
                case AScanBitSize.e12Bits:
                case AScanBitSize.e16Bits:
                    result = 2;
                    break;
            }
            return result;
        }
    }

    class GetHdf5DataSet

    {




        /// <summary>
        /// Slice data
        /// </summary>
        public class TOSOHSliceData
        {
            public double[][] sliceData;
            public double[] cycleData;

            public int pointQuantity
            {
                set;
                get;
            }

            public int cycleQuantity
            {
                set;
                get;
            }

            public AScanBitSize Resolution
            {
                get;
                set;
            }

            public void InitSliceData()
            {
                if (this.sliceData == null || this.sliceData.Length != this.cycleQuantity)
                {
                    this.sliceData = new double[this.cycleQuantity][];
                }

                int sampleSize = this.Resolution.GetSampleSize();

                for (int i = 0; i < this.cycleQuantity; i++)
                {
                    Array.Resize(ref this.sliceData[i], (int)(this.pointQuantity * sampleSize));
                }

            }

            internal void SetData(double[][] slice)
            {
                for (int i = 0; i < this.cycleQuantity; i++)
                {
                    Buffer.BlockCopy(slice[i], 0, this.sliceData[i], 0, Math.Min(slice[i].Length, this.sliceData[i].Length));
                }
            }
        }

        /// <summary>
        /// Data file format
        /// </summary>
        public class TOSOHData
        {
            public Dictionary<int, TOSOHSliceData> scanData = new Dictionary<int, TOSOHSliceData>();
            public TOSOHSliceData scanDataInfo = new TOSOHSliceData();
            
            #region Properties
            public int ScanDataN_Z
            {
                get
                {
                    return this.scanDataInfo.pointQuantity;
                }
                set
                {
                    this.scanDataInfo.pointQuantity = value;
                }
            }

            public int ScanDataN_X
            {
                get
                {
                    return this.scanDataInfo.cycleQuantity;
                }
                set
                {
                    this.scanDataInfo.cycleQuantity = value;
                }
            }

            public AScanBitSize Resolution
            {
                get
                {
                    return this.scanDataInfo.Resolution;
                }
                set
                {
                    this.scanDataInfo.Resolution = value;
                }
            }

            public int ScanDataGrid_X
            {
                get;
                set;
            }

            public int ScanDataGrid_Y
            {
                set;
                get;
            }

            private int AScanSizeInBytes
            {
                get
                {
                    return (int)(this.ScanDataN_Z * (int)(this.Resolution));
                }
            }
            #endregion

            #region Functions
            private void InitSliceData(TOSOHSliceData cycleData)
            {
                cycleData.cycleQuantity = this.scanDataInfo.cycleQuantity;
                cycleData.pointQuantity = this.scanDataInfo.pointQuantity;
                cycleData.Resolution = this.scanDataInfo.Resolution;
                cycleData.InitSliceData();
            }

            private void InitCycleData(TOSOHSliceData cycleData, double[][] seqData)
            {
                this.InitSliceData(cycleData);
                cycleData.SetData(seqData);
            }

            private bool ValidateData(double[][] sequenceData)
            {
                if (null == sequenceData || sequenceData.Length <= 0)
                    return false;
                if (sequenceData.Length != this.ScanDataN_X)
                    return false;
                if (sequenceData[0].Length <= 0 || sequenceData[0].Length < this.AScanSizeInBytes)
                    return false;
                return true;
            }

            public bool SetSequenceData(int sequenceNo, double[][] sequenceData)
            {
                if (!this.ValidateData(sequenceData))
                    return false;
                TOSOHSliceData sliceData = null;
                bool found = this.scanData.TryGetValue(sequenceNo, out sliceData);
                if (!found)
                {
                    sliceData = new TOSOHSliceData();
                    this.InitCycleData(sliceData, sequenceData);
                    this.scanData[sequenceNo] = sliceData;
                }
                else
                {
                    this.InitCycleData(sliceData, sequenceData);
                }
                return true;
            }


            #endregion
        }
    }
}
