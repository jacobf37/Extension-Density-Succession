using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Landis.SpatialModeling;
using Landis.Core;

namespace Landis.Extension.Succession.Landispro
{
    class SiteVars
    {
        private static ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> cohorts;

        public static void Initialize()
        {
            cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>();
            //PlugIn.ModelCore.RegisterSiteVar(cohorts, "Succession.Cohorts");
            PlugIn.ModelCore.RegisterSiteVar(cohorts, "Succession.AgeCohorts");
        }

        public static ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
            set
            {
                cohorts = value;
            }
        }

        public bool Establish(ISpecies species, ActiveSite site)
        {
            return PlugIn.ModelCore.GenerateUniform() < Establishment_probability_Attributes.get_probability(species.Name, PlugIn.ModelCore.Ecoregion[site].Name, PlugIn.ModelCore.TimeSinceStart);
        }

        public void AddNewCohort(ISpecies species, ActiveSite site)
        {
            ((ISiteVar<species>)cohorts)[site][species.Index].AddNewCohort();
        }

        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            return ((ISiteVar<species>)cohorts)[site][species.Index].IsMaturePresent;
        }

        public bool PlantingEstablish(ISpecies species, ActiveSite site)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            double establishProbability = Establishment_probability_Attributes.get_probability(species.Name,ecoregion.Name, PlugIn.ModelCore.TimeSinceStart);
            return establishProbability > 0.0;
        }
    }
}
