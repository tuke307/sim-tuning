// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UnitsNet.Units;

    /// <summary>
    /// AreaQuantity.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{SimTuning.Core.UnitListItem}" />
    public class AreaQuantity : ObservableCollection<UnitListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AreaQuantity" /> class.
        /// </summary>
        public AreaQuantity()
            : base()
        {
            var customAreaUnits = new List<Enum>()
            {
                AreaUnit.SquareMillimeter,
                AreaUnit.SquareCentimeter,
                AreaUnit.SquareDecimeter,
                AreaUnit.SquareMeter,
            };

            foreach (Enum unitValue in customAreaUnits)
            {
                this.Add(new UnitListItem(unitValue));
            }
        }
    }
}