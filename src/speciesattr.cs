using System;
using System.Collections.Generic;
using Landis.Utilities;
using Landis.Core;


namespace Landis.Extension.Succession.Density
{
    /// <summary>
    /// the following parameters are required in landis pro succession
    /// </summary>
    public class extra_species_attr
    {
        public string SpeciesName
        {
            get;
            set;
        }

        public float ReclassCoef    //Reclassification coefficient
        {
            get;
            set;
        }

        public int SpType       //SPT1 
        {
            get;
            set;
        }

        public int BioMassCoef  //SPT2    
        {
            get;
            set;
        }
        public int MaxDQ        //MaxDBH
        {
            get;
            set;
        }

        public int MaxSDI
        {
            get;
            set;
        }

        public int TotalSeed
        {
            get;
            set;
        }

        public float CarbonCoef
        {
            get;
            set;
        }
    }





    //public class extra_species_attrParser : Landis.TextParser<extra_species_attr>
    public class extra_species_attrParser : Landis.TextParser<List<extra_species_attr>>
    {
        public override string LandisDataValue
        {
            get { return "ExtraSpeciesParameters"; }
        }

        protected override List<extra_species_attr> Parse()
        {
            ReadLandisDataVar();
            
            List<extra_species_attr> extra_speattr = new List<extra_species_attr>();
            
            //Read in extra species attributes:
            InputVar<string> name        = new InputVar<string>("species name");
            InputVar<float> reclass_coef = new InputVar<float>("reclassification coef");
            InputVar<int> spType         = new InputVar<int>("Species Type: SPT1");
            InputVar<int> bioMassCoef    = new InputVar<int>("Species biomass Type: SPT2");
            InputVar<int> maxDQ          = new InputVar<int>("MAXDQ: Maximum diameter of species");
            InputVar<int> maxSDI         = new InputVar<int>("MAXSDI: Maximum stand density");
            InputVar<int> totalSeed      = new InputVar<int>("Number of seeds produced by each individual per year.");
            InputVar<float> carbonCoef   = new InputVar<float>("Carbon coefficient");

            //=====================================================================

            while (!AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                extra_species_attr local_extra_attr = new extra_species_attr();

                ReadValue(name, currentLine);
                local_extra_attr.SpeciesName = name.Value.Actual;

                ReadValue(reclass_coef, currentLine);
                local_extra_attr.ReclassCoef = reclass_coef.Value.Actual;

                ReadValue(spType, currentLine);
                local_extra_attr.SpType = spType.Value.Actual;

                ReadValue(bioMassCoef, currentLine);
                local_extra_attr.BioMassCoef = bioMassCoef.Value.Actual;

                ReadValue(maxDQ, currentLine);
                local_extra_attr.MaxDQ = maxDQ.Value.Actual;

                ReadValue(maxSDI, currentLine);
                local_extra_attr.MaxSDI = maxSDI.Value.Actual;

                ReadValue(totalSeed, currentLine);
                local_extra_attr.TotalSeed = totalSeed.Value.Actual;

                ReadValue(carbonCoef, currentLine);
                local_extra_attr.CarbonCoef = carbonCoef.Value.Actual;

                CheckNoDataAfter("the " + carbonCoef.Name + " column", currentLine);

                extra_speattr.Add(local_extra_attr);

                GetNextLine();                
            }

            return extra_speattr;
        }
    }





    /// <summary>
    /// all species attributes
    /// </summary>
    public class speciesattr : extra_species_attr
    {
        private ISpecies spe;
        //private string name;           //Species Name.
        
        //private int longevity;      //Maximum age.
        //private int maturity;       //Sexual Maturity.
        //private int shadeTolerance; //Shade Tolerance.
        //private int fireTolerance;  //Fire Tolerance.
        //private int effectiveD;     //Effective seeding distance.
        //private int maxD;           //Maximum seeding distance. 
        
        //private float vegProb;        //Probability of vegetative seeding.

        //private int MinSproutAge;   //Minimum sprouting age
        //private int MaxSproutAge;   //Maximum sprouting age

