// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Core;
using SimTuning.Core.Models;

namespace SimTuning.Maui.UI.ViewModels
{
    /// <content>
    /// Spiegel-Eigenschaften für die Einlass- und Vergaser-Felder
    /// (Vehicle.Motor.Einlass.* bzw. Vehicle.Motor.Einlass.Vergaser.*).
    /// </content>
    public partial class VehiclesViewModelBase
    {
        /// <summary>
        /// Gets or sets the vehicle motor einlass breite b.
        /// </summary>
        /// <value>The vehicle motor einlass breite b.</value>
        public double? VehicleMotorEinlassBreiteB
        {
            get => Vehicle?.Motor?.Einlass?.BreiteB;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.BreiteB = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass breite b unit.
        /// </summary>
        /// <value>The vehicle motor einlass breite b unit.</value>
        public UnitListItem? VehicleMotorEinlassBreiteBUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.BreiteBUnit));
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.BreiteBUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorEinlassBreiteB));
        }

        public double? VehicleMotorEinlassDurchmesserD
        {
            get => Vehicle?.Motor?.Einlass?.DurchmesserD;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.DurchmesserD = value);
        }

        public UnitListItem? VehicleMotorEinlassDurchmesserDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.DurchmesserDUnit));
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.DurchmesserDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorEinlassDurchmesserD));
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass flaeche a.
        /// </summary>
        /// <value>The vehicle motor einlass flaeche a.</value>
        public double? VehicleMotorEinlassFlaecheA
        {
            get => Vehicle?.Motor?.Einlass?.FlaecheA;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.FlaecheA = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor einlass flaeche a unit.</value>
        public UnitListItem? VehicleMotorEinlassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.FlaecheAUnit));
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.FlaecheAUnit = (UnitsNet.Units.AreaUnit?)value?.UnitEnumValue, nameof(VehicleMotorEinlassFlaecheA));
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass hoehe h.
        /// </summary>
        /// <value>The vehicle motor einlass hoehe h.</value>
        public double? VehicleMotorEinlassHoeheH
        {
            get => Vehicle?.Motor?.Einlass?.HoeheH;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.HoeheH = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor einlass hoehe h unit.</value>
        public UnitListItem? VehicleMotorEinlassHoeheHUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.HoeheHUnit));
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.HoeheHUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorEinlassHoeheH));
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass laenge l.
        /// </summary>
        /// <value>The vehicle motor einlass laenge l.</value>
        public double? VehicleMotorEinlassLaengeL
        {
            get => Vehicle?.Motor?.Einlass?.LaengeL;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.LaengeL = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor einlass laenge l unit.</value>
        public UnitListItem? VehicleMotorEinlassLaengeLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.LaengeLUnit));
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.LaengeLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorEinlassLaengeL));
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass luft bedarf.
        /// </summary>
        /// <value>The vehicle motor einlass luft bedarf.</value>
        public double? VehicleMotorEinlassLuftBedarf
        {
            get => Vehicle?.Motor?.Einlass?.LuftBedarf;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.LuftBedarf = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor einlass steuerzeit sz.</value>
        public double? VehicleMotorEinlassSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Einlass?.SteuerzeitSZ;
            set => SetMirror(Vehicle?.Motor?.Einlass, e => e.SteuerzeitSZ = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser benzin luft f.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser benzin luft f.</value>
        public double? VehicleMotorEinlassVergaserBenzinLuftF
        {
            get => Vehicle?.Motor?.Einlass?.Vergaser?.BenzinLuftF;
            set => SetMirror(Vehicle?.Motor?.Einlass?.Vergaser, v => v.BenzinLuftF = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser durchmesser d.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser durchmesser d.</value>
        public double? VehicleMotorEinlassVergaserDurchmesserD
        {
            get => Vehicle?.Motor?.Einlass?.Vergaser?.DurchmesserD;
            set => SetMirror(Vehicle?.Motor?.Einlass?.Vergaser, v => v.DurchmesserD = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor einlass vergaser durchmesser d unit.
        /// </summary>
        /// <value>The vehicle motor einlass vergaser durchmesser d unit.</value>
        public UnitListItem? VehicleMotorEinlassVergaserDurchmesserDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Einlass?.Vergaser?.DurchmesserDUnit));
            set => SetMirror(Vehicle?.Motor?.Einlass?.Vergaser, v => v.DurchmesserDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorEinlassVergaserDurchmesserD));
        }
    }
}
