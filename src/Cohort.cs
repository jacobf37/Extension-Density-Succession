using System;
using System.Collections.Generic;
using System.Text;
using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.Density
{
    class Cohort : Landis.Library.AgeOnlyCohorts.ICohort, Landis.Library.BiomassCohorts.ICohort
    {
        private ushort age;
        private ISpecies species;
        private float biomass; // live aboveground


        // Age (years)
        public ushort Age
        {
            get
            {
                return age;
            }
        }
        // LANDIS species
        public Landis.Core.ISpecies Species
        {
            get
            {
                return species;
            }
        }
        // Aboveground Biomass (g/m2)
        public int Biomass
        {
            get
            {
                return (int)biomass;
            }
        }
        //Non-woody biomass
        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            //FIXME
            return (int)(biomass * 0.3);
        }

    }
}
