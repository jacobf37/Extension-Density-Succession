using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Landis.Extension.Succession.Landispro
{
    public class landunit
    {
        public enum land_status { PASSIVE, ACTIVE, WATER, WETLAND, BOG, LOWLAND, NONFOREST, GRASSLAND };


        private int index;       //used for luReclass
        private int minShade;    //minimum number of shade years that must be present before shade 5 species may seed in on this site.        
        private int ltID;        //Landtype ID for fuel by Vera

        private string name; //Land unit name.

        private float maxRD;

        private float[] maxRDArray = new float[4];
        
        //Set of probabilities of reproduction of all elements of s where s is of type SPECATTRIBS.
        //Each probability is the chance of reproduction given that seeds are present and there is no competition.
        //private float[] probReproduction;

        private float[] probReproductionOriginalBackup;

        public float[] ProbReproductionOriginalBackup
        {
            set { probReproductionOriginalBackup = value; }
        }

        private speciesattrs species_Attrs;
        public speciesattrs Species_Attrs
        {
            get { return species_Attrs; }
        }

        private land_status status;
        public land_status Status
        {
            set { status = value; }
        }

        //=======================================================================================

        public int MinShade { get { return minShade; } set { minShade = value; } }        
        public string Name {
            get { return name; }
            set { name = value; }
        }
        public float MaxRD
        {
            get { return maxRD; }
            set { maxRD = value; }
        }

        public int LtID 
        {
            get { return ltID; }
            set { ltID = value; } 
        }

        public float[] MaxRDArrayItem
        {
            get { return maxRDArray; }
            set{ maxRDArray=value;}
        }

        public float MaxRDArray(int i)
        {
            return maxRDArray[i];
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }


        public float get_probReproduction(int index)
        {
            return Establishment_probability_Attributes.get_probability(species_Attrs[index + 1].Name, name, PlugIn.ModelCore.TimeSinceStart);
            //return probReproduction[index];
        }

        public void set_probReproduction(int index, float value)
        {
            Establishment_probability_Attributes.set_probability(name, species_Attrs[index + 1].Name, PlugIn.ModelCore.TimeSinceStart, value);
            //probReproduction[index] = value;
        }



        public float get_probReproductionOriginalBackup(int index)
        {
            return probReproductionOriginalBackup[index];
        }

        public void set_probReproductionOriginalBackup(int index, float value)
        {
            probReproductionOriginalBackup[index] = value;
        }



        public landunit()
        {            
            minShade = 0;
            ltID     = 0;

            name                           = null;
            species_Attrs                  = null;
            //probReproduction               = null;
            probReproductionOriginalBackup = null;                
        }


        //Attaches a set of species attributes to the land unit.  
        //Must be performed following construction.
        public void attach(speciesattrs spe_attr)
        {
            species_Attrs = spe_attr;
        }


        //Read a land unit from a file.
        public void read(StreamReader infile)
        {
            if (species_Attrs == null)
                throw new Exception("LANDUNIT::read(FILE*)-> No attaced species attributes.");


            uint specAtNum = species_Attrs.NumAttrs;

            name = system1.read_string(infile);


            minShade = system1.read_int(infile);

            maxRDArray[0] = system1.read_float(infile);
            maxRDArray[1] = system1.read_float(infile);
            maxRDArray[2] = system1.read_float(infile);
            maxRDArray[3] = system1.read_float(infile);

            maxRD = maxRDArray[3];


            //probReproduction               = new float[specAtNum];
            probReproductionOriginalBackup = new float[specAtNum];


            for (int i = 0; i < specAtNum; i++)
            {
                //probReproduction[i] = system1.read_float(infile);
            }


            if (string.Equals(name, "empty", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "road", StringComparison.OrdinalIgnoreCase))

                status = land_status.PASSIVE;

            else if (string.Equals(name, "water", StringComparison.OrdinalIgnoreCase))

                status = land_status.WATER;

            else if (string.Equals(name, "wetland", StringComparison.OrdinalIgnoreCase))

                status = land_status.WETLAND;

            else if (string.Equals(name, "bog", StringComparison.OrdinalIgnoreCase))

                status = land_status.BOG;

            else if (string.Equals(name, "lowland", StringComparison.OrdinalIgnoreCase))

                status = land_status.LOWLAND;

            else if (string.Equals(name, "nonforest", StringComparison.OrdinalIgnoreCase))

                status = land_status.NONFOREST;

            else if (string.Equals(name, "grassland", StringComparison.OrdinalIgnoreCase))

                status = land_status.GRASSLAND;

            else

                status = land_status.ACTIVE;

        }


        //Write a land unit to a file.
        //original function is problematic!!!!!!!!!
        public void write(StreamWriter outfile)
        {
            if (species_Attrs == null)
                throw new Exception("LANDUNIT::read(FILE*)-> No attaced species attributes.");

            uint specAtNum = species_Attrs.NumAttrs;

            outfile.Write("{0} {1} ", minShade, name);

            //for (int i = 0; i < specAtNum; ++i)
            //    outfile.Write("{0} ", probReproduction[i].ToString("n2"));

            outfile.Write('\n');
        }


        public void dump()
        {
            if (species_Attrs == null)
                throw new Exception("LANDUNIT::read(FILE*)-> No attaced species attributes.");

            uint specAtNum = species_Attrs.NumAttrs;

            Console.WriteLine("Name:          {0}", name);

            //for (int i = 0; i < specAtNum; ++i)
                //Console.WriteLine("{0}: {1}", species_Attrs[i + 1].Name, probReproduction[i]);
        }


        //Returns the probability of reproduction of the given species on the land unit.
        //Species is referrenced by number.
        public float probRepro(int index_in)
        {
            if (index_in <= species_Attrs.NumAttrs && index_in > 0)
                //return probReproduction[index_in - 1];
                return Establishment_probability_Attributes.get_probability(species_Attrs[index_in].Name, name, PlugIn.ModelCore.TimeSinceStart);
            throw new Exception("LANDUNIT::probRepro(int)-> Array bounds error.");

            // return 0.0f;
        }


        //Returns the probability of reproduction of the given species on the land unit.
        //Species is referrenced by name.
        public float probRepro(string name)
        {
            uint specAtNum = species_Attrs.NumAttrs;
            try
            {
                return Establishment_probability_Attributes.get_probability(name, this.name, PlugIn.ModelCore.TimeSinceStart);
            }
            //for (int i = 0; i < specAtNum; ++i)
            //{
            //    if (species_Attrs[i + 1].Name == name)
            //return probReproduction[i];
            //}
            catch
            {
                throw new Exception("LANDUNIT::probRepro(char*)-> Illegal species name.");
            }
            // return 0.0f;
        }


        //Returns the probability of reproduction of the given species on the land unit.
        //Species is referrenced by species attribute class.
        public float probRepro(speciesattr species_attr)
        {
            uint specAtNum = species_Attrs.NumAttrs;
            try
            {
                return Establishment_probability_Attributes.get_probability(species_attr.Name, this.Name, PlugIn.ModelCore.TimeSinceStart);
            }
            //for (int i = 0; i < specAtNum; ++i)
            //{
            //    if (species_Attrs[i + 1].Name == species_attr.Name)
            //        return probReproduction[i];
            //}
            catch {
                throw new Exception("LANDUNIT::probRepro(SPECIESATTR*)-> Illegal spec. attr.");
            }
            // return 0.0f;
        }


        public int Get_status()
        {
            return Convert.ToInt32(status);
        }


        //A landunit type is either active or inactive.
        //Inactive land units are not processed.
        public bool active()
        {
            return status == land_status.ACTIVE;
        }


        //Returns true if a land unit type is water, false otherwise.
        public bool water()
        {
            return status == land_status.WATER;
        }


        //Returns true if a land unit type is lowland, false otherwise
        public bool lowland()
        {
            return (status == land_status.WETLAND || status == land_status.BOG || status == land_status.LOWLAND || status == land_status.NONFOREST);
        }
    }
}
