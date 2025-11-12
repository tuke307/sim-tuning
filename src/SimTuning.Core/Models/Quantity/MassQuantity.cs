// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UnitsNet.Units;

    /// <summary>
    /// MassQuantity.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{SimTuning.Core.UnitListItem}" />
    public class MassQuantity : ObservableCollection<UnitListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassQuantity" /> class.
        /// </summary>
        public MassQuantity()
            : base()
        {
            var customMassUnits = new List<Enum>()
            {
                MassUnit.Milligram,
                MassUnit.Gram,
                MassUnit.Decigram,
                MassUnit.Kilogram,
                MassUnit.Tonne,
            };

            foreach (Enum unitValue in customMassUnits)
            {
                this.Add(new UnitListItem(unitValue));
            }
        }
    }
}