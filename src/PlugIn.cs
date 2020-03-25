
#define LANDISPRO_ONLY_SUCCESSION


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
//using OSGeo.GDAL;
using OSGeo.OSR;
using Landis.Core;
using Landis.SpatialModeling;


namespace Landis.Extension.Succession.Density
{
    public class PlugIn : SuccessionMain
    {
        public static readonly string ExtensionName = "Density Succession";

        private static ISiteVar<SiteCohorts> sitecohorts;
        private static ICore modelCore;

        //public static int gl_currentDecade;
        //private static UInt16 currentHarvestEventId = 0;
        //private static map16 gl_visitMap = new map16();
        private static string[] ageMaps = new string[defines.MAX_RECLASS];

        
        private string fpLogFileSEC_name = null;

        private static List<string> SEC_landtypefiles = new List<string>();
        private static List<string> SEC_gisfiles = new List<string>();

        //change by YYF 2018/11
        public static int[] freq = new int[6];
        public static uint numSpecies;
        public static uint snr, snc;
        public static pdp pPDP = new pdp();
        public static string fpforTimeBU_name = null;
        public static double[] wAdfGeoTransform = new double[6];

        //change by YYF 2019/4
        public static int envOn;

        private static uint specAtNum;

        private static int numbOfIter;


        public static InputParameters   gl_param     = new InputParameters();
        public static speciesattrs gl_spe_Attrs = new speciesattrs(defines.MAX_SPECIES);
        public static landunits    gl_landUnits = new landunits(defines.MAX_LANDUNITS);
        public static sites            gl_sites = new sites();
        //=========================================================================

        public static uint NumSpecies { set { numSpecies = value; } }
        public PlugIn() : base(ExtensionName) { }
        //public PlugIn(string name, ExtensionType ty) : base(name, ty) { }



        //this destructor is used because in the original landispro program, there are still some operations 
        //after the main simulation loops
        ~PlugIn()
        {
            In_Output.AgeDistOutputFromBufferToFile();
            
            Console.WriteLine("Ending Landispro succession.");
        }

        public static ICore ModelCore
        {
            get { return modelCore; }
            private set { modelCore = value; }
        }

        public Land_type_Attributes gl_land_attrs = null;

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            //Console.WriteLine("Run: From {0} to {1}, current {2}", modelCore.StartTime, modelCore.EndTime, modelCore.CurrentTime);
            //Console.WriteLine("\n\nhere datefile = {0}\n\n", dataFile);
           
            InputParametersParser parser = new InputParametersParser();
            gl_param = Landis.Data.Load<InputParameters>(dataFile, parser);
            //Init_Output.GDLLMode = gl_param.read(dataFile);
            //PlugIn.gl_spe_Attrs.read(saFile);
            PlugIn.gl_spe_Attrs.read(PlugIn.gl_param.ExtraSpecAtrFile);

            //PlugIn.gl_landUnits.attach(PlugIn.gl_spe_Attrs);

            //PlugIn.gl_landUnits.read(PlugIn.gl_param.LandUnitFile);

            //PlugIn.gl_sites.BiomassRead(PlugIn.gl_param.Biomassfile);

            BiomassParamParser bioparser = new BiomassParamParser();
            Landis.Data.Load<BiomassParam>(PlugIn.gl_param.Biomassfile, bioparser);

            numSpecies = gl_spe_Attrs.NumAttrs;


            species.attach(PlugIn.gl_spe_Attrs);
            gl_landUnits.attach(PlugIn.gl_spe_Attrs);
            Land_type_AttributesParser parser2 = new Land_type_AttributesParser();
            gl_land_attrs = Landis.Data.Load<Land_type_Attributes>(gl_param.LandUnitFile, parser2);
            gl_landUnits.Copy(gl_land_attrs);
            Establishment_probability_AttributesParser parser3 = new Establishment_probability_AttributesParser();
            //var establish =
            Landis.Data.Load<Establishment_probability_Attributes>(gl_param.VarianceSECFile, parser3);

        }


