using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
//using System.IO;
using Landis.Library;
using Edu.Wisc.Forest.Flel.Util;


namespace Landis.Extension.Succession.Landispro
{   

    public class landunits
    {
    	private landunit[] land_Units;          //Array holding all land units.      
        private int[] VectorIteration;  

		private uint numLU;                    //Number of land units.
        private uint numSpecies;

        //private int CurrentIteration;
		private int currentLU;                //Current land unit being pointed to by first and next access functions.
		private int maxLU;                    //Maximum number of land units.  Defined upon class construction.
        private int Totaliteration;        
        private int timestep;        

        private int flagforSECFile;


		//Constructor, sets upper limit for number of land units. 
		public landunits(int n = defines.MAX_LANDUNITS)
		{
			numLU 	  = 0;
			currentLU = 0;
			
			maxLU = n;

			land_Units = new landunit[n];

			for(int i=0; i<n; i++)
				land_Units[i] = new landunit();
		}



		~landunits()  
		{ 
			land_Units = null; 
			VectorIteration = null; 
		}


        //Returns number of land units.
        //public uint number_land_units  
        public uint Num_Landunits
        {
            get { return numLU; }
        }


		public void initiateVariableVector(int NumofIter, int temp, uint num, int flag)
		{
			timestep = temp;

			Totaliteration = NumofIter;

			numSpecies = num;

			flagforSECFile = flag;

			ReprodBackup();		
			

			VectorIteration = new int[NumofIter];

			int flagForVariance = 0;

			if (system1.frand1() > 0.5)
				flagForVariance = -1;
			else
				flagForVariance = 1;


			int onceVarianceLast = 0;

			if (flag == 0)
			{
				for (int i = 0; i < Totaliteration; i++)
					VectorIteration[i] = 0;
			}
			else if (flag == 1)
			{
				if (timestep >= 5)
				{
					for (int i = 0; i < Totaliteration; i++)
						VectorIteration[i] = 0;
				}
				else
				{
					for (int i = 0; i < Totaliteration;)
					{
						if (onceVarianceLast == 0)
						{
							int randnumber = system1.frandrand() % 2 + 3;

							onceVarianceLast = randnumber / timestep;

							if (onceVarianceLast == 0)
								onceVarianceLast = 1;

							flagForVariance = flagForVariance * (-1);

						}
						else
						{

							VectorIteration[i] = flagForVariance;

							i++;

							onceVarianceLast--;

						}

					}//end for

				}

			}//end if (flag == 1)
			else if (flag == 2)
			{
				for (int i = 0; i<Totaliteration; i++)
				{
					if (system1.frand1() > 0.5)
						VectorIteration[i] = 1;
					else
						VectorIteration[i] = -1;
				}

			}

		}




		public void ReprodBackup()
		{
			for (int i = 0; i < numLU; i++)
			{
                landunit local_landunit = land_Units[i];

				for (int j = 0; j < numSpecies; j++)
                    local_landunit.set_probReproductionOriginalBackup(j, local_landunit.get_probReproduction(j));
			}
		}




		public void ReprodUpdate(int year)
		{
			if (flagforSECFile == 3 || flagforSECFile == 0)
				return;

            float local_val = (1 + VectorIteration[year - 1]) * timestep / 10.0f;

			for (int i = 0; i < numLU; i++)
			{
                //for (int j = 0; j < numSpecies; j++)
                //    land_Units[i].probReproduction[j] = land_Units[i].probReproductionOriginalBackup[j] / 10 * timestep * local_val;
                
                landunit local_landunit = land_Units[i];
                
                for (int j = 0; j < numSpecies; j++)
                    local_landunit.set_probReproduction(j, local_landunit.get_probReproductionOriginalBackup(j) * local_val);
			}
		}

