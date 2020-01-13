using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;


namespace Landis.Extension.Succession.Landispro
{
    public static class reclass3
    {
    	//This will reclassify sites from user defined class file and the existing age
		//maps. Class file is a file containing site descriptions for a set of class.
		//Age maps involved in the reclassification need to be created before.
    	public static void reclassify(int timeStep, string[] ageMaps)
		{
            uint specAtNum = PlugIn.gl_spe_Attrs.NumAttrs;

            uint yDim = PlugIn.gl_sites.numRows;
            uint xDim = PlugIn.gl_sites.numColumns;

			
			for (int i=0; i<specAtNum; i++)
			{
				string str = ageMaps[i] + ".age";

				string speciesName;

				//read species name from ageIndex file
				using(StreamReader inAgeIndex = new StreamReader(str))
				{
					speciesName = system1.read_string(inAgeIndex);
				}



                int curSp = PlugIn.gl_spe_Attrs.current(speciesName);
			   

			   //read age map file from output directory
                str = PlugIn.gl_param.OutputDir + "/" + ageMaps[i] + timeStep.ToString() + ".gis";

               using (BinaryReader inAgeMap = new BinaryReader(File.Open(str, FileMode.Open)))
			   {
					byte[] dest = new byte[128];
			
					inAgeMap.Read(dest, 0, 128);


					// read inAgeMap
					for (uint k=yDim; k>0; k--)
					{
						for (uint j=1; j<=xDim; j++)
						{
							int coverType = inAgeMap.Read();

							if (coverType == 255)          //species absence
							{
                                specie s = PlugIn.gl_sites[k, j].current(curSp);

								s.clear();
							}
							else if (coverType >= 3) //0-empty 1-water 2-nonforest
							{
                                specie s = PlugIn.gl_sites[k, j].current(curSp);

								s.clear();

                                s.set((coverType - 2) * PlugIn.gl_sites.SuccessionTimeStep);
							}

						}//end for

					}//end for
			   }//end using

			}//end for

		}


    }
}
