using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Landis.Core;
using Landis.Library.Cohorts;

namespace Landis.Extension.Succession.Landispro
{
    class CohortData:IComparable<CohortData>
    {
        int num, age;
        public CohortData(int n, int a){
            num = n;
            age = a;
        }

        public int Num
        {
            get
            {
                return num;
            }
            set
            {
                num = value;
            }
        }

        public int Age
        {
            get
            {
                return age;
            }
        }

        public int CompareTo(CohortData other)
        {
            return Age.CompareTo(other.Age);
        }
    }
    class MyCohorts : ISpeciesCohorts<ICohort>
    {
        static readonly uint[] mask = new uint[32]
        {
            0x00000001, 0x00000002, 0x00000004, 0x00000008,
            0x00000010, 0x00000020, 0x00000040, 0x00000080,
            0x00000100, 0x00000200, 0x00000400, 0x00000800,
            0x00001000, 0x00002000, 0x00004000, 0x00008000,
            0x00010000, 0x00020000, 0x00040000, 0x00080000,
            0x00100000, 0x00200000, 0x00400000, 0x00800000,
            0x01000000, 0x02000000, 0x04000000, 0x08000000,
            0x10000000, 0x20000000, 0x40000000, 0x80000000
        };

        internal void AddNewCohort(ISpecies species)
        {
            age[species].Add(new CohortData(1,0));
        }

        private short vegPropagules;      //Number of years of vegetative propagules present.
        private short disPropagules;      //Number of years of dispersed propagules present.

        private uint availableSeed;

        private int treesFromVeg;
        private uint matureTree;
        private List<ISpecies> spe;
        private List<List<CohortData>> age = new List<List<CohortData>()>;

        public void copy(MyCohorts in_specie)
        {
            if (in_specie == null)
                return;

            vegPropagules = in_specie.VegPropagules;

            disPropagules = in_specie.DisPropagules;

            availableSeed = in_specie.AvailableSeed;

            treesFromVeg = in_specie.TreesFromVeg;

            matureTree = in_specie.MatureTree;

            age.Clear();

            for (int i = 0; i < in_specie.Count(); ++i)
                for(int j=0;j<in_specie[i].Count();++i)
                    age[j].Add(new CohortData(in_specie.age[i][j].Num, in_specie.age[i][j].Age));
        }

        public List<CohortData> this[int idx]
        {
            get
            {
                return age[idx];
            }
        }

        public int this[int idx,int ag]
        {
            get
            {
                for (int i = 0; i < age.Count; ++i)
                    if (age[idx][i].Age == ag)
                        return age[idx][i].Num;
                return 0;
            }
            set
            {
                for (int i = 0; i < age.Count; ++i)
                    if (age[idx][i].Age == ag)
                    {
                        age[idx][i].Num = value;
                        return;
                    }
                age[idx].Add(new CohortData(value, ag));
                age[idx].Sort();
            }
        }

        public void set(int age)
        {
            if (age % PlugIn.gl_param.SuccessionTimestep == 0)
                age = age / PlugIn.gl_param.SuccessionTimestep;
            else
                age = age / PlugIn.gl_param.SuccessionTimestep + 1;


            if (age < 1 || age > 320 / PlugIn.gl_param.SuccessionTimestep)
                throw new Exception("Illegal age");


            int temp1 = (age - 1) / 32;
            int temp2 = (age - 1) % 32;

            if (age == 64)
                Console.WriteLine("something happened");
            
            this[temp1] |= (int)mask[temp2];

        }

        public void reset(int age)
        {
            if (age % PlugIn.gl_param.SuccessionTimestep == 0)
                age = age / PlugIn.gl_param.SuccessionTimestep;
            else
                age = age / PlugIn.gl_param.SuccessionTimestep + 1;


            if (age < 1 || age > 320 / PlugIn.gl_param.SuccessionTimestep)
                throw new Exception("Illegal age");


            int temp1 = (age - 1) / 32;
            int temp2 = (age - 1) % 32;

            if (age == 64)
                Console.WriteLine("something happened");

            Remove(temp1);
        }

        public void Remove(int ag)
        {
            for (int i = 0; i < age.Count; ++i)
                if (age[i].Age == ag)
                {
                    age.RemoveAt(i);
                    return;
                }
        }

        public bool query()
        {
            return age.Count != 0;
        }

        public bool query(int age)
        {
            if (age % PlugIn.gl_param.SuccessionTimestep == 0)
                age = age / PlugIn.gl_param.SuccessionTimestep;
            else
                age = age / PlugIn.gl_param.SuccessionTimestep + 1;


            if (age < 1 || age > 320 / PlugIn.gl_param.SuccessionTimestep)
                throw new Exception("Illegal age");

            return this[age] != 0;
        }

        public int youngest()
        {
            if (age.Count > 0) return age[0].Age;
            else return 0;
        }

        public int oldest()
        {
            if (age.Count > 0) return age[age.Count - 1].Age;
            else return 0;
        }

        public void GrowTree()
        {
            age.Sort();
            for (int i = 0; i < age.Count; ++i)
                age[i].Num += 1;
            while (age[age.Count - 1].Age > spe.Longevity / PlugIn.gl_param.SuccessionTimestep)
                age.RemoveAt(age.Count - 1);
        }

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
            get { return treesFromVeg; }
            set { treesFromVeg = value; }
        }


        public uint AvailableSeed
        {
            get { return availableSeed; }
            set { availableSeed = value; }
        }


        public uint MatureTree
        {
            get { return matureTree; }
            set { matureTree = value; }
        }

        public MyCohorts(ISpecies spe)
        {
            vegPropagules = 0;
            disPropagules = 0;
            age.Clear();
            this.spe = spe;
        }

        //Clear all specie values.
        public new void clear()
        {
            vegPropagules = 0;
            disPropagules = 0;
            age.Clear();
        }

        int ISpeciesCohorts<ICohort>.Count
        {
            get
            {
                return age.Count;
            }
        }

        bool ISpeciesCohorts<ICohort>.IsMaturePresent
        {
            get
            {
                return age.Max(x => x.Age) > spe.Maturity;
            }
        }

        ISpecies ISpeciesCohorts<ICohort>.Species
        {
            get
            {
                return spe;
            }
        }

        IEnumerator<ICohort> IEnumerable<ICohort>.GetEnumerator()
        {
            //Console.Out.WriteLine("Itor 1");
            foreach (CohortData data in age)
                yield return new MyCohort(spe, data);
        }

        //---------------------------------------------------------------------

        IEnumerator IEnumerable.GetEnumerator()
        {
            //Console.Out.WriteLine("Itor 2");
            return ((IEnumerable<ICohort>)this).GetEnumerator();
        }

        private class MyCohort : ICohort
        {
            private CohortData data;
            private ISpecies spe;

            public MyCohort(ISpecies spe, CohortData data)
            {
                this.spe = spe;
                this.data = data;
            }

            public ISpecies Species
            {
                get
                {
                    return spe;
                }
            }
        }
    }
}