        internal void Copy(Land_type_Attributes gl_land_attrs)
        {
            numLU = (uint)Land_type_Attributes.Landtype_count;
            for (int i = 0; i < Land_type_Attributes.Landtype_count; ++i)
            {
                //land_Units[i] = new landunit();
                land_Units[i].Name = PlugIn.ModelCore.Ecoregions[i].Name;
                string name = land_Units[i].Name;
                if (string.Equals(name, "empty", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "road", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.PASSIVE;

                else if (string.Equals(name, "water", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.WATER;

                else if (string.Equals(name, "wetland", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.WETLAND;

                else if (string.Equals(name, "bog", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.BOG;

                else if (string.Equals(name, "lowland", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.LOWLAND;

                else if (string.Equals(name, "nonforest", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.NONFOREST;

                else if (string.Equals(name, "grassland", StringComparison.OrdinalIgnoreCase))

                    land_Units[i].Status = landunit.land_status.GRASSLAND;

                else

                    land_Units[i].Status = landunit.land_status.ACTIVE;
                land_Units[i].Index = i;
                land_Units[i].LtID = i;
                land_Units[i].MinShade = Land_type_Attributes.get_min_shade(i);
                land_Units[i].ProbReproductionOriginalBackup = new float[land_Units[i].Species_Attrs.NumAttrs];
            }
        }






        //Read set of land unit attributes from a file.
        public void read(string land_unit_file)
		{
            using (System.IO.StreamReader infile = new System.IO.StreamReader(land_unit_file))
            {
                numLU = 0;

                while (!system1.LDeof(infile))
                {
                    if (numLU < maxLU)
                    {
                        land_Units[numLU++].read(infile);

                        land_Units[numLU].LtID = (int)numLU;
                    }
                    else
                        throw new Exception("LANDUNITS::read(FILE*)-> Array bounds error.");

                }

                Console.Write("Number of land Units: {0}\n", numLU);
            }			
		}



		//Write set of land unit attributes to a file.
        public void write(System.IO.StreamWriter outfile)
		{
			for (int i = 0; i < numLU; i++)
				land_Units[i].write(outfile);	
		}


		//Dump set of land unit attributes to the CRT.
		public void dump()
		{
			for (int i = 0; i < numLU; i++)
			{
				land_Units[i].dump();

				Console.Write("===================================\n");
			}
		}



		//Attaches a set of species attributes to every land unit.
		//Must be performed following construction.
		public void attach(speciesattrs species_attrs_in)
		{
			for (int i = 0; i < maxLU; i++)
				land_Units[i].attach(species_attrs_in);	
		}



		//Referrence an attribute by species name.
		public landunit this [string name_in] 
		{
			get
            {
                for (int i = 0; i < numLU; i++)
                {
                    if (name_in == land_Units[i].Name)
                        return land_Units[i];
                }

                return null;
            }
		}



		//Referrence an attribute by species number.
		public landunit this [int id]
		{
            get
            {
                if (id > numLU || id < 0)
				    return null;
			    else
				    return land_Units[id];
            }
		}



		//Referrence first land unit attribute.
		public landunit first()
		{
			currentLU = 0;

			if (numLU == 0)
				return null;
			else
				return land_Units[0];
		}


		//Referrence next land unit attribute.
		//NOTE: All four referrence functions return NULL if attribute referrenced is illeagal or unavailable.
		public landunit next()
		{
			currentLU++;

			if (currentLU >= numLU)
				return null;
			else
				return land_Units[currentLU];
		}		   

    }

    public class Establishment_probability_Attributes
    {
        private static Dictionary<string, Dictionary<string, SortedDictionary<int, float>>> prob = new Dictionary<string, Dictionary<string, SortedDictionary<int, float>>>();
        public static void set_probability(string landtype, string specie, int year, float p)
        {
            if (!prob.ContainsKey(specie))
                prob.Add(specie, new Dictionary<string, SortedDictionary<int, float>>());
            if (!prob[specie].ContainsKey(landtype))
                prob[specie].Add(landtype, new SortedDictionary<int, float>());
            if(prob[specie][landtype].ContainsKey(year))
                throw new Exception("error in density-size-succession-dynamic-inputs.txt: multiple probability for specie " + specie + " in year " + year);
            prob[specie][landtype][year] = p;
        }
        public static float get_probability(string specie, string landtype, int year)
        {
            if (prob.ContainsKey(specie)&&prob[specie].ContainsKey(landtype))
            {
                return prob[specie][landtype].Last(x => x.Key <= year).Value;
            }
            else
                throw new Exception("density-size-succession-dynamic-inputs.txt: no specie " + specie + " information in "+landtype);
        }
    }

    class Establishment_probability_AttributesParser : Landis.TextParser<Establishment_probability_Attributes>
    {
        public override string LandisDataValue
        {
            get { return "Dynamic Input Data"; }
        }

        protected override Establishment_probability_Attributes Parse()
        {
            ReadLandisDataVar();
            Establishment_probability_Attributes ret = new Establishment_probability_Attributes();

            InputVar<int> int_val = new InputVar<int>("int value");
            InputVar<float> float_val = new InputVar<float>("float value");
            InputVar<string> string_val1 = new InputVar<string>("string value");
            InputVar<string> string_val2 = new InputVar<string>("string value");
            while (true)
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(int_val, currentLine);
                ReadValue(string_val1, currentLine);
                ReadValue(string_val2, currentLine);
                ReadValue(float_val, currentLine);
                //Console.WriteLine("{0} {1} {2} {3}", int_val.Value.Actual, string_val1.Value.Actual, string_val2.Value.Actual, float_val.Value.Actual);
                CheckNoDataAfter("float_val", currentLine);
                Establishment_probability_Attributes.set_probability(string_val1.Value.Actual, string_val2.Value.Actual, int_val.Value.Actual, float_val.Value.Actual);
                if(!GetNextLine())
                    break;
            }
            return ret;
        }
    }

    public class Land_type_Attributes 
    {
        private static float[,] growingSpaceOccupied;
        private static int[] min_shade;        

        //private float[] max_growingSpaceOccupied;
        private Dictionary<int, float[]> max_growingSpaceOccupied = new Dictionary<int, float[]>();

        private Dictionary<int, string> new_landtype_map = new Dictionary<int, string>();
        //public int Num_new_landtype_map { get; set; }

        public List<int> year_arr = new List<int>();

        public static void set_gso(int gsokind, int landtypekind, float value)
        {
            growingSpaceOccupied[gsokind, landtypekind] = value;
        }

        public static float get_gso(int gsokind, int landtypekind)
        {
            return growingSpaceOccupied[gsokind, landtypekind];
        }



        public void set_maxgso(int year, float[] value)
        {
            if (value.Length != Landtype_count)
                throw new Exception("max gso array is problematic\n");

            max_growingSpaceOccupied.Add(year, value);
        }

        public float get_maxgso(int year, int landtypekind)
        {
            while (!max_growingSpaceOccupied.ContainsKey(year)) year -= 1;
            return max_growingSpaceOccupied[year][landtypekind];
        }





        public static void set_min_shade(int landtypekind, int value)
        {
            min_shade[landtypekind] = value;
        }

        public static int get_min_shade(int landtypekind)
        {
            return min_shade[landtypekind];
        }



        //public Dictionary<int, string> New_landtype_map
        //{ get { return new_landtype_map; } set {new_landtype_map = value;} }
        public void Set_new_landtype_map(int key, string value)
        {
            if (new_landtype_map.ContainsKey(key))
            {
                new_landtype_map[key] = value;
            }
            else
            {
                new_landtype_map.Add(key, value);
            }
        }

        public string Get_new_landtype_map(int key)
        {
            string result = null;
            for (;key>=0;)
            {
                if (new_landtype_map.ContainsKey(key))
                {
                    result = new_landtype_map[key];
                    break;
                }
                key -= 1;
            }
            return result;
        }



        public static int Landtype_count { get; set; }

        static Land_type_Attributes()
        {
            Landtype_count = PlugIn.ModelCore.Ecoregions.Count;
            growingSpaceOccupied = new float[3, Landtype_count];
            min_shade = new int[Landtype_count];            
        }


        public Land_type_Attributes()
        {
            year_arr.Add(0);
        }
    }


    class Land_type_AttributesParser : Landis.TextParser<Land_type_Attributes>
    {
        public override string LandisDataValue
        {
            get { return "Land type Attributes"; }
        }

        protected override Land_type_Attributes Parse()
        {
            ReadLandisDataVar();

            InputVar<int> int_val = new InputVar<int>("int value");
            InputVar<float> float_val = new InputVar<float>("float value");            

            Land_type_Attributes lt_att = new Land_type_Attributes();

            StringReader currentLine = new StringReader(CurrentLine);

            int num_new_map_files = 0;

            if(PlugIn.gl_param.FlagforSECFile == 3)
            {                
                ReadValue(int_val, currentLine);
                num_new_map_files = int_val.Value.Actual;

                CheckNoDataAfter("num_new_map_files", currentLine);
                GetNextLine();
                
                InputVar<int> year = new InputVar<int>("year");
                InputVar<string> mapf = new InputVar<string>("new map file name");

                for (byte i = 0; i < num_new_map_files; i++)
                {
                    currentLine = new StringReader(CurrentLine);

                    ReadValue(year, currentLine);
                    ReadValue(mapf, currentLine);


                    lt_att.Set_new_landtype_map(year.Value.Actual, mapf.Value.Actual);
                    lt_att.year_arr.Add(year.Value.Actual);

                    CheckNoDataAfter("a new_map_file", currentLine);

                    GetNextLine();
                }

                currentLine = new StringReader(CurrentLine);
            }


            int num_landtype = Land_type_Attributes.Landtype_count;

            for (byte i = 0; i < num_landtype; i++)
            {
                ReadValue(int_val, currentLine);
                Land_type_Attributes.set_min_shade(i, int_val.Value.Actual);
            }

            CheckNoDataAfter("min_shade", currentLine);

            GetNextLine();


            for (byte gsokind = 0; gsokind < 3; gsokind++)
            {
                currentLine = new StringReader(CurrentLine);

                for(byte j = 0; j < num_landtype; j++)
                {
                    ReadValue(float_val, currentLine);

                    Land_type_Attributes.set_gso(gsokind, j, float_val.Value.Actual);
                }


                //CheckNoDataAfter("gso" + gsokind.ToString(), currentLine);

                GetNextLine();
            }


            for (int count = 0; count <= num_new_map_files; count++)
            {
                currentLine = new StringReader(CurrentLine);

                float[] max_gso_value = new float[num_landtype];

                for (byte j = 0; j < num_landtype; j++)
                {
                    ReadValue(float_val, currentLine);

                    max_gso_value[j] = float_val.Value.Actual;
                }

                lt_att.set_maxgso(lt_att.year_arr[count], max_gso_value);

                //CheckNoDataAfter("max_gso", currentLine);

                GetNextLine();
            }

            return lt_att;
        }
    }
}