        //================================================================
        private float maxAreaOfSTDTree;
        //================================================================
       

        public string Name 
        { 
            get { return spe.Name; } 
        }

        public int Longevity 
        { 
            get { return spe.Longevity; } 
        }

        public int Maturity  
        { 
            get { return spe.Maturity;  } 
        }

        public int Shade_Tolerance 
        { 
            get { return spe.ShadeTolerance; } 
        }

        public int Max_seeding_Dis 
        { 
            get { return spe.MaxSeedDist; } 
        }

        public float MaxAreaOfSTDTree
        {
            get { return maxAreaOfSTDTree; }
            set { maxAreaOfSTDTree = value; }
        }

        //Add by YYF 2018/11
        public int MaxSproutAge
        {
            get { return spe.MaxSproutAge; }
        }

        public int MinSproutAge
        {
            get { return spe.MinSproutAge; }
        }

        //Add by YYF 2019/4
        public int FireTolerance
        {
            get { return spe.FireTolerance; }
        }

        public float VegReprodProb
        {
            get { return spe.VegReprodProb; }
        }
        //========

        //Constructor
        public speciesattr()        
        {
            spe = null;
            ReclassCoef    = 0.0f;
        }



        //get species attributes
        public void read(extra_species_attr extra_attr, Core.ISpecies spe)
        {
            this.spe = spe;

            if (extra_attr.SpeciesName != Name)
                throw new System.Exception("name is not consistent. This is speciesattr.cs");

            ReclassCoef = extra_attr.ReclassCoef;
            SpType      = extra_attr.SpType;
            BioMassCoef = extra_attr.BioMassCoef;
            MaxDQ       = extra_attr.MaxDQ;
            MaxSDI      = extra_attr.MaxSDI;
            TotalSeed   = extra_attr.TotalSeed;
            CarbonCoef  = extra_attr.CarbonCoef;

            maxAreaOfSTDTree = 10000.0f / MaxSDI;
        }




        public void write(System.IO.StreamWriter outfile)    //Write species attributes to a file.
        {
            outfile.Write("{0}\t", Name);
            outfile.Write("{0}\t", Longevity);
            outfile.Write("{0}\t", Maturity);
            outfile.Write("{0}\t", Shade_Tolerance);
            outfile.Write("{0}\t", spe.FireTolerance);
            outfile.Write("{0}\t", spe.EffectiveSeedDist);
            outfile.Write("{0}\t", spe.MaxSeedDist);
            outfile.Write("{0}\t", spe.VegReprodProb);
            outfile.Write("{0}\t", spe.MaxSproutAge);
            outfile.Write("{0}\n", ReclassCoef);
        }




        //Dump species attributes to the CRT.
        public void dump()
        {
            Console.WriteLine("name:           {0}", Name);
            Console.WriteLine("longevity:      {0}", Longevity);
            Console.WriteLine("maturity:       {0}", Maturity);
            Console.WriteLine("shadeTolerance: {0}", Shade_Tolerance);
            Console.WriteLine("fireTolerance:  {0}", spe.FireTolerance);
            Console.WriteLine("effectiveD:     {0}", spe.EffectiveSeedDist);
            Console.WriteLine("maxD:           {0}", spe.MaxSeedDist);
            Console.WriteLine("vegProb:        {0}", spe.VegReprodProb);
            Console.WriteLine("maxSproutAge:   {0}", spe.MaxSproutAge);
            Console.WriteLine("reclassCoef:    {0}", ReclassCoef);
        }




        //Given a distance this will return a seeding probability for a species.
        public float prob(float distance)
        {
            float a = .95f;
            float b = 0.001f;

            double md = spe.MaxSeedDist;
            double ed = spe.EffectiveSeedDist;


            if (distance <= ed)
                return a;

            if (distance > md)
                return b;

            double alpha = Math.Log(a / b) / (md - ed);
            double prob = a * Math.Exp(-1 * alpha * (distance - ed));

            return (float)prob;
        }

    }
}
