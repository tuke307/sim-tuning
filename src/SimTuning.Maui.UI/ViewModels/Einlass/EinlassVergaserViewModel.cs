// Copyright (c) 2025 tuke productions. All rights reserved.
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SimTuning.Core;
using SimTuning.Core.Models;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnitsNet.Units;

namespace SimTuning.Maui.UI.ViewModels
{
    public partial class EinlassVergaserViewModel : ViewModelBase
    {
        private readonly IPopupService _popupService;

        public EinlassVergaserViewModel(
            ILogger<EinlassVergaserViewModel> logger,
            IVehicleService vehicleService,
            IPopupService popupService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _popupService = popupService;

            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();

            UnitHubvolumen = VolumeQuantityUnits.Where(x => x.UnitEnumValue.Equals(VolumeUnit.CubicMillimeter)).First();
            UnitVergasergroeße = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            UnitHauptdueseD = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Micrometer)).First();
        }

        #region Commands

        public void InsertHelperVehicle(VehiclesModel helperVehicle)
        {
            if (helperVehicle.Motor.HubraumV.HasValue)
                Hubvolumen = helperVehicle.Motor.HubraumV;

            if (helperVehicle.Motor.ResonanzU.HasValue)
                Resonanzdrehzahl = helperVehicle.Motor.ResonanzU;
        }

        [RelayCommand]
        private async Task ShowHelperVehiclesAsync()
        {
            var result = await _popupService.ShowPopupAsync<VehiclesViewModel>(Shell.Current);
            if (result is VehiclesModel vehicleModel)
            {
                InsertHelperVehicle(vehicleModel);
            }
        }

        private void Refresh_Hauptduesendurchmesser()
        {
            if (Vergasergroeße.HasValue && UnitVergasergroeße != null)
            {
                HauptdueseD = EinlassLogic.GetVergaserHauptduesenDurchmesser(
                    UnitsNet.UnitConverter.Convert(
                        Vergasergroeße.Value,
                        UnitVergasergroeße.UnitEnumValue,
                        LengthUnit.Millimeter));
            }
        }

        private void Refresh_Vergasergroeße()
        {
            if (Hubvolumen.HasValue && Resonanzdrehzahl.HasValue && UnitHubvolumen != null)
            {
                Vergasergroeße = EinlassLogic.GetVergaserDurchmesser(
                    UnitsNet.UnitConverter.Convert(
                        Hubvolumen.Value,
                        UnitHubvolumen.UnitEnumValue,
                        VolumeUnit.CubicCentimeter),
                    Resonanzdrehzahl.Value);
            }
        }

        #endregion Commands

        #region Values

        private readonly ILogger<EinlassVergaserViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private double? _hauptdueseD;

        private double? _hubvolumen;

        private double? _resonanzdrehzahl;

        private UnitListItem? _unitHauptdueseD;

        private UnitListItem? _unitHubvolumen;

        private UnitListItem? _unitVergasergroeße;

        private double? _vergasergroeße;

        public double? HauptdueseD
        {
            get => _hauptdueseD;
            set { SetProperty(ref _hauptdueseD, value); }
        }

        public double? Hubvolumen
        {
            get => _hubvolumen;
            set
            {
                SetProperty(ref _hubvolumen, value);
                Refresh_Vergasergroeße();
            }
        }

        public ObservableCollection<UnitListItem> LengthQuantityUnits { get; }

        public double? Resonanzdrehzahl
        {
            get => _resonanzdrehzahl;
            set
            {
                SetProperty(ref _resonanzdrehzahl, value);
                Refresh_Vergasergroeße();
            }
        }

        public UnitListItem? UnitHauptdueseD
        {
            get => _unitHauptdueseD;
            set
            {
                HauptdueseD = Core.Helpers.Functions.UpdateValue(HauptdueseD, UnitHauptdueseD, value);

                SetProperty(ref _unitHauptdueseD, value);
            }
        }

        public UnitListItem? UnitHubvolumen
        {
            get => _unitHubvolumen;
            set
            {
                Hubvolumen = Core.Helpers.Functions.UpdateValue(Hubvolumen, UnitHubvolumen, value);

                SetProperty(ref _unitHubvolumen, value);
            }
        }

        public UnitListItem? UnitVergasergroeße
        {
            get => _unitVergasergroeße;
            set
            {
                Vergasergroeße = Core.Helpers.Functions.UpdateValue(Vergasergroeße, UnitVergasergroeße, value);

                SetProperty(ref _unitVergasergroeße, value);
            }
        }

        public double? Vergasergroeße
        {
            get => _vergasergroeße;
            set
            {
                SetProperty(ref _vergasergroeße, value);
                Refresh_Hauptduesendurchmesser();
            }
        }

        public ObservableCollection<UnitListItem> VolumeQuantityUnits { get; }

        #endregion Values
    }
}
