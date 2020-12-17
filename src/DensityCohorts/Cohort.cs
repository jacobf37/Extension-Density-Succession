//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Utilities;
using System;
using System.Collections.Generic;
using Landis.Extension.Succession.BiomassPnET;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// A species cohort with density information.
    /// </summary>
    public class Cohort
        : Landis.Library.AgeOnlyCohorts.ICohort,Landis.Library.BiomassCohorts.ICohort, Landis.Library.DensityCohorts.ICohort
    {

        private ISpecies species;
        private CohortData data;
        private ushort age;
        private int treenumber;
        private bool firstYear;
        private float diameter;

        public static IEcoregionPnET ecoregion;
        public static ActiveSite site;

        //---------------------------------------------------------------------

        public ISpecies Species
        {
            get {
                return species;
            }
        }

        //---------------------------------------------------------------------

        public ushort Age
        {
            get {
                return data.Age;
            }
        }

        //---------------------------------------------------------------------

        public int Biomass
        {
            get
            {
                ISpeciesDensity speciesdensity = PlugIn.SpeciesDensity.AllSpecies[species.Index];
                double biomass  = Math.Exp(PlugIn.biomass_util.GetBiomassData(speciesdensity.BiomassClass, 1) + PlugIn.biomass_util.GetBiomassData(speciesdensity.BiomassClass, 2) * Math.Log(Diameter)) * data.Treenumber / 1000.00; // Mg/cell
                int biomass_int = System.Convert.ToInt32(biomass);
                double biomass_gm2 = biomass * 1000 * 1000 / (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength);
                int biomass_gm2_int = System.Convert.ToInt32(biomass_gm2);
                return biomass_gm2_int;
            }
        }

        //---------------------------------------------------------------------

        public float Diameter
        {
            get
            {
                //FIXME - this seems to be working
                float diameter = 0;
                
                Dictionary<int, double> diameters = DiameterInputs.AllData[ecoregion.Name][species.Name].Diameters;
                if (diameters.ContainsKey(Age))
                {
                    diameter = (float)diameters[Age];
                }
                else
                {
                    for (int i = Age; i > 0; i--)
                    {
                        if (diameters.ContainsKey(i))
                        {
                            diameter = (float)diameters[i];
                        }
                    }
                }
                return diameter;
            }
        }

        //---------------------------------------------------------------------

        //---------------------------------------------------------------------

        public float cohortDiameter(ActiveSite site)
        {
        
                //FIXME - this seems to be working
            float diameter = 0;
            
            Dictionary<int, double> diameters = DiameterInputs.AllData[ecoregion.Name][species.Name].Diameters;
            if (diameters.ContainsKey(Age))
            {
                diameter = (float)diameters[Age];
            }
            else
            {
                for (int i = Age; i > 0; i--)
                {
                    if (diameters.ContainsKey(i))
                    {
                        diameter = (float)diameters[i];
                    }
                }
            }
            return diameter;
            
        }

        //---------------------------------------------------------------------

        public void Accumulate(Cohort c)
        {
            data.Treenumber += c.data.Treenumber;
        }

        //---------------------------------------------------------------------

        public int Treenumber
        {
            get
            {
                return data.Treenumber;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The cohort's age and density data.
        /// </summary>
        public CohortData Data
        {
            get {
                return data;
            }
        }


        //---------------------------------------------------------------------

        public Cohort(ISpeciesDensity species, ushort year_of_birth, string SiteName) // : base(species, 0, (int)(1F / species.DNSC * (ushort)species.InitialNSC))
        {
            this.species = species;
            age = 1;

            //FIXME - initialize the number of trees in a new cohort
            this.treenumber = 100;

            firstYear = true;
        }

        //---------------------------------------------------------------------

        public static void SetSiteAccessFunctions(SiteCohorts sitecohorts)
        {
            Cohort.ecoregion = sitecohorts.Ecoregion;
            Cohort.site = sitecohorts.Site;
        }

        //---------------------------------------------------------------------

        public static void RaiseDeathEvent(object sender,
                    Cohort cohort,
                    ActiveSite site,
                    ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
            {
                DeathEvent(sender, new Landis.Library.DensityCohorts.DeathEventArgs(cohort, site, disturbanceType));
            }

        }

        //---------------------------------------------------------------------

        public Cohort(ISpecies species,
                      ushort   age,
                      int   treenumber,
                      IEcoregionPnET ecoregion)
        {
            this.species = species;
            this.data.Age = age;
            this.data.Treenumber = treenumber;
            this.data.Diameter = 0;
            this.data.Biomass = 0;

            if (ecoregion.Active)
            {
                float diameter = 0;
                Dictionary<int, double> diameters = DiameterInputs.AllData[ecoregion.Name][species.Name].Diameters;
                if (diameters.ContainsKey(Age))
                {
                    diameter = (float)diameters[Age];
                }
                else
                {
                    for (int i = Age; i > 0; i--)
                    {
                        if (diameters.ContainsKey(i))
                        {
                            diameter = (float)diameters[i];
                        }
                    }
                }
                this.data.Diameter = diameter;
                ISpeciesDensity speciesdensity = PlugIn.SpeciesDensity.AllSpecies[species.Index];
                double biomass = Math.Exp(PlugIn.biomass_util.GetBiomassData(speciesdensity.BiomassClass, 1) + PlugIn.biomass_util.GetBiomassData(speciesdensity.BiomassClass, 2) * Math.Log(diameter)) * data.Treenumber / 1000.00; // Mg/cell
                int biomass_int = System.Convert.ToInt32(biomass);
                double biomass_gm2 = biomass * 1000 * 1000 / (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength);
                int biomass_gm2_int = System.Convert.ToInt32(biomass_gm2);
                this.data.Biomass = biomass_gm2_int;
            }
        }

        //---------------------------------------------------------------------

        public Cohort(ISpecies   species,
                      CohortData cohortData)
        {
            this.species = species;
            this.data = cohortData;
        }

        //---------------------------------------------------------------------

        public Cohort(Cohort cohort) // : base(cohort.species, new Landis.Library.BiomassCohorts.CohortData(cohort.age, cohort.Biomass))
        {
            this.species = cohort.species;
            this.data.Age = cohort.age;
            this.data.Treenumber = cohort.treenumber;
            this.diameter = cohort.diameter;
            this.data.Biomass = cohort.Biomass;
        }

        //---------------------------------------------------------------------

        public Cohort(ISpeciesDensity species, ushort age, int treenumber, string SiteName, ushort firstYear, IEcoregionPnET siteEcoregion)
        {
            //InitializeSubLayers();
            this.species = species;
            ecoregion = siteEcoregion;
            this.data.Age = age;
            this.data.Treenumber = treenumber;
            this.data.Biomass = this.Biomass;
            this.data.Diameter = this.Diameter;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Increments the cohort's age by one year.
        /// </summary>
        public void IncrementAge()
        {
            data.Age += 1;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Changes the cohort's tree number.
        /// </summary>
        public void ChangeTreenumber(int delta)
        {
            int newTreenumber = data.Treenumber + delta;
            data.Treenumber = System.Math.Max(0, newTreenumber);
            data.Biomass = this.Biomass;
        }

        //---------------------------------------------------------------------

        public int ComputeNonWoodyBiomass(ActiveSite site)
        {
            //Percentage nonWoodyPercentage = Cohorts.BiomassCalculator.ComputeNonWoodyPercentage(this, site);
            //return (int) (data.Biomass * nonWoodyPercentage);
            return 0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Occurs when a cohort dies either due to senescence or biomass
        /// disturbances.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> DeathEvent;
        //---------------------------------------------------------------------
        public static event PartialDeathEventHandler<PartialDeathEventArgs> PartialDeathEvent;

        /// <summary>
        /// Raises a Cohort.DeathEvent if partial mortality.
        /// </summary>
        public static void PartialMortality(object sender,
                                ICohort cohort,
                                ActiveSite site,
                                ExtensionType disturbanceType,
                                float reduction)
        {
            if (PartialDeathEvent != null)
                PartialDeathEvent(sender, new PartialDeathEventArgs(cohort, site, disturbanceType, reduction));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Raises a Cohort.DeathEvent.
        /// </summary>
        public static void Died(object     sender,
                                ICohort    cohort,
                                ActiveSite site,
                                ExtensionType disturbanceType)
        {
            if (DeathEvent != null)
                DeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Occurs when a cohort is killed by an age-only disturbance.
        /// </summary>
        public static event DeathEventHandler<DeathEventArgs> AgeOnlyDeathEvent;

        //---------------------------------------------------------------------

        /// <summary>
        /// Raises a Cohort.AgeOnlyDeathEvent.
        /// </summary>
        public static void KilledByAgeOnlyDisturbance(object     sender,
                                                      ICohort    cohort,
                                                      ActiveSite site,
                                                      ExtensionType disturbanceType)
        {
            if (AgeOnlyDeathEvent != null)
                AgeOnlyDeathEvent(sender, new DeathEventArgs(cohort, site, disturbanceType));
        }
    }
}
