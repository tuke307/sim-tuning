using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimTuning.Core.Models
{
    public class DataPoint
    {
        public double X { get; set; }

        public double Y { get; set; }

        public DataPoint()
        {
        }

        public DataPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}