using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Landis.Extension.Succession.Landispro
{
    public class parameters
    {
        // NO_DISPERSAL      //No seed dispersal.
        // UNIFORM           //Uniform seed dispersal.
        // NEIGHBORS         //Seed to immediate neighbors.
        // DISPERSAL         //Seed within effective distance.
        // RAND_ASYM         //Seed using interpolated chaotic distances.
        // MAX_DIST          //RAND_ASYM up to maximum distances.
        // SIM_RAND_ASYM     //RAND_ASYM up to maximum distances, Simulated random asymptopic after maximum distance.
        public enum dispersal_type { NO_DISPERSAL, UNIFORM, NEIGHBORS, DISPERSAL, RAND_ASYM, MAX_DIST, SIM_RAND_ASYM };

        public int numberOfIterations = 0;
        public int numberOfReplicates = 0;
        public int randSeed;//Random number seed.
        public int cellSize;//Length of side of cell in meters.
        public int timestep;
        public int timestep_Wind;
        public int timestep_Fire;
        public int timestep_BDA;
        public int timestep_Fuel;
        public int timestep_Harvest;
        
        public int flagforSECFile; //0, 1, 2, 3 //0 is no change, 1 is pulse, 2 is random, 3 read from file 
        public int GrowthFlag;
        public int MortalityFlag;
        public int SeedRainFlag;
        public int VolumeFlag;            
        public int dispRegime;//Seeding regime: NO_DISPERSAL, UNIFORM, NEIGHBORS, DISPERSAL, or INTERPOLATE.
        

        public double stocking_x_value;
        public double stocking_y_value;
        public double stocking_z_value;

        public StreamReader FpSECfile;
        

        public int fire;            //Turn fire disturbances on/off.
        public int harvest;           //Turn harvest events on/off.
        public int standAdjacencyFlag;//Turn stand adjacency flag on/off.
        public int harvestDecadeSpan; //Sites harvested within this time span are considered
        
        //If the number of "recently harvested" sites on a divided by the number of active sites on a stand
        //is greater than or equal to this value, consider the stand "recently harvested".
        public float harvestThreshold;


        
        public string varianceSECFile;//Customized By user year variances
        public string specAttrFile;  //Species Attributes input file name.
        public string landUnitFile;  //Landtype attribute file name.
        public string landImgMapFile;  //Landtype map file name
        //public string landUnitMapFile;//landtype map file name. 
        //public string siteInFile;//Site input file name.
        public string siteImgFile;//Site input file name.
        public string reclassInFile;//Reclassification input file name.
        //public string reclassOutFile;//Reclassification output file name.
        //public string ageIndexFile;//species age output index file
        public string OutputOption70;//landis 7.0 output option
        public string outputDir;//Output directory.
        public string disturbance;       //Disturbance regime file name.
        //public string default_plt;//For map output
        public string freq_out_put;//Output map frequency file
        public string Biomassfile;
        public string GrowthFlagFile;
        public string MortalityFile;
        public string VolumeFile;
        public string strWindInitName;//Turn wind disturbances on/off.
        public string strFireInitName;
        public string strBDAInitName;//BDAInit.dat directory and file name
        public string strHarvestInitName;
        public string strFuelInitName;

        

        public parameters()
        {
            numberOfIterations = 0;
            numberOfReplicates = 0;
            randSeed = 0;
        }



        public int read(StreamReader infile, ref int BDANo)
        {
            Console.Write("Reading from parameter File\n\n");

            specAttrFile    = system1.read_string(infile);
            landUnitFile    = system1.read_string(infile);
            landImgMapFile  = system1.read_string(infile);
            //landUnitMapFile = system1.read_string(infile);
            //siteInFile      = system1.read_string(infile);
            siteImgFile     = system1.read_string(infile);
            reclassInFile   = system1.read_string(infile);
            //reclassOutFile  = system1.read_string(infile);
            //ageIndexFile    = system1.read_string(infile);
            OutputOption70  = system1.read_string(infile);
            outputDir       = system1.read_string(infile);
            //default_plt     = system1.read_string(infile);
            freq_out_put    = system1.read_string(infile);
            Biomassfile     = system1.read_string(infile);

            numberOfIterations = system1.read_int(infile);
            randSeed           = system1.read_int(infile);
            cellSize           = system1.read_int(infile);
            
            string dispType;
            dispType = system1.read_string(infile);
            
            timestep         = system1.read_int(infile);
            timestep_Wind    = system1.read_int(infile);
            timestep_Fire    = system1.read_int(infile);
            timestep_BDA     = system1.read_int(infile);
            timestep_Fuel    = system1.read_int(infile);
            timestep_Harvest = system1.read_int(infile);

            varianceSECFile = system1.read_string(infile);
            
            if (varianceSECFile == "N/A")
                flagforSECFile = 0;
            else if (varianceSECFile == "0")
                flagforSECFile = 0;
            else if (varianceSECFile == "1")
                flagforSECFile = 1;
            else if (varianceSECFile == "2")
                flagforSECFile = 2;
            else
                flagforSECFile = 3;

            GrowthFlagFile = system1.read_string(infile);
            if (GrowthFlagFile == "N/A")
                GrowthFlag = 0;
            else if (GrowthFlagFile == "0")
                GrowthFlag = 0;
            else
                GrowthFlag = 1;

            MortalityFile = system1.read_string(infile);
            if (MortalityFile == "N/A")
                MortalityFlag = 0;
            else if (MortalityFile == "0")
                MortalityFlag = 0;
            else
                MortalityFlag = 1;

            
            SeedRainFlag = 0;


            VolumeFile = system1.read_string(infile);
            if (VolumeFile == "N/A")
                VolumeFlag = 0;
            else if (VolumeFile == "0")
                VolumeFlag = 0;
            else
                VolumeFlag = 1;


            stocking_x_value = system1.read_float(infile);
            stocking_y_value = system1.read_float(infile);
            stocking_z_value = system1.read_float(infile);


            strWindInitName = system1.read_string(infile);



            int dllmode = 0;

            if (strWindInitName != "N/A")
                dllmode = dllmode | defines.G_WIND;

            strFireInitName = system1.read_string(infile);
            if (strFireInitName != "N/A")
                dllmode = dllmode | defines.G_FIRE;

            strBDAInitName = system1.read_string(infile);
            if (strBDAInitName != "N/A")
            {
                dllmode = dllmode | defines.G_BDA;
                BDANo = int.Parse(File.ReadAllText(strBDAInitName));
            }

            strFuelInitName = system1.read_string(infile);
            if (strFuelInitName != "N/A")
                dllmode = dllmode | defines.G_FUEL;

            strHarvestInitName = system1.read_string(infile);
            if (strHarvestInitName != "N/A")
                dllmode = dllmode | defines.G_HARVEST;


            dispRegime = (int)(dispersal_type)Enum.Parse(typeof(dispersal_type), dispType);

            
            return dllmode;
        }




        //This shall dump the contents of parameters.
        void dump()        
        {

            Console.Write("specAttrFile:       {0}\n", specAttrFile);

            Console.Write("landUnitFIle:       {0}\n", landUnitFile);

            //Console.Write("landUnitMapFIle:    {0}\n", landUnitMapFile);
            Console.Write("landImgFile:        {0}\n", landImgMapFile);
            //Console.Write("siteInFile:         {0}\n", siteInFile);
            Console.Write("siteImgFile:        {0}\n", siteImgFile);
            Console.Write("reclassInFile:      {0}\n", reclassInFile);

            //Console.Write("reclassOutFile:     {0}\n", reclassOutFile);

            //Console.Write("ageIndexFile:       {0}\n", ageIndexFile);

            Console.Write("outputDir:          {0}\n", outputDir);

            Console.Write("disturbance:        {0}\n", disturbance);

            //Console.Write("default_plt:        {0}\n", default_plt);

            Console.Write("freq_out_put:		{0}\n", freq_out_put);

            Console.Write("numberOfIterations: {0}\n", numberOfIterations);

            Console.Write("randSeed:           {0}\n", randSeed);

            Console.Write("cellSize:           {0}\n", cellSize);

            switch (dispRegime)
            {
                case (int)dispersal_type.NO_DISPERSAL: Console.Write("dispRegime:         NO_DISPERSAL\n"); break;

                case (int)dispersal_type.UNIFORM: Console.Write("dispRegime:         UNIFORM\n"); break;

                case (int)dispersal_type.NEIGHBORS: Console.Write("dispRegime:         NEIGHBORS\n"); break;

                case (int)dispersal_type.DISPERSAL: Console.Write("dispRegime:         DISPERSAL\n"); break;

                case (int)dispersal_type.RAND_ASYM: Console.Write("dispRegime:         RAND_ASYM\n"); break;

                case (int)dispersal_type.MAX_DIST: Console.Write("dispRegime:         MAX_DIST\n"); break;

                case (int)dispersal_type.SIM_RAND_ASYM: Console.Write("dispRegime:         SIM_RAND_ASYM\n"); break;

                default: throw new Exception("PARAMETERS::dump()-> Illegal dispRegime");
            }

            Console.Write("fire flag:          {0}\n", fire);
            
        }



        //This shall write the contents of parameters to outfile
        void write(StreamWriter outfile)        
        {
            outfile.Write("specAttrFile:       {0}\n", specAttrFile);

            outfile.Write("landUnitFIle:       {0}\n", landUnitFile);

            //outfile.Write("landUnitMapFIle:    {0}\n", landUnitMapFile);
            outfile.Write("landImgFile:        {0}\n", landImgMapFile);
            //outfile.Write("siteInFile:         {0}\n", siteInFile);
            outfile.Write("siteImgFile:        {0}\n", siteImgFile);
            outfile.Write("reclassInFile:      {0}\n", reclassInFile);

            //outfile.Write("reclassOutFile:     {0}\n", reclassOutFile);

            //outfile.Write("ageIndexFile:       {0}\n", ageIndexFile);

            outfile.Write("outputDir:          {0}\n", outputDir);

            outfile.Write("disturbance:        {0}\n", disturbance);

            //outfile.Write("default_plt:        {0}\n", default_plt);

            outfile.Write("freq_out_put:		{0}\n", freq_out_put);

            outfile.Write("numberOfIterations: {0}\n", numberOfIterations);

            outfile.Write("randSeed:           {0}\n", randSeed);

            outfile.Write("cellSize:           {0}\n", cellSize);


            switch (dispRegime)
            {
                case (int)dispersal_type.NO_DISPERSAL: 
                    outfile.Write("dispRegime:         NO_DISPERSAL\n"); break;

                case (int)dispersal_type.UNIFORM: 
                    outfile.Write("dispRegime:         UNIFORM\n"); break;

                case (int)dispersal_type.NEIGHBORS: 
                    outfile.Write("dispRegime:         NEIGHBORS\n"); break;

                case (int)dispersal_type.DISPERSAL: 
                    outfile.Write("dispRegime:         DISPERSAL\n"); break;

                case (int)dispersal_type.RAND_ASYM: 
                    outfile.Write("dispRegime:         RAND_ASYM\n"); break;

                case (int)dispersal_type.MAX_DIST: 
                    outfile.Write("dispRegime:         MAX_DIST\n"); break;

                case (int)dispersal_type.SIM_RAND_ASYM: 
                    outfile.Write("dispRegime:         SIM_RAND_ASYM\n"); break;

                default: 
                    throw new Exception("PARAMETERS::dump()-> Illegal dispRegime");

            }

            outfile.Write("fire flag:          {0}\n", fire);

        }
    }
}
