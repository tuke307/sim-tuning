// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Data.Models;
using System.Collections.Generic;

namespace SimTuning.Core.Services
{
    /// <summary>
    /// IVehicleService.
    /// </summary>
    public interface IVehicleService
    {
        /// <summary>
        /// Updates the one.
        /// </summary>
        /// <param name="dyno">The dyno.</param>
        DynoModel? CreateOne(DynoModel dyno);

        /// <summary>
        /// Creates the one.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns></returns>
        VehiclesModel? CreateOne(VehiclesModel vehicle);

        /// <summary>
        /// Deletes the specified ausrollen.
        /// </summary>
        /// <param name="ausrollen">The ausrollen.</param>
        void Delete(List<AusrollenModel> ausrollen);

        /// <summary>
        /// Deletes the specified geschwindigkeit.
        /// </summary>
        /// <param name="geschwindigkeit">The geschwindigkeit.</param>
        void Delete(List<GeschwindigkeitModel> geschwindigkeit);

        /// <summary>
        /// Deletes the one.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        void DeleteOne(VehiclesModel vehicle);

        /// <summary>
        /// Deletes the one.
        /// </summary>
        /// <param name="dyno">The dyno.</param>
        void DeleteOne(DynoModel dyno);

        /// <summary>
        /// Retrieves this instance.
        /// </summary>
        /// <returns>Eine Liste von <see cref="DynoModel" />.</returns>
        List<DynoModel>? RetrieveDynos(bool forceupdate = false);

        /// <summary>
        /// Retrieves the environments.
        /// </summary>
        /// <param name="forceupdate">if set to <c>true</c> [forceupdate].</param>
        /// <returns>Eine Liste von <see cref="EnvironmentModel" />.</returns>
        List<EnvironmentModel>? RetrieveEnvironments(bool forceupdate = false);

        /// <summary>
        /// Retrieves the motoren.
        /// </summary>
        /// <param name="forceupdate">if set to <c>true</c> [forceupdate].</param>
        /// <returns>Eine Liste von <see cref="MotorModel" />.</returns>
        List<MotorModel>? RetrieveMotoren(bool forceupdate = false);

        /// <summary>
        /// Retrieves the one.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Ein <see cref="VehiclesModel" />.</returns>
        VehiclesModel? RetrieveOne(int? id);

        /// <summary>
        /// Retrieves the one active.
        /// </summary>
        /// <returns>Ein <see cref="DynoModel" />.</returns>
        DynoModel? RetrieveOneActive();

        /// <summary>
        /// Retrieves the vehicles.
        /// </summary>
        /// <param name="forceupdate">if set to <c>true</c> [forceupdate].</param>
        /// <returns>Eine Liste von <see cref="VehiclesModel" />.</returns>
        List<VehiclesModel>? RetrieveVehicles(bool forceupdate = false);

        /// <summary>
        /// Updates the one.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        void UpdateOne(VehiclesModel vehicle);

        /// <summary>
        /// Updates the one.
        /// </summary>
        /// <param name="dyno">The dyno.</param>
        void UpdateOne(DynoModel dyno);
    }
}