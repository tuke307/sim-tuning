// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Core;
using SimTuning.Core.Models;

namespace SimTuning.Maui.UI.ViewModels
{
    /// <content>
    /// Spiegel-Eigenschaften für die Überströmer-Felder (Vehicle.Motor.Ueberstroemer.*).
    /// </content>
    public partial class VehiclesViewModelBase
    {
        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer anzahl.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer anzahl.</value>
        public double? VehicleMotorUeberstroemerAnzahl
        {
            get => Vehicle?.Motor?.Ueberstroemer?.Anzahl;
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.Anzahl = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer breite b.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer breite b.</value>
        public double? VehicleMotorUeberstroemerBreiteB
        {
            get => Vehicle?.Motor?.Ueberstroemer?.BreiteB;
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.BreiteB = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer breite b unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer breite b unit.</value>
        public UnitListItem? VehicleMotorUeberstroemerBreiteBUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Ueberstroemer?.BreiteBUnit));
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.BreiteBUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorUeberstroemerBreiteB));
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer flaeche a.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer flaeche a.</value>
        public double? VehicleMotorUeberstroemerFlaecheA
        {
            get => Vehicle?.Motor?.Ueberstroemer?.FlaecheA;
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.FlaecheA = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer flaeche a unit.</value>
        public UnitListItem? VehicleMotorUeberstroemerFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Ueberstroemer?.FlaecheAUnit));
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.FlaecheAUnit = (UnitsNet.Units.AreaUnit?)value?.UnitEnumValue, nameof(VehicleMotorUeberstroemerFlaecheA));
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h.</value>
        public double? VehicleMotorUeberstroemerHoeheH
        {
            get => Vehicle?.Motor?.Ueberstroemer?.HoeheH;
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.HoeheH = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer hoehe h unit.</value>
        public UnitListItem? VehicleMotorUeberstroemerHoeheHUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Ueberstroemer?.HoeheHUnit));
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.HoeheHUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorUeberstroemerHoeheH));
        }

        /// <summary>
        /// Gets or sets the vehicle motor ueberstroemer steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor ueberstroemer steuerzeit sz.</value>
        public double? VehicleMotorUeberstroemerSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Ueberstroemer?.SteuerzeitSZ;
            set => SetMirror(Vehicle?.Motor?.Ueberstroemer, u => u.SteuerzeitSZ = value);
        }
    }
}
