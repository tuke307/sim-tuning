// Copyright (c) 2021 tuke productions. All rights reserved.
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using MathNet.Numerics;
using SimTuning.Core.Models;
using SimTuning.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimTuning.Core.ModuleLogic
{
    /// <summary>
    /// Dyno Logik.
    /// </summary>
    public static class DynoLogic
    {
        /// <summary>
        /// Funktion zur Anwendung der Formel zur Kraftberechnung.
        /// Formel: P=m*v*a+cwv^y+1.
        /// </summary>
        /// <param name="masse">The masse.</param>
        /// <param name="geschwindigkeit">The geschwindigkeit.</param>
        /// <param name="beschleunigung">The beschleunigung.</param>
        /// <param name="cw">The cw.</param>
        /// <param name="y">The y.</param>
        /// <returns>Kraft in PS.</returns>
        public static double StrengthFormula(double masse, double geschwindigkeit, double beschleunigung, double cw = 0, double y = 0)
        {
            return (masse * geschwindigkeit * beschleunigung) + (cw * y);
        }

        public static ObservablePoint ToObservablePoint(this DataPoint dataPoint)
        {
            return new ObservablePoint(dataPoint.X, dataPoint.Y);
        }
    }
}