using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Landispro
{
    // NO_DISPERSAL      //No seed dispersal.
    // UNIFORM           //Uniform seed dispersal.
    // NEIGHBORS         //Seed to immediate neighbors.
    // DISPERSAL         //Seed within effective distance.
    // RAND_ASYM         //Seed using interpolated chaotic distances.
    // MAX_DIST          //RAND_ASYM up to maximum distances.
    // SIM_RAND_ASYM     //RAND_ASYM up to maximum distances, Simulated random asymptopic after maximum distance.
    public enum dispersal_type { NO_DISPERSAL, UNIFORM, NEIGHBORS, DISPERSAL, RAND_ASYM, MAX_DIST, SIM_RAND_ASYM };


    public class InputParameters
    {
        public int SuccessionTimestep { get; set; }
        public int Num_Iteration      { get; set; }
        public int CellSize           { get; set; }//Length of side of cell in meters
        public int FlagforSECFile     { get; set; }//0 is no change, 3 read from file         

        public string VarianceSECFile { get; set; }//related to dynamic input file; Customized By user year variances
        public string ExtraSpecAtrFile{ get; set; } //some extra Species Attributes input file
        public string SiteImgFile     { get; set; }
        public string ReclassInFile   { get; set; }//Site input file name.
        public string OutputOption70  { get; set; }//landis 7.0 output option
        public string OutputDir       { get; set; }
        public string Freq_out_put    { get; set; }//Output map frequency file
        public string Biomassfile     { get; set; }
        public string GrowthFlagFile  { get; set; }
        public string MortalityFile   { get; set; }
        public string VolumeFile      { get; set; }

        public string LandUnitFile	  { get; set; }//Landtype attribute file name.
        public string LandImgMapFile  { get; set; }//Landtype map file name
        
        public int GrowthFlag   { get; set; }
        public int MortalityFlag{ get; set; }
        public int RandSeed     { get; set; }//Random number seed.
        public int SeedRainFlag { get; set; }
        public int VolumeFlag   { get; set; }
        public int DispRegime   { get; set; }//Seeding regime: NO_DISPERSAL, UNIFORM, NEIGHBORS, DISPERSAL, or INTERPOLATE.

        public InputParameters()
        {
            Num_Iteration = 0;            
            RandSeed = 0;
        }





        //This shall dump the contents of parameters.
        public void dump()
        {
            Console.Write("specAttrFile:       {0}\n", ExtraSpecAtrFile);

            Console.Write("landUnitFIle:       {0}\n", LandUnitFile);

            //Console.Write("landUnitMapFIle:    {0}\n", landUnitMapFile);
            Console.Write("landImgFile:        {0}\n", LandImgMapFile);
            //Console.Write("siteInFile:         {0}\n", siteInFile);
            Console.Write("siteImgFile:        {0}\n", SiteImgFile);
            Console.Write("reclassInFile:      {0}\n", ReclassInFile);

            Console.Write("OutputOption70:     {0}\n", OutputOption70);
            Console.Write("outputDir:          {0}\n", OutputDir);
            Console.Write("freq_out_put:	   {0}\n", Freq_out_put);
            Console.Write("Biomassfile:        {0}\n", Biomassfile);
            Console.Write("numberOfIterations: {0}\n", Num_Iteration);
            Console.Write("randSeed:           {0}\n", RandSeed);
            Console.Write("cellSize:           {0}\n", CellSize);

            //Console.Write("disturbance:        {0}\n", disturbance);

            //Console.Write("default_plt:        {0}\n", default_plt);


            switch (DispRegime)
            {
                case (int)dispersal_type.NO_DISPERSAL:  Console.Write("dispRegime:         NO_DISPERSAL\n"); break;

                case (int)dispersal_type.UNIFORM:       Console.Write("dispRegime:         UNIFORM\n"); break;

                case (int)dispersal_type.NEIGHBORS:     Console.Write("dispRegime:         NEIGHBORS\n"); break;

                case (int)dispersal_type.DISPERSAL:     Console.Write("dispRegime:         DISPERSAL\n"); break;

                case (int)dispersal_type.RAND_ASYM:     Console.Write("dispRegime:         RAND_ASYM\n"); break;

                case (int)dispersal_type.MAX_DIST:      Console.Write("dispRegime:         MAX_DIST\n"); break;

                case (int)dispersal_type.SIM_RAND_ASYM: Console.Write("dispRegime:         SIM_RAND_ASYM\n"); break;

                default: throw new Exception("PARAMETERS::dump()-> Illegal dispRegime");
            }

            Console.ReadLine();

        }

    }






    class InputParametersParser : Landis.TextParser<InputParameters>
    {
        public override string LandisDataValue
        {
            get { return "Density-Size-Succession"; }
        }



        protected override InputParameters Parse()
        {
            ReadLandisDataVar();

            InputParameters parameters = new InputParameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.SuccessionTimestep = timestep.Value.Actual;

            InputVar<string> seedAlg = new InputVar<string>("SeedingAlgorithm");
            ReadVar(seedAlg);
            parameters.DispRegime = (int)(dispersal_type)Enum.Parse(typeof(dispersal_type), seedAlg.Value.Actual);

            InputVar<string> initCommunities = new InputVar<string>("InitialCommunitiesWithAge");
            ReadVar(initCommunities);
            parameters.ReclassInFile = initCommunities.Value.Actual;

            InputVar<string> communitiesMap = new InputVar<string>("InitialCommunitiesMap");
            ReadVar(communitiesMap);
            parameters.SiteImgFile = communitiesMap.Value.Actual;


            InputVar<string> landUnitFile = new InputVar<string>("LandtypeAttributesFile");
            ReadVar(landUnitFile);
            parameters.LandUnitFile = landUnitFile.Value.Actual;


            InputVar<int> env_change = new InputVar<int>("Environment_change");
            ReadVar(env_change);
            if(0 == env_change.Value.Actual)
                parameters.FlagforSECFile = 0;
            else
                parameters.FlagforSECFile = 3;


            //according to dr. wang wenjuan, the dynamic input file either exists or is "0"
            //hence, the flag is either 0 or 3 according to current c++ landispro succession version
            //if (varianceSECFile.Value.Actual == "N/A" || varianceSECFile.Value.Actual == "0")
            //    parameters.FlagforSECFile = 0;
            //else
            //    parameters.FlagforSECFile = 3;


            InputVar<string> varianceSECFile = new InputVar<string>("DynamicInputFile");
            ReadVar(varianceSECFile);
            parameters.VarianceSECFile = varianceSECFile.Value.Actual;

            
            //InputVar<string> extraDynFile = new InputVar<string>("ExtraDynamicFile");
            //ReadVar(extraDynFile);
            //parameters.ExtraDynFile = extraDynFile.Value;


            InputVar<string> extraSpeciesFile = new InputVar<string>("ExtraSpeciesAttributeFile");
            ReadVar(extraSpeciesFile);
            parameters.ExtraSpecAtrFile = extraSpeciesFile.Value.Actual;

            InputVar<string> growthFlagFile = new InputVar<string>("SpeciesGrowthRatesbyLandtypeFile");
            ReadVar(growthFlagFile);
            parameters.GrowthFlagFile = growthFlagFile.Value.Actual;

            //if (growthFlagFile.Value == "N/A" || growthFlagFile.Value == "0")
            //    parameters.GrowthFlag = 0;
            //else
            parameters.GrowthFlag = 1;//accoring to dr. wang wenjuan, there must be a file for growth rate



            InputVar<string> biomassFile = new InputVar<string>("BiomassVariableFile");
            ReadVar(biomassFile);
            parameters.Biomassfile = biomassFile.Value.Actual;

            InputVar<int> randSeed = new InputVar<int>("RandomSeedForLandisPro");
            ReadVar(randSeed);
            parameters.RandSeed = randSeed.Value.Actual;




            InputVar<string> mortalityFile = new InputVar<string>("MortalityRate");
            ReadVar(mortalityFile);
            parameters.MortalityFile = mortalityFile.Value.Actual;

            if (mortalityFile.Value.Actual == "N/A" || mortalityFile.Value.Actual == "0")
                parameters.MortalityFlag = 0;
            else
                parameters.MortalityFlag = 1;


            InputVar<string> volumeFile = new InputVar<string>("SpeciesHeight");
            ReadVar(volumeFile);
            parameters.VolumeFile = volumeFile.Value.Actual;

            if (volumeFile.Value.Actual == "N/A" || volumeFile.Value.Actual == "0")
                parameters.VolumeFlag = 0;
            else
                parameters.VolumeFlag = 1;



            InputVar<string> outputOption70 = new InputVar<string>("OutPutOptionFile");
            ReadVar(outputOption70);
            parameters.OutputOption70 = outputOption70.Value.Actual;


            InputVar<string> outputDir = new InputVar<string>("OutPutDirectory");
            ReadVar(outputDir);
            parameters.OutputDir = outputDir.Value.Actual;

            InputVar<string> freq_out_put = new InputVar<string>("FrequencyOutPutOptionFile");
            ReadVar(freq_out_put);
            parameters.Freq_out_put = freq_out_put.Value.Actual;


            //---------------------------------------------------------------------------------
            //could not be directly got from the input parameter files
            parameters.Num_Iteration = PlugIn.ModelCore.EndTime / parameters.SuccessionTimestep; //numberOfIterations
            parameters.CellSize = (int)PlugIn.ModelCore.CellLength; //in landispro succession, the cellsize must be "int"


            //---------------------------------------------------------------------------------



            //parameters.dump();

            return parameters;
        }
    }



}
