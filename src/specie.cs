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

namespace Landis.Extension.Succession.Landispro
{
    public class specie : agelist, Library.AgeOnlyCohorts.ISpeciesCohorts
    {
        private short vegPropagules;      //Number of years of vegetative propagules present.
        private short disPropagules;      //Number of years of dispersed propagules present.

        private uint availableSeed;

        private int treesFromVeg;
        private uint matureTree;
        private int index;


        public short DisPropagules
        {
            get { return disPropagules; }
        }


        public short VegPropagules
        {
            get { return vegPropagules; }
            set { vegPropagules = value; }
        }


        public int TreesFromVeg
        {
            get { return treesFromVeg;  }
            set { treesFromVeg = value; }
        }


        public uint AvailableSeed
        {
            get { return availableSeed;  }
            set { availableSeed = value; }
        }

        internal void AddNewCohort()
        {
            agevector[0] += 1;
        }

        public uint MatureTree
        {
            get { return matureTree;  }
            set { matureTree = value; }
        }



        //Constructor.
        public specie(int index)
        {
            vegPropagules = 0;
            disPropagules = 0;
            this.index = index;
        }


        //Clear all specie values.
        public new void clear()
        {
            vegPropagules = 0;
            disPropagules = 0;

            base.clear();
        }



        //Kill all trees bounded by l and h.
        public void kill(int l, int h)
        {
            int num = (int)TimeStep;

            for (int i = l; i <= h; i += num)
                reset(i);
        }



        //Kill all trees bounded by l and h if and only if the age is present in a.
        public void kill(int l, int h, agelist a)
        {
            int num = (int)TimeStep;

            for (int i = l; i <= h; i += num)
            {
                if (a.query(i))
                    reset(i);
            }
        }



        //Read a specie from a file.
        //public void read(FILE *infile)
        public new void read(StreamReader infile)
        {
            //fscanc(infile,"%d ",&a);
            int a = system1.read_int(infile);

            vegPropagules = (short)a;

            base.read(infile);
        }



        //public void readTreeNum(FILE* infile, int specIndex)
        public new void readTreeNum(StreamReader infile, int specIndex)
        {
            //fscanc(infile,"%d ",&a);
            int a = system1.read_int(infile);
            //Console.WriteLine("a = {0}", a);
            vegPropagules = (short)a;


            base.readTreeNum(infile, specIndex);

            treesFromVeg = 0;
        }



        //Write a specie to a file.
        public new void write(StreamWriter outfile)
        {
            //fprintf(outfile,"%d ",vegPropagules);

            outfile.Write(vegPropagules);
            outfile.Write(' ');

            base.write(outfile);
        }


        //public void initilizeDisPropagules(int maturity, char * name)
        public void initilizeDisPropagules(int maturity)
        {
            if (oldest() >= maturity)
            {
                disPropagules = 1; //can disperse, non OS
            }
        }



        public void updateDispropagules(int maturity)
        {
            if (oldest() >= maturity)
                disPropagules = 1; //disPropagules = true;
            else
                disPropagules = 0; //disPropagules = false;
        }



        //Adds trees to age ten.
        public void birth()
        {
            set((int)TimeStep);
        }




        //the function in the c++ version is deprecated
        ////Grows trees ten years.
        //public void grow() 
        //{
        //    shiftOld();
        //} 


        //Kills trees of a given age class.
        public void kill(int i)
        {
            reset(i);
        }
        



        public void copy(specie in_specie)        
        {
            if(in_specie == null)
                return;

            vegPropagules = in_specie.vegPropagules;      
            
            disPropagules = in_specie.disPropagules;

            availableSeed = in_specie.availableSeed;

            treesFromVeg  = in_specie.treesFromVeg;

            matureTree    = in_specie.MatureTree;

            index = in_specie.index;

            base.copy(in_specie.agevector);
        }

        public void copy(SpeciesParam x, int index)
        {
            vegPropagules = x.VegPropagules(index);
            //Console.WriteLine("{0} {1}", x.Count, index);
            base.copy(x.Agevector(index));
        }

        internal void setAgeVector(uint[] v)
        {
            if (agevector[0] != 0 && v[0] != 0)
                Console.WriteLine("WARNING: agevector[0] = " + agevector[0].ToString());
            else if (agevector[0] != 0)
                v[0] = agevector[0];
            agevector = v;
        }

        private class MyCohort : Landis.Library.Cohorts.ICohort
        {
            private int number, age;
            private ISpecies spe;

            public MyCohort(ISpecies spe, int n, int a)
            {
                this.spe = spe;
                this.number = n;
                this.age = a;
            }

            public ISpecies Species
            {
                get
                {
                    return spe;
                }
            }
        }

        public IEnumerator<Landis.Library.Cohorts.ICohort> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                if (agevector[i] > 0)
                    yield return new MyCohort(Species, (int)agevector[i], i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Landis.Library.Cohorts.ICohort>)this).GetEnumerator();
        }

        internal int Length
        {
            get { return agevector.Length; }
        }

        public int Count
        {
            get
            {
                int ret=0;
                for (int i = 0; i < Length; ++i)
                    if (agevector[i] != 0)
                        ret += 1;
                return ret;
            }
        }

        public ISpecies Species
        {
            get
            {
                return PlugIn.ModelCore.Species[index];
            }
        }

        public bool IsMaturePresent
        {
            get
            {
                for (int i = PlugIn.ModelCore.Species[index].Longevity / PlugIn.gl_param.SuccessionTimestep; i < Length; ++i)
                    if (agevector[i] > 0)
                        return true;
                return false;
            }
        }

        internal void RemoveMarkedCohorts(ISpeciesCohortsDisturbance disturbance)
        {
            throw new NotImplementedException();
        }

        internal void RemoveMarkedCohorts(ICohortDisturbance disturbance)
        {
            for (int i = 0; i < agevector.Length; ++i)
                if (agevector[i] > 0)
                    if (disturbance.MarkCohortForDeath(new Library.AgeOnlyCohorts.Cohort(Species, (ushort)i)))
                        agevector[i] = 0;
        }

        IEnumerator<Library.AgeOnlyCohorts.ICohort> IEnumerable<Library.AgeOnlyCohorts.ICohort>.GetEnumerator()
        {
            for (int i = 0; i < agevector.Length; ++i)
                if(agevector[i]>0)
                    yield return new Library.AgeOnlyCohorts.Cohort(Species, (ushort)agevector[i]);
        }
    }
}
