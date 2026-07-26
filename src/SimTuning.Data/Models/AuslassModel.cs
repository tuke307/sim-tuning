// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using UnitsNet.Units;

    /// <summary>
    /// AuslassModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class AuslassModel : BaseEntityModel
    {
        /// <summary>
        /// Gets the breite b base unit.
        /// </summary>
        /// <value>The breite b base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit BreiteBBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the durchmesser d base unit.
        /// </summary>
        /// <value>The durchmesser d base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DurchmesserDBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the flaeche a base unit.
        /// </summary>
        /// <value>The flaeche a base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.AreaUnit FlaecheABaseUnit { get => UnitsNet.Units.AreaUnit.SquareMillimeter; }

        /// <summary>
        /// Gets the hoehe h base unit.
        /// </summary>
        /// <value>The hoehe h base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit HoeheHBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the laenge l base unit.
        /// </summary>
        /// <value>The laenge l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit LaengeLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets or sets the auspuff.
        /// </summary>
        /// <value>The auspuff.</value>
        public virtual AuspuffModel Auspuff { get; set; }

        /// <summary>
        /// Gets or sets the breite b.
        /// </summary>
        /// <value>The breite b.</value>
        public double? BreiteB { get; set; }

        /// <summary>
        /// Gets or sets the breite b unit.
        /// </summary>
        /// <value>The breite b unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? BreiteBUnit
        {
            get => this._BreiteBUnit ?? BreiteBBaseUnit;
            set
            {
                this.BreiteB = ConvertValueForUnit(this.BreiteB, this.BreiteBUnit, value);
                this._BreiteBUnit = value;
            }
        }

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
        public UnitsNet.Units.LengthUnit? DurchmesserDUnit
        {
            get => this._DurchmesserDUnit ?? DurchmesserDBaseUnit;
            set
            {
                this.DurchmesserD = ConvertValueForUnit(this.DurchmesserD, this.DurchmesserDUnit, value);
                this._DurchmesserDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the flaeche a.
        /// </summary>
        /// <value>The flaeche a.</value>
        public double? FlaecheA { get; set; }

        /// <summary>
        /// Gets or sets the flaeche a unit.
        /// </summary>
        /// <value>The flaeche a unit.</value>
        [NotMapped]
        public AreaUnit FlaecheAUnit
        {
            get => this._FlaecheAUnit ?? FlaecheABaseUnit;
            set
            {
                this.FlaecheA = ConvertValueForUnit(this.FlaecheA, this.FlaecheAUnit, value);
                this._FlaecheAUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the hoehe h.
        /// </summary>
        /// <value>The hoehe h.</value>
        public double? HoeheH { get; set; }

        /// <summary>
        /// Gets or sets the hoehe h unit.
        /// </summary>
        /// <value>The hoehe h unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit HoeheHUnit
        {
            get => this._HoeheHUnit ?? HoeheHBaseUnit;
            set
            {
                this.HoeheH = ConvertValueForUnit(this.HoeheH, this.HoeheHUnit, value);
                this._HoeheHUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the laenge l.
        /// </summary>
        /// <value>The laenge l.</value>
        public double? LaengeL { get; set; }

        /// <summary>
        /// Gets or sets the laenge l unit.
        /// </summary>
        /// <value>The laenge l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit LaengeLUnit
        {
            get => this._LaengeLUnit ?? LaengeLBaseUnit;
            set
            {
                this.LaengeL = ConvertValueForUnit(this.LaengeL, this.LaengeLUnit, value);
                this._LaengeLUnit = value;
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
        public int MotorId { get; set; }

        /// <summary>
        /// Gets or sets the steuerzeit sz.
        /// </summary>
        /// <value>The steuerzeit sz.</value>
        public double? SteuerzeitSZ { get; set; }

        /// <summary>
        /// Gets or sets the breite b unit.
        /// </summary>
        /// <value>The breite b unit.</value>
        [NotMapped]
        private LengthUnit? _BreiteBUnit { get; set; }

        /// <summary>
        /// Gets or sets the durchmesser d unit.
        /// </summary>
        /// <value>The durchmesser d unit.</value>
        [NotMapped]
        private LengthUnit? _DurchmesserDUnit { get; set; }

        /// <summary>
        /// Gets or sets the flaeche a unit.
        /// </summary>
        /// <value>The flaeche a unit.</value>
        [NotMapped]
        private AreaUnit? _FlaecheAUnit { get; set; }

        /// <summary>
        /// Gets or sets the hoehe h unit.
        /// </summary>
        /// <value>The hoehe h unit.</value>
        [NotMapped]
        private LengthUnit? _HoeheHUnit { get; set; }

        /// <summary>
        /// Gets or sets the laenge l unit.
        /// </summary>
        /// <value>The laenge l unit.</value>
        [NotMapped]
        private LengthUnit? _LaengeLUnit { get; set; }
    }
}