// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using UnitsNet.Units;

    /// <summary>
    /// FahrzeugModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class VehiclesModel : BaseEntityModel
    {
        /// <summary>
        /// Gets the front a base unit.
        /// </summary>
        /// <value>The front a base unit.</value>
        // [NotMapped] public static UnitsNet.Units.AreaUnit FrontABaseUnit { get =>
        // UnitsNet.Units.AreaUnit.SquareMeter; }

        /// <summary>
        /// Gets the gewicht base unit.
        /// </summary>
        /// <value>The gewicht base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.MassUnit GewichtBaseUnit { get => UnitsNet.Units.MassUnit.Kilogram; }

        /// <summary>
        /// Gets or sets the beschreibung.
        /// </summary>
        /// <value>The beschreibung.</value>
        public string Beschreibung { get; set; }

        /// <summary>
        /// Gets or sets the cw.
        /// </summary>
        /// <value>The cw.</value>
        // public double? Cw { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VehiclesModel" /> is
        /// deletable.
        /// </summary>
        /// <value><c>true</c> if deletable; otherwise, <c>false</c>.</value>
        [Required]
        public bool Deletable { get; set; }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public virtual DynoModel Dyno { get; set; }

        /// <summary>
        /// Gets or sets the front a.
        /// </summary>
        /// <value>The front a.</value>
        // public double? FrontA { get; set; }

        /// <summary>
        /// Gets or sets the front a unit.
        /// </summary>
        /// <value>The front a unit.</value>
        // [NotMapped] public UnitsNet.Units.AreaUnit? FrontAUnit { get =>
        // this._FrontAUnit ?? FrontABaseUnit; set { if (this.FrontA.HasValue) {
        // UnitsNet.UnitConverter.TryConvert( this.FrontA.Value, this.FrontAUnit, value,
        // out double convertedValue);

        // if (UnitSettings.RoundOnUnitChange) { this.FrontA = Math.Round(convertedValue,
        // UnitSettings.RoundingAccuracy); } else { this.FrontA = convertedValue; } }

        // this._FrontAUnit = value; } }

        /// <summary>
        /// Gets or sets the gewicht.
        /// </summary>
        /// <value>The gewicht.</value>
        public double? Gewicht { get; set; }

        /// <summary>
        /// Gets or sets the unit gewicht.
        /// </summary>
        /// <value>The unit gewicht.</value>
        [NotMapped]
        public UnitsNet.Units.MassUnit? GewichtUnit
        {
            get => this._GewichtUnit ?? GewichtBaseUnit;
            set
            {
                if (this.Gewicht.HasValue)
                {

                    UnitsNet.UnitConverter.TryConvert(
                       this.Gewicht.Value,
                       this.GewichtUnit,
                       value,
                       out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.Gewicht = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.Gewicht = convertedValue;
                    }
                }

                this._GewichtUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the motor.
        /// </summary>
        /// <value>The motor.</value>
        [ForeignKey("MotorId")]
        public virtual MotorModel Motor { get; set; }

        /// <summary>
        /// Gets or sets the motor identifier.
        /// </summary>
        /// <value>The motor identifier.</value>
        public int? MotorId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the uebersetzung.
        /// </summary>
        /// <value>The uebersetzung.</value>
        // public double? Uebersetzung { get; set; }

        /// <summary>
        /// Gets or sets the front a unit.
        /// </summary>
        /// <value>The front a unit.</value>
        // [NotMapped] private AreaUnit? _FrontAUnit { get; set; }

        /// <summary>
        /// Gets or sets the gewicht unit.
        /// </summary>
        /// <value>The gewicht unit.</value>
        [NotMapped]
        private MassUnit? _GewichtUnit { get; set; }
    }
}