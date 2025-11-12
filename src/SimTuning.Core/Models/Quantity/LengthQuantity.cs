// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UnitsNet.Units;

    /// <summary>
    /// LengthQuantity.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{SimTuning.Core.UnitListItem}" />
    public class LengthQuantity : ObservableCollection<UnitListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LengthQuantity" /> class.
        /// </summary>
        public LengthQuantity()
            : base()
        {
            var customLengthUnits = new List<Enum>()
            {
                LengthUnit.Micrometer,
                LengthUnit.Millimeter,
                LengthUnit.Centimeter,
                LengthUnit.Decimeter,
                LengthUnit.Meter,
                LengthUnit.Kilometer,
            };

            foreach (Enum unitValue in customLengthUnits)
            {
                this.Add(new UnitListItem(unitValue));
            }
        }
    }
}