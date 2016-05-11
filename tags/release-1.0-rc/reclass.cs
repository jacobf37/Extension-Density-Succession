using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Landis.Extension.Succession.Landispro
{
    public static class reclass
    {
        private static readonly uint snr = PlugIn.gl_sites.numRows;
        private static readonly uint snc = PlugIn.gl_sites.numColumns;
        
        private static readonly int time_step = PlugIn.gl_sites.SuccessionTimeStep;

        //This will perform a reclassification based on the underlying map land units.  
        //It will perform the reclassification on the global object succession.sites and place the results in the map parameter.		
        public static void luReclass(map8 m)
        {
            m.dim(snr, snc);

            m.rename("Landtype");


            uint lun = PlugIn.gl_landUnits.Num_Landunits;

            for (int k = 0; k < lun; k++)
                PlugIn.gl_landUnits[k].Index = k;                       

            for (uint i = snr; i >= 1; i--)
                for (uint j = 1; j <= snc; j++)
                    m[i, j] = (ushort)PlugIn.gl_sites.locateLanduPt(i, j).Index;


            if (lun < map8.maxLeg)
            {
                //temporary modify by Qia jan 26 2009 original i<lun    
                for (uint i = 0; i < lun; i++)
                    m.assignLeg(i, PlugIn.gl_landUnits[(int)i].Name);
            }
            else
            {
                //temporary modify by Qia jan 26 2009 original i<lun    
                for (uint i = 0; i < map8.maxLeg; i++)
                    m.assignLeg(i, PlugIn.gl_landUnits[(int)i].Name);
            }
        }




        //This will perform a reclassification based upon the oldest cohort upon a landis stand.
        //The cohorts will be scaled into 16 age classes.
        public static void ageReclass(map8 m)
        {
            m.dim(snr, snc);

            m.rename("Age class representation");

            for (uint j = 1; j < map8.MapmaxValue; j++)
                m.assignLeg(j, "");


            string str;
            //J.Yang hard coding changing itr*sites.TimeStep to itr
            //J.Yang maxLeg is defined as 256 in map8.h, therefore, maximum age cohorts it can output is 254 
            for (uint i = 1; i < map8.MaxValueforLegend - 4; i++)
            {
                str = string.Format("{0:   } - {1:   } yr", (i - 1) * time_step + 1, i * time_step);

                m.assignLeg(i, str);
            }


            m.assignLeg(0, "NoSpecies");

            m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
            m.assignLeg(map8.MaxValueforLegend - 2, "Water");
            m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");

            str = string.Format("	  >{0:   } yr", (map8.MaxValueforLegend - 4 - 1) * time_step);
            m.assignLeg(map8.MaxValueforLegend - 4, str);
            
            for (uint i = snr; i >= 1; i--)
            {
                for (uint j = 1; j <= snc; j++)
                {
                    if (PlugIn.gl_sites.locateLanduPt(i, j).active())
                    {

                        m[i, j] = 0;

                        uint myage = 0;

                        site local_site = PlugIn.gl_sites[i, j];

                        specie s = local_site.first();

                        while (s != null)
                        {
                            uint temp = s.oldest();

                            if (temp > myage)
                                myage = temp;

                            s = local_site.next();
                        }

                        m[i, j] = (ushort)(myage / time_step);

                    }

                    else if (PlugIn.gl_sites.locateLanduPt(i, j).lowland())

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 3);

                    else if (PlugIn.gl_sites.locateLanduPt(i, j).water())

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 2);

                    else

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 1);

                }

            }
        }






        public static void ageReclassYoungest(map8 m)
        {
            m.dim(snr, snc);


            m.rename("Age class representation");

            for (uint j = 1; j < map8.MapmaxValue; j++)
                m.assignLeg(j, "");


            string str;
            //J.Yang hard coding changing itr*sites.TimeStep to itr
            //J.Yang maxLeg is defined as 256 in map8.h, therefore, maximum age cohorts it can output is 254 
            for (uint i = 1; i < map8.MaxValueforLegend - 4; i++)
            {
                str = string.Format("{0:   } - {1:   } yr", (i - 1) * time_step + 1, i * time_step);

                m.assignLeg(i, str);
            }

            m.assignLeg(0, "NoSpecies");

            m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
            m.assignLeg(map8.MaxValueforLegend - 2, "Water");
            m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");

            str = string.Format("	  >{0:   } yr", (map8.MaxValueforLegend - 4 - 1) * time_step);
            m.assignLeg(map8.MaxValueforLegend - 4, str);
            
            for (uint i = snr; i >= 1; i--)
            {
                for (uint j = 1; j <= snc; j++)
                {
                    if (PlugIn.gl_sites.locateLanduPt(i, j).active())
                    {
                        m[i, j] = 0;

                        int myage = map8.MapmaxValue;

                        site local_site = PlugIn.gl_sites[i, j];

                        specie s = local_site.first();

                        while (s != null)
                        {
                            int temp = s.youngest();

                            if (temp < myage && s.youngest() > 0)
                                myage = temp;

                            s = local_site.next();
                        }

                        if (myage == map8.MapmaxValue)
                            myage = 0;
                        else
                            myage = myage / time_step;

                        m[i, j] = (ushort)myage;
                    }

                    else if (PlugIn.gl_sites.locateLanduPt(i, j).lowland())

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 3);

                    else if (PlugIn.gl_sites.locateLanduPt(i, j).water())

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 2);

                    else

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 1);

                }

            }
        }




        //This will faciliate age output at 10 year step for species specified in species age index file.	
        //This will output age at 10 year step for each specified species.
        //The cohorts can be up to 50 age classes, 0-500 years.	
        public static void speciesAgeMap(map8 m, string ageFile)
        {
            int curSp = PlugIn.gl_spe_Attrs.current(ageFile);

            m.dim(snr, snc);

            m.rename(ageFile);


            string str;
            for (uint i = 1; i < map8.maxLeg - 4; i++)
            {
                str = string.Format("{0:   } - {1:   } yr", (i - 1) * time_step + 1, i * time_step);

                m.assignLeg(i, str);
            }

            m.assignLeg(0, "NotPresent");

            m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
            m.assignLeg(map8.MaxValueforLegend - 2, "Water");
            m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");

            str = string.Format("	  >{0} yr", (map8.maxLeg - 4 - 1) * time_step);
            m.assignLeg(map8.MaxValueforLegend - 4, str);



            for (uint i = snr; i >= 1; i--)
            {
                for (uint j = 1; j <= snc; j++)
                {
                    if (PlugIn.gl_sites.locateLanduPt(i, j) == null)
                        throw new Exception("Invalid landunit error\n");


                    if (PlugIn.gl_sites.locateLanduPt(i, j).active())
                    {
                        m[i, j] = 0;       //where species not presents

                        if (PlugIn.gl_sites[i, j] == null)
                            throw new Exception("No site\n");

                        specie s = PlugIn.gl_sites[i, j].current(curSp);

                        if (s == null)
                        {
                            Console.WriteLine("{0}\n", curSp);

                            throw new Exception("No Species\n");
                        }

                        if (s.query())
                        {
                            m[i, j] = (ushort)(s.oldest() / time_step); //compare ageReclass which uses +3 there???

                            if (m[i, j] > map8.MaxValueforLegend - 4)   //maximum longevity is 640 years// Notice 66 means 640 years
                                m[i, j] = (ushort)(map8.MaxValueforLegend - 4);
                        }

                    }

                    else if (PlugIn.gl_sites.locateLanduPt(i, j).water())

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 2);

                    else if (PlugIn.gl_sites.locateLanduPt(i, j).lowland())

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 3);

                    else

                        m[i, j] = (ushort)(map8.MaxValueforLegend - 1);

                }

            }


        }


    }
}