        public override void Initialize()
        {
            DateTime now = DateTime.Now;
            //Gdal.AllRegister();
            Console.WriteLine("Start Landis Pro at {0}", now);

            Timestep = gl_param.SuccessionTimestep;
            gl_sites.SuccessionTimeStep = gl_param.SuccessionTimestep;
            In_Output.Init_IO();


            envOn = 0;
            int reclYear = 0;


            Console.Write("Beginning Landis 7.0 Pro Run\n Initializing...\n");

            if (envOn > 0)
                Console.Write("Environment will be updated every {0} iterarion\n", envOn);
            
            Density.Timestep.timestep = (uint)gl_param.SuccessionTimestep;

            numbOfIter = gl_param.Num_Iteration;

            //gl_sites.Stocking_x_value = gl_param.Stocking_x_value;
            //gl_sites.Stocking_y_value = gl_param.Stocking_y_value;
            //gl_sites.Stocking_z_value = gl_param.Stocking_z_value;
            
            gl_sites.CellSize = gl_param.CellSize;

#if !LANDISPRO_ONLY_SUCCESSION
                gl_sites.TimeStep_BDA     = gl_param.Timestep_BDA;
                gl_sites.TimeStep_Fire    = gl_param.Timestep_Fire;
                gl_sites.TimeStep_Fuel    = gl_param.Timestep_Fuel;
                gl_sites.TimeStep_Harvest = gl_param.Timestep_Harvest;  //Harvest Module
                gl_sites.TimeStep_Wind    = gl_param.Timestep_Wind;
#endif


            for (int x = 0; x < 5; x++)
                freq[x] = 1;

#if !LANDISPRO_ONLY_SUCCESSION
            if ((Init_Output.GDLLMode & defines.G_HARVEST) != 0)    //Harvest Module
                freq[5] = 1;

            if ((Init_Output.GDLLMode & defines.G_BDA) != 0)
                Console.Write("BDA ");

            if ((Init_Output.GDLLMode & defines.G_WIND) != 0)
                Console.Write("Wind ");

            if ((Init_Output.GDLLMode & defines.G_HARVEST) != 0)
                Console.Write("Harvest ");

            if ((Init_Output.GDLLMode & defines.G_FUEL) != 0)
                Console.Write("Fuel ");

            if ((Init_Output.GDLLMode & defines.G_FUELMANAGEMENT) != 0)
                Console.Write("Fuel management ");

            if ((Init_Output.GDLLMode & defines.G_FIRE) != 0)
                Console.Write("Fire ");

            if (Init_Output.GDLLMode != 0)
                Console.Write("are(is) on\n");
#endif

            //In_Output.getInput(freq, ageMaps, pPDP, BDANo, wAdfGeoTransform);
            SiteVars.Initialize();
            In_Output.getInput(freq, pPDP);

#if !LANDISPRO_ONLY_SUCCESSION
            if ((gDLLMode & DEFINES.G_HARVEST) != 0)
            {
                Console.WriteLine("Harvest Dll loaded in...");
                GlobalFunctions.HarvestPass(sites, speciesAttrs);
                sites.Harvest70outputdim();
            }
#endif

            Console.Write("Finish getting input\n");



            In_Output.OutputScenario();
            
            In_Output.initiateOutput_landis70Pro();



            snr = gl_sites.numRows;
            snc = gl_sites.numColumns;

            specAtNum = gl_spe_Attrs.NumAttrs;


            gl_sites.GetSeedDispersalProbability(null, gl_param.SeedRainFlag);

            gl_sites.GetSpeciesGrowthRates(gl_param.GrowthFlagFile, gl_param.GrowthFlag);

            gl_sites.GetSpeciesMortalityRates(gl_param.MortalityFile, gl_param.MortalityFlag);

            gl_sites.GetVolumeRead(gl_param.VolumeFile, gl_param.VolumeFlag);

            initiateRDofSite_Landis70();


            if (reclYear != 0)
            {
                int local_num = reclYear / gl_sites.SuccessionTimeStep;

                //Jacob reclass3.reclassify(reclYear, ageMaps);

                //Jacob In_Output.putOutput(local_num, local_num, freq);

                In_Output.putOutput_Landis70Pro(local_num, local_num, freq);

                In_Output.putOutput_AgeDistStat(local_num);
                
                Console.Write("Ending Landispro Succession.\n");
            }
            else
            {
                //Jacob In_Output.putOutput(0, 0, freq);

                In_Output.putOutput_Landis70Pro(0, 0, freq);

                In_Output.putOutput_AgeDistStat(0);
            }


            if (gl_param.RandSeed == 0)  //random
            {
                DateTime startTime = new DateTime(1970, 1, 1);
                gl_param.RandSeed = (int)Convert.ToUInt32(Math.Abs((DateTime.Now - startTime).TotalSeconds));
            }

            system1.fseed(gl_param.RandSeed);


            Console.WriteLine("gl_param.RandSeed = {0}", gl_param.RandSeed);


            if (envOn > gl_param.Num_Iteration)
                throw new Exception("Invalid year of interpretation for updating environment");

            fpforTimeBU_name  = gl_param.OutputDir + "/Running_Time_Stat.txt";
            fpLogFileSEC_name = gl_param.OutputDir + "/SECLog.txt";

            var now2 = DateTime.Now;

            Console.WriteLine("\nFinish the initilization at {0}", now2);

            var ltimeDiff = now2 - now;

            Console.Write("it took {0} seconds\n", ltimeDiff);
            gl_landUnits.initiateVariableVector(gl_param.Num_Iteration, gl_param.SuccessionTimestep, specAtNum, gl_param.FlagforSECFile);
            using (StreamWriter fpforTimeBU = new StreamWriter(fpforTimeBU_name))
            {
                fpforTimeBU.Write("Initilization took: {0} seconds\n", ltimeDiff);
            }
        }









