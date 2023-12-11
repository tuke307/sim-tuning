using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dbscan;
using LiveChartsCore.Defaults;

namespace SimTuning.Core.Models
{
    public class DataPoint : IPointData
    {
        public double X { get; set; }

        public double Y { get; set; }

        public Dbscan.Point Point { get; }

        public DataPoint() { }

        public DataPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
            Point = new Dbscan.Point(x, y);
        }
    }
}
