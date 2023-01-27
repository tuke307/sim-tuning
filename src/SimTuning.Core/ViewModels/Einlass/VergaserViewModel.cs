// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SimTuning.Core.Models;
using SimTuning.Core.ModuleLogic;
using SimTuning.Core.Services;
using SimTuning.Data;
using SimTuning.Data.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnitsNet.Units;

namespace SimTuning.Core.ViewModels.Einlass
{
    public class VergaserViewModel : ViewModelBase
    {
        public VergaserViewModel(
            ILogger<VergaserViewModel> logger,
            INavigationService navigationService,
            IVehicleService vehicleService)
        {
            this._logger = logger;
            this._vehicleService = vehicleService;

            VolumeQuantityUnits = new VolumeQuantity();
            LengthQuantityUnits = new LengthQuantity();

            UnitHubvolumen = VolumeQuantityUnits.Where(x => x.UnitEnumValue.Equals(VolumeUnit.CubicMillimeter)).First();
            UnitVergasergroeße = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Millimeter)).First();
            UnitHauptdueseD = LengthQuantityUnits.Where(x => x.UnitEnumValue.Equals(LengthUnit.Micrometer)).First();

            HelperVehicles = new ObservableCollection<VehiclesModel>(_vehicleService.RetrieveVehicles());

            InsertDataCommand = new RelayCommand(InsertData);
        }

        #region Commands



        public void InsertData()
        {
            if (HelperVehicle.Motor.HubraumV.HasValue)
                Hubvolumen = HelperVehicle.Motor.HubraumV;

            if (HelperVehicle.Motor.ResonanzU.HasValue)
                Resonanzdrehzahl = HelperVehicle.Motor.ResonanzU;
        }


        private void Refresh_Hauptduesendurchmesser()
        {
            if (Vergasergroeße.HasValue)
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
            if (Hubvolumen.HasValue && Resonanzdrehzahl.HasValue)
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

        private readonly ILogger<VergaserViewModel> _logger;
        private readonly IVehicleService _vehicleService;
        private double? _hauptdueseD;

        private VehiclesModel _helperVehicle;

        private ObservableCollection<VehiclesModel> _helperVehicles;

        private double? _hubvolumen;

        private double? _resonanzdrehzahl;

        private UnitListItem _unitHauptdueseD;

        private UnitListItem _unitHubvolumen;

        private UnitListItem _unitVergasergroeße;

        private double? _vergasergroeße;

        public double? HauptdueseD
        {
            get => _hauptdueseD;
            set { SetProperty(ref _hauptdueseD, value); }
        }

        public VehiclesModel HelperVehicle
        {
            get => _helperVehicle;
            set { SetProperty(ref _helperVehicle, value); }
        }

        public ObservableCollection<VehiclesModel> HelperVehicles
        {
            get => _helperVehicles;
            set { SetProperty(ref _helperVehicles, value); }
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

        public IRelayCommand InsertDataCommand { get; set; }

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

        public UnitListItem UnitHauptdueseD
        {
            get => _unitHauptdueseD;
            set
            {
                HauptdueseD = Helpers.Functions.UpdateValue(HauptdueseD, UnitHauptdueseD, value);

                SetProperty(ref _unitHauptdueseD, value);
            }
        }

        public UnitListItem UnitHubvolumen
        {
            get => _unitHubvolumen;
            set
            {
                Hubvolumen = Helpers.Functions.UpdateValue(Hubvolumen, UnitHubvolumen, value);

                SetProperty(ref _unitHubvolumen, value);
            }
        }

        public UnitListItem UnitVergasergroeße
        {
            get => _unitVergasergroeße;
            set
            {
                Vergasergroeße = Helpers.Functions.UpdateValue(Vergasergroeße, UnitVergasergroeße, value);

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