        public static void succession_Landis70(pdp ppdp,int itr)
        {
            gl_sites.GetMatureTree();

            //Jacob ----- Test
            foreach (Site site in modelCore.Landscape.ActiveSites)
            {
                uint tempRow = (uint)gl_sites.convertLP_Row(site.Location.Row);
                uint tempCol = (uint)site.Location.Column;

                string tempName = site.Location.ToString();
                int mapCD = PlugIn.ModelCore.Ecoregion[site].MapCode;
                string erName = PlugIn.ModelCore.Ecoregion[site].Name;

                float local_RD = gl_sites[tempRow, tempCol].RD;

                landunit l = PlugIn.gl_landUnits[PlugIn.ModelCore.Ecoregion[PlugIn.ModelCore.Landscape.GetSite(site.Location.Row, site.Location.Column)].Name];

                site local_site = gl_sites[tempRow, tempCol];

                for (int k = 1; k <= specAtNum; ++k)
                {
                    local_site.SpecieIndex(k).GrowTree();
                }

                //BRM
                foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                {
                    foreach (Landis.Library.BiomassCohorts.Cohort cohort in speciesCohorts)
                        cohort.IncrementAge();
                }

            }


/*          Jacob ----- Old site iterator      
 *                //increase ages
                for (uint i = 1; i <= snr; ++i)
            {
                for (uint j = 1; j <= snc; ++j)
                {
                    ppdp.addedto_sTSLMortality(i, j, (short)gl_sites.SuccessionTimeStep);

                    //define land unit
                    landunit l = gl_sites.locateLanduPt(i, j);
                    //if (l != null && l.active())
                    if (l != null)
                    {
                        site local_site = gl_sites[i, j];

                        for (int k = 1; k <= specAtNum; ++k)
                        {
                            local_site.SpecieIndex(k).GrowTree();
                        }
                        
                    }

                }
                //Console.ReadLine();
            }*/

            //seed dispersal
            initiateRDofSite_Landis70();
            Console.WriteLine("Seed Dispersal:");

            //Jacob ----- Test
            foreach (Site site in modelCore.Landscape.ActiveSites)
            {
                uint tempRow = (uint)gl_sites.convertLP_Row(site.Location.Row);
                uint tempCol = (uint)site.Location.Column;

                string tempName = site.Location.ToString();
                int mapCD = PlugIn.ModelCore.Ecoregion[site].MapCode;
                string erName = PlugIn.ModelCore.Ecoregion[site].Name;

                float local_RD = gl_sites[tempRow, tempCol].RD;

                landunit l = PlugIn.gl_landUnits[PlugIn.ModelCore.Ecoregion[PlugIn.ModelCore.Landscape.GetSite(site.Location.Row, site.Location.Column)].Name];

                if (local_RD < l.MaxRDArray(0))

                    gl_sites.SiteDynamics(0, tempRow, tempCol);

                else if (local_RD >= l.MaxRDArray(0) && local_RD < l.MaxRDArray(1))

                    gl_sites.SiteDynamics(1, tempRow, tempCol);

                else if (local_RD >= l.MaxRDArray(1) && local_RD <= l.MaxRDArray(2))

                    gl_sites.SiteDynamics(2, tempRow, tempCol);

                else if (local_RD > l.MaxRDArray(2) && local_RD <= l.MaxRDArray(3))

                    gl_sites.SiteDynamics(3, tempRow, tempCol);

                else
                {
                    Debug.Assert(local_RD > l.MaxRDArray(3));
                    gl_sites.SiteDynamics(4, tempRow, tempCol);
                }
            }

/*          Jacob ----- Old site iterator  
             *            for (uint i = 1; i <= snr; ++i)
                        {
                            //Console.WriteLine("\n{0}%\n", 100 * i / snr);

                            for (uint j = 1; j <= snc; ++j)
                            {
                                //Console.WriteLine("i = {0}, j = {1}", i, j);
                                landunit l = gl_sites.locateLanduPt(i, j);
                                KillTrees(i, j);
                                if (l != null && l.active())
                                //if (l != null)
                                {
                                    float local_RD = gl_sites[i, j].RD;

                                    if (local_RD < l.MaxRDArray(0))

                                        gl_sites.SiteDynamics(0, i, j);

                                    else if (local_RD >= l.MaxRDArray(0) && local_RD < l.MaxRDArray(1))

                                        gl_sites.SiteDynamics(1, i, j);

                                    else if (local_RD >= l.MaxRDArray(1) && local_RD <= l.MaxRDArray(2))

                                        gl_sites.SiteDynamics(2, i, j);

                                    else if (local_RD > l.MaxRDArray(2) && local_RD <= l.MaxRDArray(3))

                                        gl_sites.SiteDynamics(3, i, j);

                                    else
                                    {
                                        Debug.Assert(local_RD > l.MaxRDArray(3));
                                        gl_sites.SiteDynamics(4, i, j);
                                    }
                                }
                            }

                        }*/
            
            Console.WriteLine("End density succession");
        }





