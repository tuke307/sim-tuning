// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using UnitsNet.Units;

    /// <summary>
    /// VergaserModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class VergaserModel : BaseEntityModel
    {
        /// <summary>
        /// Gets the durchmesser d base unit.
        /// </summary>
        /// <value>The durchmesser d base unit.</value>
        [NotMapped]
        public static LengthUnit DurchmesserDBaseUnit { get => LengthUnit.Millimeter; }

        /// <summary>
        /// Gets or sets the benzin luft f.
        /// </summary>
        /// <value>The benzin luft f.</value>
        public double? BenzinLuftF { get; set; }

        /// <summary>
        /// Gets or sets the durchmesser d.
        /// </summary>
        /// <value>The durchmesser d.</value>
        public double? DurchmesserD { get; set; }

        /// <summary>
        /// Gets or sets the durchmesser d unit.
        /// </summary>
        /// <value>The durchmesser d unit.</value>
        [NotMapped]
        public LengthUnit? DurchmesserDUnit
        {
            get => this._DurchmesserDUnit ?? DurchmesserDBaseUnit;
            set
            {
                this.DurchmesserD = ConvertValueForUnit(this.DurchmesserD, this.DurchmesserDUnit, value);
                this._DurchmesserDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the einlass.
        /// </summary>
        /// <value>The einlass.</value>
        [ForeignKey("EinlassId")]
        public virtual EinlassModel Einlass { get; set; }

        /// <summary>
        /// Gets or sets the einlass identifier.
        /// </summary>
        /// <value>The einlass identifier.</value>
        public int EinlassId { get; set; }

        /// <summary>
        /// Gets or sets the durchmesser d unit.
        /// </summary>
        /// <value>The durchmesser d unit.</value>
        [NotMapped]
        private LengthUnit? _DurchmesserDUnit { get; set; }
    }
}