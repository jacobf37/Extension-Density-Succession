using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;


namespace Landis.Extension.Succession.Landispro
{
    public class speciesattrs
    {
		private speciesattr[] spec_Attrs;          //Array holding all species attributes.

		private uint numAttrs;                    //Number of species attributes.
		
        private int currentAttr;                 //Current species attribute being pointed to by first and next access functions.	
		private int maxAttrs;                    //Maximum number of attributes.  Defined upon class construction.
        private int MaxDistanceofAllSpecs;
        private int MaxShadeTolerance;

        //==========================================================================

        public int MaxDistance
        {
            get { return MaxDistanceofAllSpecs; }
        }


        public int MaxShade
        {
            get { return MaxShadeTolerance; }
        }


        //Returns number of species.
        public uint NumAttrs
        {
            get { return numAttrs; }
        }



		//Constructor, sets upper limit for number of species. 
		public speciesattrs(int n = defines.MAX_SPECIES)
		{
			numAttrs    = 0;
			currentAttr = 0;
			maxAttrs    = n;

			spec_Attrs = new speciesattr[n];
            for (int i = 0; i < n; i++)
                spec_Attrs[i] = new speciesattr();
		}

		
		//Read set of species attributes from a file
        public void read(string extraSpecAttrFile)
        {
            PlugIn.ModelCore.UI.WriteLine("Loading extra species attributes from file \"{0}\" ...", extraSpecAttrFile);
            extra_species_attrParser parser = new extra_species_attrParser();

            List<extra_species_attr> list_extra_speattr = new List<extra_species_attr>();

            try
            {
                list_extra_speattr = Landis.Data.Load<List<extra_species_attr>>(extraSpecAttrFile, parser);
            }
            catch (System.IO.FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", extraSpecAttrFile);
                throw new System.ApplicationException(mesg);
            }

            numAttrs = (uint)list_extra_speattr.Count;

            for (int i = 0; i < numAttrs; i++ )
            {
                spec_Attrs[i].read(list_extra_speattr[i], PlugIn.ModelCore.Species[i]);
            }

            Console.WriteLine("number of species attributes: {0}", numAttrs);

            MaxDistanceofAllSpecs = 0;

            for (int i = 0; i < numAttrs; i++)
            {
                if (spec_Attrs[i].Max_seeding_Dis >= MaxDistanceofAllSpecs)
                {
                    MaxDistanceofAllSpecs = spec_Attrs[i].Max_seeding_Dis;
                }

            }

            MaxShadeTolerance = 0;

            for (int i = 0; i < numAttrs; i++)
            {
                if (spec_Attrs[i].Shade_Tolerance >= MaxShadeTolerance && spec_Attrs[i].SpType >= 0)
                {
                    MaxShadeTolerance = spec_Attrs[i].Shade_Tolerance;
                }

            }

        }





























		//Write set of species attributes to a file.
		public void write(StreamWriter outfile)
        {
            for (int i = 0; i < numAttrs; i++)
	        {
		        spec_Attrs[i].write(outfile);
	        }
        }



		//Dump set of species attributes to the CRT.
		public void dump()
		{
			for (int i = 0; i < numAttrs; i++)
			{
				spec_Attrs[i].dump();

				Console.WriteLine("==================================");
			}
		}             



		//Referrence an attribute by species name.
		public speciesattr this[string name]
		{
			get
            {
                for (int i = 0; i < numAttrs; i++)
                {
                    if (name.Equals(spec_Attrs[i].Name))
                        return spec_Attrs[i];
                }

                return null;
            }
		}



		//Referrence an attribute by species number.
		public speciesattr this[int index]
		{
			get
            {
                if (index > numAttrs || index == 0)
                    throw new Exception("Specie Attributes out bound");

                return spec_Attrs[index - 1];
            }
		}




		//Referrence first species attribute.
		public speciesattr first()
		{
			currentAttr = 0;

			if (numAttrs == 0)
				return null;
			else
				return spec_Attrs[currentAttr];
		}



		//Referrence next species attribute.
		public speciesattr next()
		{
			currentAttr++;

			if (currentAttr >= numAttrs)
				return null;
			else
				return spec_Attrs[currentAttr];
		}

		



		//Return current number of species name 
		//been changed 		
		public int current(string name)
		{
			for (int i = 0; i < numAttrs; i++)
			{
                if (name.Equals(spec_Attrs[i].Name))
					return i;
			}

			// throw new Exception("does not exist\n");
			return -1;
		}	
	
	
    }
}
