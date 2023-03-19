// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// DynoPS-Model.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class DynoPsModel : BaseEntityModel
    {
        public DynoPsModel(double drehzahl, double ps)
        {
            this.Drehzahl = drehzahl;
            this.Ps = ps;
        }

        public DynoPsModel()
        {
        }

        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>The x.</value>
        [Required]
        public double Drehzahl { get; set; }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public virtual DynoModel Dyno { get; set; }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>The y.</value>
        [Required]
        public double Ps { get; set; }
    }
}