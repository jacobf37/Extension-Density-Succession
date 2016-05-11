using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;


namespace Landis.Extension.Succession.Landispro
{
    public class reclass2
    {
        private static readonly int MAX_RECLASS = 50;

    	private static int[] maximum = new int [MAX_RECLASS];   //Maximum table.
		private static int[,] BOOL   = new int [MAX_RECLASS, MAX_RECLASS];  //Boolean table of class values.
		private static int numClasses;    //Number of output classes.


		//This will reset the reclassification system.
		private static void reset()
		{
            uint specAtnum = PlugIn.gl_spe_Attrs.NumAttrs;

			for (int i=1; i<=specAtnum; i++)
                maximum[i - 1] = (int)(PlugIn.gl_spe_Attrs[i].Longevity / (PlugIn.gl_spe_Attrs[i].ReclassCoef + 0.00000001f));
		}



		//This will read in a class description file given the file name
		//and the number of classes in the file (m).
		private void readInClassDescrip(StreamReader infile)
		{
            uint specAtnum = PlugIn.gl_spe_Attrs.NumAttrs;
            
            int i;

			for (i=0; i<MAX_RECLASS; i++)     /// if this is not done the reclass maps
			    for (int j=0; j<MAX_RECLASS; j++) /// do not get reclassified _CSH
			       BOOL[i, j]=0;


			i = 1;

			numClasses = 0;

			while (!system1.LDeof(infile))
			{
				string str = system1.LDfgets(infile);

				string[] words = str.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

				if(words.Length >= 1)
				{
					numClasses++;

					for(int k=1; k<words.Length; k++) //omit the first item: "0"
		            {
		                int bvalue;

						if (words[k][0] == '!')
						{
							bvalue = -1;

							words[k] = words[k].Substring(1);
						}
						else
						{
							bvalue = 1;
						}

						for (int j=1; j<=specAtnum; j++)
						{
                            if (PlugIn.gl_spe_Attrs[j].Name == words[k])
								BOOL[i, j] = bvalue;
						}//end for

		            }//end for

		            i++;
				}
	            

			}

		}





		//This will calculate the reclassification value given a specie list for a site.
		private int reclassificationValue(specie s)	
		{
			return s.number();
		}



		
		//This will reclassify a singular site.  M is the number of possible output classes.
		private int reclassifySite(site site_in, int m)
		{
			float[] sval = new float[MAX_RECLASS];

			specie local_specie = site_in.first();
			
			int j = 1;

			while (local_specie != null)
			{
			     float c = (float)local_specie.oldest() / maximum[j - 1];

			     if (c > 1.0)
			        c = 1.0f;

			     for (int i=1; i<=m; i++)
			     {
					if (BOOL[i, j] != 0)
				 	{						
						if(BOOL[i, j] > 0)
							sval[i] += c;
						else
							sval[i] -= c;


						if (sval[i] != 0)
						{
							if (sval[i] > 1.0) 
								sval[i] = 1.0f;

							if (sval[i] < 0.0) 
								sval[i] = 0.0f;
						}

				 	}

			     }

			     j++;

			     local_specie = site_in.next();
			}



			int mx = 0;

			float mxVal = 0.0f;

			for (int i=1; i<=m; i++)
			{
				if (sval[i] > mxVal)
			    {			    	
					mxVal = sval[i];

					mx = i;
			    }
			}			   


			if (mxVal > 0.0)
			    return mx;
			else
			    return m + 1;

		}




		//This will reclass the Landis map and return the result m.  The map will
		//be reclassified using a method defined in a an output reclassification text file.	
		public void fileReclass(ref map8 m, string fname)
		{
			int n = 0;

            uint snr = PlugIn.gl_sites.numRows;
            uint snc = PlugIn.gl_sites.numColumns;

			using(StreamReader infile = new StreamReader(fname))
			{				
				reset();				

				m.rename("Reclassification file: " + fname);				

				m.dim(snr,snc);

				m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
				m.assignLeg(map8.MaxValueforLegend - 2, "Water");
				m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");

				uint i = 1;
				
				while (!system1.LDeof(infile))
				{
                    string sub = system1.LDfgets(infile).Split()[0];

				 	m.assignLeg(i, sub);

				 	i++;
				 	n++;
				}

				m.assignLeg(i, "Other");

				for (uint j=i+1; j<map8.maxLeg-3; j++)
					m.assignLeg(j, "");
			}


			using(StreamReader infile = new StreamReader(fname))
			{
				readInClassDescrip(infile);

				for (uint i=snr; i>=1; i--)
				{
					for (uint j=1; j<=snc; j++)
					{
                        if (PlugIn.gl_sites.locateLanduPt(i, j).active())

                            m[i, j] = (ushort)(reclassifySite(PlugIn.gl_sites[i, j], n));

                        else if (PlugIn.gl_sites.locateLanduPt(i, j).lowland())

                            m[i, j] = (ushort)(map8.MaxValueforLegend - 3);

					 	else if (PlugIn.gl_sites.locateLanduPt(i,j).water())

                            m[i, j] = (ushort)(map8.MaxValueforLegend - 2);

					 	else

                            m[i, j] = (ushort)(map8.MaxValueforLegend - 1);

				   }

				}
			}

			
		}


    }
}