        //initiating Landis70 RD values
        public static void initiateRDofSite_Landis70()
        {
            for (uint i = 1; i <= snr; ++i)
                for (uint j = 1; j <= snc; ++j)
                    gl_sites.GetRDofSite(i, j);
        }




        //start killing trees gradually at the 80 % longevity until they reach their longevity
        // modified version of function : void SUCCESSION::kill(SPECIE *s, SPECIESATTR *sa) 
        public static void KillTrees(uint local_r, uint local_c)
        {
            site local_site = gl_sites[local_r, local_c];

            for (int k = 1; k <= specAtNum; ++k)//sites.specNum
            {
                int longev = gl_spe_Attrs[k].Longevity;

                int numYears = longev / 5;

                float chanceMod = 0.8f / (numYears + 0.00000001f);

                float chanceDeath = 0.2f;

                int m_beg = (longev - numYears) / gl_sites.SuccessionTimeStep;
                int m_end = longev / gl_sites.SuccessionTimeStep;

                specie local_specie = local_site.SpecieIndex(k);

                for (int m = m_beg; m <= m_end; m++)
                {
                    int tmpTreeNum = (int)local_specie.getTreeNum(m, k);

                    int tmpMortality = 0;

                    if (tmpTreeNum > 0)
                    {
                        float local_threshold = chanceDeath * gl_sites.SuccessionTimeStep / 10;

                        for (int x = 1; x <= tmpTreeNum; x++)
                        {
                            if (system1.frand() < local_threshold)
                                tmpMortality++;
                        }
                        local_specie.setTreeNum(m, k, Math.Max(0, tmpTreeNum - tmpMortality));
                    }

                    chanceDeath += chanceMod;

                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////

        //                      SINGULAR LANDIS ITERATION ROUTINE                  //

        /////////////////////////////////////////////////////////////////////////////



        //This processes a singular Landis iteration.  It loops through each site
        //followed by each species.  For every iteration of the loop grow and kill
        //are called.  Then seed availability is checked.  If seed is available
        //and shade conditions are correct birth is called.
        public void singularLandisIteration(int itr, pdp ppdp)
        {
            DateTime ltime, ltimeTemp;
            TimeSpan ltimeDiff;


            using (StreamWriter fpforTimeBU = File.AppendText(fpforTimeBU_name))
            {
                fpforTimeBU.WriteLine("\nProcessing succession at Year: {0}:", itr);


                if (itr % gl_sites.SuccessionTimeStep == 0)
                {
                    ltime = DateTime.Now;

                    Console.WriteLine("Start succession ... at {0}", ltime);

                    system1.fseed(gl_param.RandSeed + itr / gl_sites.SuccessionTimeStep * 6);

                    gl_landUnits.ReprodUpdate(itr / gl_sites.SuccessionTimeStep);
                    //Console.WriteLine("random number: {0}", system1.frand());
                    succession_Landis70(ppdp,itr);
                    //Console.WriteLine("random number: {0}", system1.frand());
                    ltimeTemp = DateTime.Now;

                    ltimeDiff = ltimeTemp - ltime;

                    Console.WriteLine("Finish succession at {0} sit took {1} seconds", DateTime.Now, ltimeDiff);

                    fpforTimeBU.WriteLine("Processing succession: {0} seconds", ltimeDiff);

                    fpforTimeBU.Flush();
                }
            }
            
            system1.fseed(gl_param.RandSeed);
        }



        /*
        public static void updateLandtypeMap8(BinaryReader ltMapFile)
        {
            uint[] dest = new uint[32];

            //LDfread((char*)dest, 4, 32, ltMapFile);
            for (int i = 0; i < 32; i++)
                dest[i] = ltMapFile.ReadUInt32();


            int b16or8;
            if ((dest[1] & 0xff0000) == 0x020000)
                b16or8 = 16;
            else if ((dest[1] & 0xff0000) == 0)
                b16or8 = 8;
            else
            {
                b16or8 = -1;
                throw new Exception("Error: IO: Landtype map is neither 16 bit or 8 bit.");
            }

            uint nCols = dest[4];
            uint nRows = dest[5];


            uint xDim = gl_sites.numColumns;
            uint yDim = gl_sites.numRows;

            if ((nCols != xDim) || (nRows != yDim))
                throw new Exception("landtype map and species map do not match.");



            if (b16or8 == 8)  //8 bit
            {
                for (uint i = yDim; i > 0; i--)
                {
                    for (uint j = 1; j <= xDim; j++)
                    {
                        int coverType = ltMapFile.Read();

                        if (coverType >= 0)
                            gl_sites.fillinLanduPt(i, j, gl_landUnits[coverType]);
                        else
                            throw new Exception("illegel landtype class found.");
                    }
                }
            }
            else if (b16or8 == 16)  //16 bit
            {
                for (uint i = yDim; i > 0; i--)
                {
                    for (uint j = 1; j <= xDim; j++)
                    {
                        int coverType = ltMapFile.ReadUInt16();

                        if (coverType >= 0)
                            gl_sites.fillinLanduPt(i, j, gl_landUnits[coverType]);
                        else
                            throw new Exception("illegel landtype class found.");
                    }
                }
            }

        }
        */






        //Main program.  This contains start and shut down procedures as well as the main iteration loop.
        public override void Run()
        {
            int i = modelCore.TimeSinceStart;

            int i_d_timestep = i / gl_sites.SuccessionTimeStep;
            for (int r = 0; r < gl_landUnits.Num_Landunits; ++r)
            {
                gl_landUnits[r].MinShade = Land_type_Attributes.get_min_shade(r);
                float[] rd = new float[4];
                rd[0] = Land_type_Attributes.get_gso(0, r);
                rd[1] = Land_type_Attributes.get_gso(1, r);
                rd[2] = Land_type_Attributes.get_gso(2, r);
                rd[3] = gl_land_attrs.get_maxgso(i, r);
                gl_landUnits[r].MaxRDArrayItem = rd;
                gl_landUnits[r].MaxRD = rd[3];
            }

            //Simulation loops////////////////////////////////////////////////

            //for (int i = 1; i <= numbOfIter * gl_sites.TimeStep; i++)
            //{


            if (i % gl_sites.SuccessionTimeStep == 0)
            {
                if (gl_param.FlagforSECFile == 3)
                {
                    int index = i_d_timestep - 1;

                    if (index == 0)
                    {
                        SEC_landtypefiles.Clear();
                        SEC_gisfiles.Clear();
                        /*
                        using (StreamReader FpSECfile = new StreamReader(gl_param.VarianceSECFile))
                        {
                            int num_of_files = system1.read_int(FpSECfile);

                            for (int ii_count_num = 0; ii_count_num < num_of_files; ii_count_num++)
                            {
                                string SECfileMapGIS   = system1.read_string(FpSECfile);
                                string SECfileNametemp = system1.read_string(FpSECfile);

                                SEC_landtypefiles.Add(SECfileNametemp);
                                SEC_gisfiles.Add(SECfileMapGIS);
                            }
                        }
                                      */                  

                        if (index < gl_land_attrs.year_arr.Count)
                        {
                            /*string SECfileNametemp = SEC_landtypefiles[index];

                            string SECfileMapGIS = SEC_gisfiles[index];

                            gl_param.LandUnitFile = SECfileNametemp;

                            gl_landUnits.read(gl_param.LandUnitFile);
                            */
                            Console.Write("\nEnvironment parameter Updated.\n");
                            string SECfileMapGIS = gl_land_attrs.Get_new_landtype_map(index);
                            //strcpy(parameters.landUnitMapFile, SECfileMapGIS);

                            gl_param.LandImgMapFile = SECfileMapGIS;

                            /*Dataset simgFile = null;

                            if ((simgFile = (Dataset)Gdal.Open(gl_param.LandImgMapFile, Access.GA_ReadOnly)) == null) //* landtype.img
                            {
                                Console.Write("Land Map Img file %s not found.\n", gl_param.LandImgMapFile);

                                throw new Exception(gl_param.LandImgMapFile);
                            }
                            else
                            {
                                double[] adfGeoTransform = new double[6];

                                simgFile.GetGeoTransform(adfGeoTransform);
                                { 
                                    for (int ii = 0; ii < 6; ii++)
                                    {
                                        wAdfGeoTransform[ii] = adfGeoTransform[ii];
                                    }
                                }

                                updateLandtypeImg8(simgFile);

                                simgFile = null;
                            }
                            */

                            Console.WriteLine("\nEnvironment map Updated.");

                            landunit SECLog_use = gl_landUnits.first();

                            int ii_count = 0;


                            using (StreamWriter fpLogFileSEC = new StreamWriter(fpLogFileSEC_name))
                            {
                                fpLogFileSEC.Write("Year: {0}\n", i);

                                for (; ii_count < gl_landUnits.Num_Landunits; ii_count++)
                                {
                                    fpLogFileSEC.Write("Landtype{0}:\n", ii_count);


                                    for (int jj_count = 1; jj_count <= specAtNum; jj_count++)
                                    {
                                        fpLogFileSEC.Write("spec{0}: {1:N6}, ", jj_count, SECLog_use.probRepro(jj_count));
                                    }

                                    SECLog_use = gl_landUnits.next();

                                    fpLogFileSEC.Write("\n");
                                }
                            }
                        }
                    }


                    if (index > 0)
                    {
                        if (index < SEC_landtypefiles.Count)
                        {
                            /*gl_param.LandUnitFile = SEC_landtypefiles[index];
                            //gl_param.landUnitMapFile =      SEC_gisfiles[index];
                            gl_param.LandImgMapFile = SEC_gisfiles[index];

                            gl_landUnits.read(gl_param.LandUnitFile);
                            */
                            gl_param.LandImgMapFile = gl_land_attrs.Get_new_landtype_map(index);
                            Console.WriteLine("\nEnvironment parameter Updated.");


                            /*using(BinaryReader GISmap = new BinaryReader(File.Open(gl_param.landUnitMapFile, FileMode.Open)))
                            {
                                updateLandtypeMap8(GISmap);
                            }*/
                            /*Dataset ltimgFile = null;

                            if ((ltimgFile = (Dataset)Gdal.Open(gl_param.LandImgMapFile, Access.GA_ReadOnly)) == null) //* landtype.img
                            {
                                Console.WriteLine("Land Map Img file {0} not found.", gl_param.LandImgMapFile);

                                throw new Exception(gl_param.LandImgMapFile);
                            }
                            else
                            {
                                double[] adfGeoTransform = new double[6];

                                ltimgFile.GetGeoTransform(adfGeoTransform);
                                {
                                    for (int ii = 0; ii < 6; ii++)
                                    {
                                        wAdfGeoTransform[ii] = adfGeoTransform[ii];
                                    }
                                }

                                updateLandtypeImg8(ltimgFile);

                                ltimgFile = null;

                            }*/

                            Console.WriteLine("\nEnvironment map Updated.");

                            landunit SECLog_use = gl_landUnits.first();

                            using (StreamWriter fpLogFileSEC = new StreamWriter(fpLogFileSEC_name))
                            {
                                fpLogFileSEC.Write("Year: {0}\n", i);

                                int ii_count = 0;

                                for (; ii_count < gl_landUnits.Num_Landunits; ii_count++)
                                {
                                    fpLogFileSEC.Write("Landtype{0}:\n", ii_count);

                                    for (int jj_count = 1; jj_count <= specAtNum; jj_count++)
                                        fpLogFileSEC.Write("spec{0}: {1:N6}, ", jj_count, SECLog_use.probRepro(jj_count));

                                    SECLog_use = gl_landUnits.next();

                                    fpLogFileSEC.Write("\n");
                                }
                            }

                        }

                    }
                }

            }//end if

            Console.WriteLine("Processing succession at Year {0}", i);


            singularLandisIteration(i, pPDP);


            if (i % gl_sites.SuccessionTimeStep == 0 || i == numbOfIter * gl_sites.SuccessionTimeStep)
            {
                int[] frequency = new int[6] { 1, 1, 1, 1, 1, 1 };
                    
                if (i % (gl_sites.SuccessionTimeStep * freq[0]) == 0 && i_d_timestep <= numbOfIter)
                {
                    In_Output.putOutput_Landis70Pro(0, i_d_timestep, freq);
                }
                    
                if (i == gl_sites.SuccessionTimeStep * numbOfIter)
                {
                    In_Output.putOutput_Landis70Pro(0, numbOfIter, frequency);
                }

                /* Jacob   
                if (i % (gl_sites.SuccessionTimeStep * freq[4]) == 0 && i_d_timestep <= numbOfIter)
                {
                    In_Output.putOutput(0, i_d_timestep, freq);
                }
                */
                
                /* Jacob
                if (i == gl_sites.SuccessionTimeStep * numbOfIter)
                {
                    In_Output.putOutput(0, numbOfIter, frequency);
                }
                */

                In_Output.putOutput_AgeDistStat(i_d_timestep);

            }


            //}

            //Simulation loops end/////////////////////////////////////////////////


        }//end Run()

        public override void InitializeSites(string initialCommunities, string initialCommunitiesMap, ICore modelCore)
        {
            throw new NotImplementedException();
        }
    }


}
