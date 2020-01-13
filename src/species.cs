using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using Landis.Library.Cohorts;
using Landis.Core;
using System.Collections;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.Landispro
{
    public class species: Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
    	protected specie[] all_species;    //Array holding all species information.

        protected byte currentSpec;        //Current species number being pointed to by first and next access functions.

        protected static uint numSpec = 0;  //Number of species.

        public static uint NumSpec
        {
            get { return numSpec; }
        }

        protected static speciesattrs species_Attrs = null;//Pointer to an attached set of species Attributes.


		//Constructor.  This constructor can only be used on the first creation
		//instance of class species.  It sets the number of different varieties of species in the model.

		public species(uint n)
		{
			if (numSpec != 0)
                throw new Exception("SPECIES::SPECIES(int)-> Number of species may only be set once at construction.");

			all_species = new specie[n];
            for (int i = 0; i < n; i++ )
                all_species[i] = new specie(i);

			numSpec = n;

			currentSpec = 0;
		}




		//Constructor.  Should be used on all instance constructions other than the 
		//first.  This may be used on the first instance of construction but should 
		//be followed by a call to setNumber before the second construction instance.
		public species()
		{
			if (numSpec == 0)
			{
				all_species = null;
				currentSpec = 0;
			}
			else
			{
				all_species = new specie[numSpec];
				currentSpec = 0;

				for (int i = 0; i < numSpec; i++)
				{
                    all_species[i] = new specie(i);
					all_species[i].AGELISTAllocateVector(i);
				}

			}

		}




        public specie SpecieIndex(int n)
        {
            if (n > numSpec || n <= 0)
                throw new Exception("Specie Index Error out Bound");
            else
                return all_species[n - 1];
        }

        //public specie this[int i]
        //{
        //    get { return all_species[i];  }
        //    set { all_species[i] = value; }
        //}
        public specie this[int i]
        {
            get 
            {
                if (i > numSpec || i <= 0)
                    throw new Exception("Specie Index Error out Bound");           	

             	return all_species[i - 1];
            }
            
            private set 
            {
            	if (i > numSpec || i < 0)
             		throw new Exception("in species.cs: assignment error\n");

            	all_species[i - 1].copy(value);
            }
        }


        //This returns the number of species.
        public uint Num_Species
        {
            get { return numSpec; }
            
            set 
            {
                if (numSpec == 0)
                    numSpec = value;
                else
                    throw new Exception("SPECIES::setNumber()-> Number of species already set.");
            }
        }

        ISpeciesCohorts ISiteCohorts<ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                return this[species];
            }
        }

        public specie this[ISpecies species]
        {
            get
            {
                foreach (var i in all_species)
                    if (i.Species.Name == species.Name)
                        return i;
                return null;
            }
        }


        //If all instances of class species are destructed then the reset function 
        //may be called.  After it has been called instances may be constructed with
        //a different number of species present.
        public void reset()
		{
			numSpec = 0;
		}




		//Read a set of species from a file.
		public void read(StreamReader infile)
		{
			for (int i = 0; i < numSpec; i++)
			{
				all_species[i].readTreeNum(infile, i);

                all_species[i].initilizeDisPropagules(specAtt(i + 1).Maturity);
			}            
		}





		//Write a set of species to a file.
		//public void write(FILE *outfile)
        public void write(StreamWriter outfile)
		{
			for (int i = 0; i < numSpec; i++)
			{
				all_species[i].write(outfile);

                outfile.WriteLine(); //fprintf(outfile, "\n");
			}
		}





		//Referrence first species.
        public specie first()
		{
			currentSpec = 0;

			return all_species[currentSpec];
		}




		public specie current(int n)
		{
			if (n >= numSpec || n < 0)
				return null;
			else
			{
				currentSpec = (byte)n;

				return all_species[n];
			}
		}





		//Referrence next species.
		public specie next()
		{
			currentSpec++;

			if (currentSpec >= numSpec)
				return null;
			else
                return all_species[currentSpec];

		}



		//Referrence the current species attribute.
		public speciesattr specAtt()
		{
			if (species_Attrs == null)
				throw new Exception("specAtt()-> Species attributes not attached to species.");


			if (currentSpec >= numSpec)
				return null;
			else
                return species_Attrs[currentSpec + 1];

		}



		
		//Referrence the current species attribute.
		public speciesattr specAtt(int i)
		{
			if (species_Attrs == null)
				throw new Exception("specAtt()-> Species attributes not attached to species.");

			if (i > numSpec || i <= 0)
				throw new Exception("specAtt out of bound");

            return species_Attrs[i];
		}




		//This attaches a set of species attributes to all instances of species.
		public static void attach(speciesattrs s)
		{
			species_Attrs = s;
		}



		



		//This returns the youngest age.
		public int youngest()
		{
			int yngest = 10000;

			for (int i = 0; i < numSpec; i++)
			{
				yngest = Math.Min(yngest, all_species[i].youngest());
			}

			return yngest;
		}



		//This returns the oldest age among the youngest.
		public int oldest()
		{
			int odst = 0;

			for (int i = 0; i < numSpec; i++)
			{
				odst = (int)Math.Max(odst, all_species[i].oldest());
			}

			return odst;

		}





        //public void copy(species s)
        //{
        //    if (s == null)
        //        return;

        //    numSpec = s.number();

        //    all_species = new specie[numSpec];

        //    for (int i = 0; i < numSpec; i++)
        //        all_species[i].copy(s.all_species[i]);//all_species[i] = s.all_species[i];

        //    currentSpec = 0;
        //}

        public void copy(species s)
        {
            if (s == null)
                return;

            this.copy(s.all_species, species.numSpec);
        }



        public void copy(specie[] in_all_species, uint in_numSpec)
        {
            if (in_all_species == null)
                return;

            numSpec = in_numSpec;
            
            all_species = new specie[numSpec];

            for (int i = 0; i < numSpec; i++)
            {
                all_species[i] = new specie(i);
                all_species[i].copy(in_all_species[i]);
            }

            currentSpec = 0;
        }

        public bool IsMaturePresent(ISpecies species)
        {
            foreach (var i in all_species)
                if (i.Species.Name == species.Name)
                    return i.IsMaturePresent;
            return false;
        }

        public IEnumerator<specie> GetEnumerator()
        {
            foreach (var i in all_species)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RemoveMarkedCohorts(ICohortDisturbance disturbance)
        {
            foreach(var i in all_species)
                i.RemoveMarkedCohorts(disturbance);
        }

        public void RemoveMarkedCohorts(ISpeciesCohortsDisturbance disturbance)
        {
            foreach (var i in all_species)
                i.RemoveMarkedCohorts(disturbance);
        }

        public void AddNewCohort(ISpecies species)
        {
            var pos = this[species];
            if (pos != null)
                pos.AddNewCohort();
            else
                throw new NotImplementedException();
        }

        public void Grow(ushort years, ActiveSite site, int? successionTimestep, ICore mCore)
        {
            foreach (var i in all_species)
                for (int j = 0; j < years; ++j)
                    i.GrowTree();
        }

        IEnumerator<ISpeciesCohorts> IEnumerable<ISpeciesCohorts>.GetEnumerator()
        {
            foreach (var i in all_species)
                yield return i;
        }
    }
}
