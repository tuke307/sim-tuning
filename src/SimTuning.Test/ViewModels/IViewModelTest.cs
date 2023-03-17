// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Test
{
    public interface IViewModelTest
    {
        #region Auslass

        void AuslassAnwendungViewModelTest();

        void AuslassMainViewModelTest();

        void AuslassTheorieViewModelTest();

        #endregion Auslass

        #region Dyno

        void DynoAusrollenViewModelTest();

        void DynoBeschleunigungViewModelTest();

        void DynoDataViewModelTest();

        void DynoDiagnosisViewModelTest();

        void DynoMainViewModelTest();

        void DynoRuntimeViewModelTest();

        void DynoSpectrogramViewModelTest();

        #endregion Dyno

        #region Einlass

        void EinlassKanalViewModelTest();

        void EinlassMainViewModelTest();

        void EinlassVergaserViewModelTest();

        #endregion Einlass

        #region Motor

        void MotorHubraumViewModelTest();

        void MotorMainViewModelTest();

        void MotorSteuerdiagrammViewModelTest();

        void MotorUmrechnungViewModelTest();

        void MotorVerdichtungViewModelTest();

        #endregion Motor

        #region Einstellungen

        void EinstellungenApplicationViewModelTest();

        void EinstellungenMenuViewModelTest();

        void EinstellungenVehiclesViewModelTest();

        #endregion Einstellungen

        void HomeViewModelViewModelTest();

        void MainPageViewModelTest();
    }
}