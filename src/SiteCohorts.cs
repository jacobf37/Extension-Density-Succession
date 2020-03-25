using System;
using System.Collections.Generic;
using System.Text;
using Landis.Core;

namespace Landis.Extension.Succession.Density
{
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        private Dictionary<ISpecies, List<Cohort>> cohorts = null;

        public bool IsMaturePresent(ISpecies species)
        {

            bool speciesPresent = cohorts.ContainsKey(species);

            bool IsMaturePresent = (speciesPresent && (cohorts[species].Max(o => o.Age) >= species.Maturity)) ? true : false;

            return IsMaturePresent;
        }

    }
}
