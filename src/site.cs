using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;


namespace Landis.Extension.Succession.Landispro
{
    public class site : species
    {
        private int numofsites;
        private int highestShadeTolerance;

        private float maxAge;
        private float rd;


        public int NumSites
        {
            get { return numofsites;  }
            set { numofsites = value; }
        }


        public int HighestShadeTolerance
        {
            get { return highestShadeTolerance;  }
            set { highestShadeTolerance = value; }
        }


        public float MaxAge
        {
            get { return maxAge;  }
            set { maxAge = value; }
        }


        public float RD
        {
            get { return rd;  }
            set { rd = value; }
        }


        //Constructor.
        public site(uint n)
            : base(n)
        {
            numofsites = 0;
        }


        //Constructor.
        public site()
            : base()
        {
            numofsites = 0;
        }



        //void write(FILE *outfile)
        public new void write(StreamWriter outfile)
        {
            base.write(outfile);
            outfile.WriteLine("\n"); 
        }



        public void copy(site s)
        {
        	if (s == null)
        		return;

            rd                    = s.rd;
            maxAge                = s.maxAge;	        
	        numofsites 			  = s.numofsites;
            highestShadeTolerance = s.highestShadeTolerance;

	        base.copy(s.all_species, site.numSpec);
        }


    }
}
