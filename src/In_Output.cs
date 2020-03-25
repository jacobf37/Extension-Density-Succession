
#define LANDISPRO_ONLY_SUCCESSION


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OSGeo.GDAL;
using OSGeo.OSR;
using Landis.Utilities;

//this file corresponds to IO.cpp in landis pro framewrok


namespace Landis.Extension.Succession.Density
{
    public class In_Output
    {
        private static int gDLLMode;
        private static string[] reMethods = new string[defines.MAX_RECLASS];
        
        private static int time_step;
        private static int pro0or401;
        private static uint species_num;
        private static string output_dir;

        private static int[] red = new int[map8.maxLeg];
        private static int[] green = new int[map8.maxLeg];
        private static int[] blue = new int[map8.maxLeg];

        private static int[] red2 = new int[map8.maxLeg];
        private static int[] green2 = new int[map8.maxLeg];
        private static int[] blue2 = new int[map8.maxLeg];

        private static int[] red3 = new int[map8.maxLeg];
        private static int[] green3 = new int[map8.maxLeg];
        private static int[] blue3 = new int[map8.maxLeg];

        private static int[] red4 = new int[map8.maxLeg];
        private static int[] green4 = new int[map8.maxLeg];
        private static int[] blue4 = new int[map8.maxLeg];

        public static int reclassMethods = 0;
        public static int numAgeMaps = 0;

        public static List<string> BioMassFileNames = new List<string>();
        public static List<string> BasalFileNames = new List<string>();
        public static List<string> TreesFileNames = new List<string>();
        public static List<string> IVFileNames = new List<string>();
        public static List<string> DBHFileNames = new List<string>();
        public static List<string> RDFileNames = new List<string>();
        public static List<string> SeedsFileNames = new List<string>();

        public static int flagoutputBiomass;
        public static int flagoutputBasal;
        public static int flagoutputTrees;
        public static int flagoutputIV;
        public static int flagoutputDBH;

        public static double[] AgeDistOutputBuffer_TPA = null;
        public static double[] AgeDistOutputBuffer_BA = null;

        public static double[] AgeDistOutputBuffer_TPA_landtype = null;
        public static double[] AgeDistOutputBuffer_BA_landtype = null;
        public static string mapCRS = null;
        public static double[] wAdfGeoTransform = new double[6];
        //private static object DataType;

        public static void Init_IO()
        {
            (new int[] { 0, 0, 100, 150, 200, 0, 0, 0, 150, 0, 150, 255, 80, 150, 255 }).CopyTo(red, 0);
            (new int[] { 0, 0, 0, 0, 0, 100, 150, 255, 0, 150, 150, 255, 80, 150, 255 }).CopyTo(green, 0);
            (new int[] { 0, 150, 0, 0, 0, 0, 0, 0, 150, 150, 0, 0, 80, 150, 255 }).CopyTo(blue, 0);

            (new int[] { 0, 70, 0, 0, 0, 0, 0, 0, 200, 100, 255, 150, 200, 200, 255, 255 }).CopyTo(red2, 0);
            (new int[] { 0, 70, 0, 0, 0, 150, 200, 255, 30, 200, 50, 50, 200, 0, 255 }).CopyTo(green2, 0);
            (new int[] { 0, 70, 125, 200, 255, 0, 100, 0, 30, 50, 50, 0, 0, 255 }).CopyTo(blue2, 0);

            (new int[] { 0, 70, 0, 0, 0, 0, 0, 0, 200, 100, 255, 150, 200, 200, 255, 255 }).CopyTo(red3, 0);
            (new int[] { 0, 70, 0, 0, 0, 150, 200, 255, 30, 200, 50, 50, 200, 0, 255 }).CopyTo(green3, 0);
            (new int[] { 0, 70, 125, 200, 255, 0, 100, 0, 30, 50, 50, 0, 0, 255 }).CopyTo(blue3, 0);

            (new int[] { 0, 70, 0, 0, 0, 0, 0, 0, 200, 100, 255, 150, 200, 200, 255, 255 }).CopyTo(red4, 0);
            (new int[] { 0, 70, 0, 0, 0, 150, 200, 255, 30, 200, 50, 50, 200, 0, 255 }).CopyTo(green4, 0);
            (new int[] { 0, 70, 125, 200, 255, 0, 100, 0, 30, 50, 50, 0, 0, 255 }).CopyTo(blue4, 0);

            time_step = PlugIn.gl_sites.SuccessionTimeStep;
            pro0or401 = PlugIn.gl_sites.Pro0or401;
            //species_num = PlugIn.gl_sites.SpecNum;
            species_num = PlugIn.gl_spe_Attrs.NumAttrs;
            output_dir = PlugIn.gl_param.OutputDir;
        }


        public static int GDLLMode { set { gDLLMode = value; } get { return gDLLMode; } }

        public static void AgeDistOutputBufferInitialize(uint SpecNum, uint LandTypeNum)
        {
            if (PlugIn.gl_sites.Flag_AgeDistStat == 0)
                return;

            long leng = SpecNum * 500 / time_step * 500 / time_step;

            AgeDistOutputBuffer_TPA = new double[leng];
            AgeDistOutputBuffer_BA = new double[leng];

            AgeDistOutputBuffer_TPA_landtype = new double[leng * LandTypeNum];
            AgeDistOutputBuffer_BA_landtype = new double[leng * LandTypeNum];

        }

        public static void AgeDistOutputBufferRelease()
        {
            if (PlugIn.gl_sites.Flag_AgeDistStat == 0)
                return;

            AgeDistOutputBuffer_BA = null;
            AgeDistOutputBuffer_TPA = null;
            AgeDistOutputBuffer_BA_landtype = null;
            AgeDistOutputBuffer_TPA_landtype = null;
        }

        public static void SetAgeDistoutputBuffer(int type, int specIndex, int ageIndex, int yearIndex, double value)
        {
            if (PlugIn.gl_sites.Flag_AgeDistStat == 0)
                return;

            int dimension = 500 / time_step;

            int index = (specIndex - 1) * dimension * dimension + (ageIndex - 1) * dimension + yearIndex - 1;

            switch (type)
            {
                case (int)enum1.BA: AgeDistOutputBuffer_BA[index] = value; break;
                case (int)enum1.TPA: AgeDistOutputBuffer_TPA[index] = value; break;
            }
        }


        public static double GetAgeDistoutputBuffer(int type, int specIndex, int ageIndex, int yearIndex)
        {
            int dimension = 500 / time_step;
            int index = (specIndex - 1) * dimension * dimension + (ageIndex - 1) * dimension + yearIndex - 1;

            switch (type)
            {
                case (int)enum1.BA: return AgeDistOutputBuffer_BA[index];
                case (int)enum1.TPA: return AgeDistOutputBuffer_TPA[index];
                default: throw new Exception("GetAgeDistoutputBuffer no choice");
            }
        }

        public static void SetAgeDistoutputBuffer_Landtype(int type, int specIndex, int ageIndex, int yearIndex, int landtypeIndex, double value)
        {
            if (PlugIn.gl_sites.Flag_AgeDistStat == 0)
                return;

            int dimension = 500 / time_step;

            int doubl_dim = dimension * dimension;

            long index = landtypeIndex * species_num * doubl_dim + (specIndex - 1) * doubl_dim + (ageIndex - 1) * dimension + yearIndex - 1;

            switch (type)
            {
                case (int)enum1.BA: AgeDistOutputBuffer_BA_landtype[index] = value; break;
                case (int)enum1.TPA: AgeDistOutputBuffer_TPA_landtype[index] = value; break;
            }
        }

        public static double GetAgeDistoutputBuffer_Landtype(int type, int specIndex, int ageIndex, int yearIndex, int landtypeIndex)
        {
            int dimension = 500 / time_step;

            int doubl_dim = dimension * dimension;

            long index = landtypeIndex * species_num * doubl_dim + (specIndex - 1) * doubl_dim + (ageIndex - 1) * dimension + yearIndex - 1;

            switch (type)
            {
                case (int)enum1.BA: return AgeDistOutputBuffer_BA_landtype[index];
                case (int)enum1.TPA: return AgeDistOutputBuffer_TPA_landtype[index];
                default: throw new Exception("GetAgeDistoutputBuffer_Landtype");
            }
        }



        //This function is to look up the sites with same species structure for future delete of redundant site
        //To make sure all sites are different from each other during run time
        public static void lookupredundant(int[] combineMatrix, int numCovers)
        {
            for (int i = 0; i < numCovers - 1; i++)
            {
                if (i == combineMatrix[i])
                {
                    site siteI = PlugIn.gl_sites.SortedIndex[i];

                    for (int j = i + 1; j < numCovers; j++)
                    {
                        site siteJ = PlugIn.gl_sites.SortedIndex[j];

                        if (j == combineMatrix[j])
                        {
                            int ifequal = PlugIn.gl_sites.SITE_compare(siteI, siteJ);

                            if (ifequal == 0)
                                combineMatrix[j] = i;
                        }

                    }

                }

            }

        }



        //after looking up, this function is to delete the redunt sites 
        public static void deleteRedundantInitial(int[] combineMatrix, int numCovers)
        {
            for (int i = numCovers - 1; i >= 0; i--)
            {
                if (i != combineMatrix[i])
                {
                    PlugIn.gl_sites.SortedIndex[i] = null;
                    PlugIn.gl_sites.SortedIndex.RemoveAt(i);
                }
            }
        }


        //This will convert HSV colors to RGB.
        public static void HSV_to_RGB(float h, float s, float v, ref int red, ref int green, ref int blue)
        {
            if (h == 360.0)
                h = 0.0f;

            h = h / 60.0f;

            int i = (int)h;

            float f = h - i;

            float p = v * (1 - s);

            float q = v * (1 - (s * f));

            float t = v * (1 - (s * (1 - f)));


            float r, g, b;

            switch (i)
            {
                case 0: r = v; g = t; b = p; break;

                case 1: r = q; g = v; b = p; break;

                case 2: r = p; g = v; b = t; break;

                case 3: r = p; g = q; b = v; break;

                case 4: r = t; g = p; b = v; break;

                case 5: r = v; g = p; b = q; break;

                default: throw new Exception("Exit from HSV_to_RGB\n");

            }

            red = (int)(r * 255.0);
            green = (int)(g * 255.0);
            blue = (int)(b * 255.0);

        }


        void ioInit() { Console.Write("Beginning Landis Run.\n"); }




        public static void outputFileheader(StreamWriter outfile)
        {
            outfile.Write("ncols  {0}\n", PlugIn.gl_sites.numColumns);

            outfile.Write("nrows  {0}\n", PlugIn.gl_sites.numRows);

            //outfile.Write("xllcorner  {0}\n", succession.gl_sites.xLLCorner - succession.gl_sites.getHeader()[30] / 2);
            //outfile.Write("yllcorner  {0}\n", succession.gl_sites.yLLCorner - succession.gl_sites.getHeader()[30] * succession.gl_sites.numRows + succession.gl_sites.getHeader()[30] / 2);
            //outfile.Write("cellsize  {0}\n", succession.gl_sites.getHeader()[30]);

            float header_elemt = PlugIn.gl_sites.Header_ele_to_float(30);

            outfile.Write("xllcorner  {0}\n", PlugIn.gl_sites.XLLCorner - header_elemt / 2);

            outfile.Write("yllcorner  {0}\n", PlugIn.gl_sites.YLLCorner - header_elemt * PlugIn.gl_sites.numRows + header_elemt / 2);

            outfile.Write("cellsize  {0}\n", header_elemt);

            outfile.Write("NODATA_value  -9999\n");
        }



        public static void initiateOutput_landis70Pro()
        {


            if (reMethods[0] != "N/A")
            {
                for (int i = 1; i <= species_num; ++i)
                {
                    BioMassFileNames.Add(PlugIn.gl_spe_Attrs[i].Name + "_Bio");
                }

                flagoutputBiomass = 1;
            }
            else
            {
                flagoutputBiomass = 0;
            }


            if (reMethods[1] != "N/A")
            {
                for (int i = 1; i <= species_num; ++i)
                    BasalFileNames.Add(PlugIn.gl_spe_Attrs[i].Name + "_BA");

                flagoutputBasal = 1;
            }
            else
            {
                flagoutputBasal = 0;
            }


            if (reMethods[2] != "N/A")
            {
                for (int i = 1; i <= species_num; ++i)
                    TreesFileNames.Add(PlugIn.gl_spe_Attrs[i].Name + "_TreeNum");

                flagoutputTrees = 1;
            }
            else
            {
                flagoutputTrees = 0;
            }

            for (int i = 1; i <= species_num; i++)
            {
                string specific_name = PlugIn.gl_spe_Attrs[i].Name;

                IVFileNames.Add(specific_name + "_IV");
                SeedsFileNames.Add(specific_name + "_AvailableSeed");
                RDFileNames.Add(specific_name + "_RelativeDensity");
                DBHFileNames.Add(specific_name + "_DBH");
            }

            flagoutputIV = 1;
            flagoutputDBH = 1;

            AgeDistOutputBufferInitialize(species_num, PlugIn.gl_landUnits.Num_Landunits);
        }



        public static void AgeDistOutputFromBufferToFile()
        {
            if (PlugIn.gl_sites.Flag_AgeDistStat == 0)
                return;

            double value = 0.0;

            int age1 = 0, age2 = 0, year = 0;

            string str = output_dir + "/BA_TPADist.txt";


            Console.Write("Output BA and TPA stat:\n");

            var output = new StreamWriter(new FileStream(str, FileMode.Create));

            for (int k = 1; k <= species_num; ++k)
            {
                output.Write("{0}:\n", PlugIn.gl_spe_Attrs[k].Name);
                output.Write("AgeRange\\Year: ");

                int end_year = PlugIn.gl_sites.GetAgeDistStat_YearCount(k - 1);

                for (int count_year = 1; count_year <= end_year; count_year++)
                {
                    PlugIn.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);
                    output.Write("TPA_{0} ", year);
                }

                for (int count_year = 1; count_year <= end_year; count_year++)
                {
                    PlugIn.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);
                    output.Write("BA_{0} ", year);
                }

                output.WriteLine();

                for (int count_age = 1; count_age <= PlugIn.gl_sites.GetAgeDistStat_AgeRangeCount(k - 1); count_age++)
                {
                    PlugIn.gl_sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);

                    output.Write("Age{0}-{1} ", age1, age2);

                    for (int count_year = 1; count_year <= end_year; count_year++)
                    {
                        value = GetAgeDistoutputBuffer((int)enum1.TPA, k, count_age, count_year);

                        output.Write("{0} ", value);
                    }

                    for (int count_year = 1; count_year <= end_year; count_year++)
                    {
                        value = GetAgeDistoutputBuffer((int)enum1.BA, k, count_age, count_year);

                        output.Write("{0} ", value);
                    }

                    output.Write("\n");
                }
            }



            output.Write("\n\n");

            for (int count_landtype = 0; count_landtype < PlugIn.gl_landUnits.Num_Landunits; count_landtype++)
            {
                output.Write("Landtype:{0}\n", count_landtype);

                for (int k = 1; k <= species_num; k++)
                {
                    output.Write("{0}:\n", PlugIn.gl_spe_Attrs[k].Name);

                    output.Write("AgeRange\\Year: ");

                    int end_year = PlugIn.gl_sites.GetAgeDistStat_YearCount(k - 1);

                    for (int count_year = 1; count_year <= end_year; count_year++)
                    {
                        PlugIn.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);

                        output.Write("TPA_{0} ", year);
                    }

                    for (int count_year = 1; count_year <= end_year; count_year++)
                    {
                        PlugIn.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);

                        output.Write("BA_{0} ", year);
                    }

                    output.Write("\n");

                    for (int count_age = 1; count_age <= PlugIn.gl_sites.GetAgeDistStat_AgeRangeCount(k - 1); count_age++)
                    {
                        PlugIn.gl_sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);

                        output.Write("Age{0}-{1} ", age1, age2);

                        for (int count_year = 1; count_year <= end_year; count_year++)
                        {
                            value = GetAgeDistoutputBuffer_Landtype((int)enum1.TPA, k, count_age, count_year, count_landtype);

                            output.Write("{0} ", value);
                        }

                        for (int count_year = 1; count_year <= end_year; count_year++)
                        {
                            value = GetAgeDistoutputBuffer_Landtype((int)enum1.BA, k, count_age, count_year, count_landtype);

                            output.Write("{0} ", value);
                        }

                        output.Write("\n");

                    }

                }

            }

            AgeDistOutputBufferRelease();

            output.Close();
        }




        public static void putOutput_AgeDistStat(int itr)
        {
            if (PlugIn.gl_sites.Flag_AgeDistStat == 0)
                return;

            int age1 = 0, age2 = 0, year = 0;

            //double TmpBasalAreaS = 0;
            double local_const = 3.1415926 / (4 * 10000.00);

            int itr_m_timestep = itr * time_step;

            for (int k = 1; k <= species_num; ++k)
            {
                int age_range = PlugIn.gl_sites.GetAgeDistStat_AgeRangeCount(k - 1);

                int end_year = PlugIn.gl_sites.GetAgeDistStat_YearCount(k - 1);

                int local_longevity = PlugIn.gl_spe_Attrs[k].Longevity;

                for (int count_age = 1; count_age <= age_range; count_age++)
                {
                    PlugIn.gl_sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);

                    int beg = Math.Min(local_longevity, age1) / time_step;
                    int end = Math.Min(local_longevity, age2) / time_step;

                    for (int count_year = 1; count_year <= end_year; count_year++)
                    {
                        PlugIn.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);

                        if (itr_m_timestep == year)
                        {
                            double TmpBasalAreaS = 0;
                            uint TmpTreesS = 0;

                            for (uint i = PlugIn.gl_sites.numRows; i > 0; --i)
                            {
                                for (uint j = 1; j <= PlugIn.gl_sites.numColumns; ++j)
                                {
                                    landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                                    specie local_specie = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                    for (int m = beg; m <= end; m++)
                                    {
                                        float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                        TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_specie.getTreeNum(m, k);

                                        TmpTreesS += local_specie.getTreeNum(m, k);
                                    }
                                }
                            }

                            SetAgeDistoutputBuffer((int)enum1.BA, k, count_age, count_year, TmpBasalAreaS);

                            SetAgeDistoutputBuffer((int)enum1.TPA, k, count_age, count_year, TmpTreesS);

                        }
                    }
                }


            }


            for (int count_landtype = 0; count_landtype < PlugIn.gl_landUnits.Num_Landunits; count_landtype++)
            {
                for (int k = 1; k <= species_num; k++)
                {
                    int age_range = PlugIn.gl_sites.GetAgeDistStat_AgeRangeCount(k - 1);

                    int end_year = PlugIn.gl_sites.GetAgeDistStat_YearCount(k - 1);

                    int local_longevity = PlugIn.gl_spe_Attrs[k].Longevity;

                    for (int count_age = 1; count_age <= age_range; count_age++)
                    {
                        PlugIn.gl_sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);

                        int beg = Math.Min(local_longevity, age1) / time_step;
                        int end = Math.Min(local_longevity, age2) / time_step;

                        for (int count_year = 1; count_year <= end_year; count_year++)
                        {
                            PlugIn.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);

                            if (itr_m_timestep == year)
                            {
                                double TmpBasalAreaS = 0;
                                uint TmpTreesS = 0;

                                for (uint i = PlugIn.gl_sites.numRows; i > 0; i--)
                                {
                                    for (uint j = 1; j <= PlugIn.gl_sites.numColumns; j++)
                                    {
                                        landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                                        specie local_specie = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                        if (PlugIn.gl_sites.locateLanduPt(i, j) == PlugIn.gl_landUnits[count_landtype])
                                        {
                                            for (int m = beg; m <= end; m++)
                                            {
                                                float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                                TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_specie.getTreeNum(m, k);

                                                TmpTreesS += local_specie.getTreeNum(m, k);
                                            }
                                        }
                                    }
                                }

                                SetAgeDistoutputBuffer_Landtype((int)enum1.BA, k, count_age, count_year, count_landtype, TmpBasalAreaS);

                                SetAgeDistoutputBuffer_Landtype((int)enum1.TPA, k, count_age, count_year, count_landtype, TmpTreesS);

                            }

                        }

                    }

                }

            }
        }



        public static void ageMapOutput(int itr)
        {
            DateTime a1 = DateTime.Now;

            Driver poDriver = Gdal.GetDriverByName("HFA");
            if (poDriver == null)
                throw new Exception("Age map output GDAL driver error!");

            string[] papszOptions = null;

            int[] pintScanline = null;

            int TmpTreeT = 0, TmpTreesS = 0;

            Dataset ageMapDS = null;

            Band ageMapBand = null;

            int col_num = (int)PlugIn.gl_sites.numColumns;
            int row_num = (int)PlugIn.gl_sites.numRows;
            int integerND = -999;
            Console.WriteLine("Age map output -- {0}, itr = {1}", a1, itr);

            string output_dir_string = output_dir + "/";
            string local_string = "_" + (itr * time_step).ToString() + ".img";

            for (int k = 1; k <= species_num; k++)
            {
                string outName = Path.Combine(output_dir_string, PlugIn.gl_spe_Attrs[k].Name, local_string);
                ageMapDS = poDriver.Create(outName, col_num, row_num, 1, DataType.GDT_Int16, papszOptions);
                ageMapDS.SetProjection(mapCRS);
                if (ageMapDS == null)
                    throw new Exception("Error creating age output IMG file");

                ageMapDS.SetGeoTransform(wAdfGeoTransform);

                ageMapBand = ageMapDS.GetRasterBand(1);
                ageMapBand.SetNoDataValue(integerND);

                int m_max = PlugIn.gl_spe_Attrs[k].Longevity / time_step;
                for (uint i = (uint)row_num; i > 0; --i)
                {
                    for (uint j = 1; j <= col_num; ++j)
                    {
                        if (!PlugIn.gl_sites.locateLanduPt(i, j).Active)
                        {
                            pintScanline[(row_num - i) * col_num + j - 1] = integerND;
                        }
                        else if (PlugIn.gl_sites.locateLanduPt(i, j).Active)
                        {
                            TmpTreesS = 0;

                            specie local_specis = PlugIn.gl_sites[i, j].SpecieIndex(k);

                            for (int m = 1; m <= m_max; m++)
                            {
                                TmpTreesS += (int)local_specis.getTreeNum(m, k);
                            }
                            pintScanline[(row_num - i) * col_num + j - 1] = TmpTreesS;
                        }
                    }
                }

                ageMapBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);


                if (ageMapDS != null)
                    ageMapDS.Dispose();
            }
        }


        public static void putOutput_Landis70Pro(int rep, int itr, int[] freq)
        {
            DateTime a1 = DateTime.Now;

            double TmpBiomassT = 0, TmpBiomassS = 0, TmpBasalAreaT = 0, TmpBasalAreaS = 0, TmpCarbon = 0, TmpCarbonTotal = 0, TmpRDTotal;

            float floatND = -999;

            int integerND = 65535;

            int TmpTreeT = 0, TmpTreesS = 0;

            Driver poDriver = Gdal.GetDriverByName("HFA");

            if (poDriver == null)
                throw new Exception("Start GDAL driver error!");

            string[] papszMetadata = poDriver.GetMetadata("");
            string[] papszOptions = null;

            

            float[] pafScanline = null;
            float[] pafScanline1 = null;
            float[] pafScanline2 = null;
            float[] pafScanline3 = null;
            float[] pafScanline4 = null;
            float[] pafScanline5 = null;

            int[] pintScanline = null;

            //VSIFree(pszSRS_WKT);

            Band outPoBand = null;
            Band outPoBand1 = null;
            Band outPoBand2 = null;
            Band outPoBand3 = null;
            Band outPoBand4 = null;
            Band outPoBand5 = null;

            Dataset poDstDS = null;
            Dataset poDstDS1 = null;
            Dataset poDstDS2 = null;
            Dataset poDstDS3 = null;
            Dataset poDstDS4 = null;
            Dataset poDstDS5 = null;

            Console.WriteLine("Start 7.0 Style writing output at {0}, itr = {1}", a1, itr);


            double local_const = 3.1415926 / (4 * 10000.00);

            string output_dir_string = output_dir + "/";
            string local_string = "_" + (itr * time_step).ToString() + ".img";
            int cellsize_square = PlugIn.gl_sites.CellSize * PlugIn.gl_sites.CellSize;


            int col_num = (int)PlugIn.gl_sites.numColumns;
            int row_num = (int)PlugIn.gl_sites.numRows;
            int total_size = col_num * row_num;

            pafScanline = new float[total_size];

            float Biomass_threshold = PlugIn.gl_sites.BiomassThreshold;
            for (int k = 1; k <= species_num; ++k)
            {
                int m_max = PlugIn.gl_spe_Attrs[k].Longevity / time_step;

                if (PlugIn.gl_sites.GetOutputGeneralFlagArray((uint)k - 1, (int)enum1.Bio) != 0)
                {
                    if (BioMassFileNames[k - 1] != "N/A")
                    {
                        string fpbiomass = (output_dir_string + BioMassFileNames[k - 1] + local_string);


                        poDstDS = poDriver.Create(fpbiomass, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                        poDstDS.SetProjection(mapCRS);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");

                        poDstDS.SetGeoTransform(wAdfGeoTransform);

                        outPoBand = poDstDS.GetRasterBand(1);
                        outPoBand.SetNoDataValue(floatND);

                        float biomass1 = PlugIn.gl_sites.GetBiomassData(PlugIn.gl_spe_Attrs[k].BioMassCoef, 1);
                        float biomass2 = PlugIn.gl_sites.GetBiomassData(PlugIn.gl_spe_Attrs[k].BioMassCoef, 2);


                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                landunit l = PlugIn.gl_sites.locateLanduPt(i, j);
                                
                                if (!l.Active)
                                {
                                    pafScanline[(row_num - i) * col_num + j - 1] = floatND;
                                }
                                else if (l.Active)
                                {
                                    TmpBiomassS = 0;

                                    specie local_specis = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                    for (int m = 1; m <= m_max; m++)
                                    {
                                        float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                        if (local_grow_rate >= Biomass_threshold)
                                            TmpBiomassS += Math.Exp(biomass1 + biomass2 * Math.Log(local_grow_rate)) * local_specis.getTreeNum(m, k) / 1000.00;
                                    }


                                    pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBiomassS;
                                }
                                //if (itr > 29 && TmpBiomassS != 0)
                                //    Console.Write("{0:F2} ", TmpBiomassS);
                            }

                        }

                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }

                if (PlugIn.gl_sites.GetOutputGeneralFlagArray((uint)k - 1, (int)enum1.BA) != 0)
                {
                    if (BasalFileNames[k - 1] != "N/A")
                    {
                        string fpbasal = output_dir_string + BasalFileNames[k - 1] + local_string;

                        poDstDS = poDriver.Create(fpbasal, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);//*
                        poDstDS.SetProjection(mapCRS);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");

                        poDstDS.SetGeoTransform(wAdfGeoTransform);

                        outPoBand = poDstDS.GetRasterBand(1);
                        outPoBand.SetNoDataValue(floatND);

                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                landunit l = PlugIn.gl_sites.locateLanduPt(i, j);
                                if (!l.Active)
                                {
                                    pafScanline[(row_num - i) * col_num + j - 1] = floatND;
                                }
                                else if (l.Active)
                                {
                                    TmpBasalAreaS = 0;

                                    specie local_specis = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                    for (int m = 1; m <= m_max; m++)
                                    {
                                        float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                        TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_specis.getTreeNum(m, k);
                                    }

                                    pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBasalAreaS;
                                }
                                //if (itr > 29 && TmpBasalAreaS != 0)
                                //    Console.Write("{0:F1} ", TmpBasalAreaS);
                            }
                        }


                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);//*

                        if (poDstDS != null)
                            poDstDS.Dispose();

                    }
                }

                pintScanline = new int[total_size];

                if (PlugIn.gl_sites.GetOutputGeneralFlagArray((uint)k - 1, (int)enum1.TPA) != 0)
                {
                    if (TreesFileNames[k - 1] != "N/A")
                    {
                        string fptree = output_dir_string + TreesFileNames[k - 1] + local_string;

                        poDstDS = poDriver.Create(fptree, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_UInt16, papszOptions);//*
                        poDstDS.SetProjection(mapCRS);
                        poDstDS.SetGeoTransform(wAdfGeoTransform);

                        outPoBand = poDstDS.GetRasterBand(1);
                        outPoBand.SetNoDataValue(integerND);
                        
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");


                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                if (!PlugIn.gl_sites.locateLanduPt(i, j).Active)
                                {
                                    pintScanline[(row_num - i) * col_num + j - 1] = integerND;
                                }
                                else if (PlugIn.gl_sites.locateLanduPt(i, j).Active)
                                {
                                    TmpTreesS = 0;

                                    specie local_specis = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                    for (int m = 1; m <= m_max; m++)
                                    {
                                        TmpTreesS += (int)local_specis.getTreeNum(m, k);
                                    }
                                    //if (itr > 9 && TmpTreesS != 0)
                                    //    Console.Write("{0}\n", TmpTreesS);
                                    pintScanline[(row_num - i) * col_num + j - 1] = TmpTreesS;
                                }
                            }
                        }

                        outPoBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);


                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }

                if (PlugIn.gl_sites.GetOutputGeneralFlagArray((uint)k - 1, (int)enum1.IV) != 0)
                {
                    if (IVFileNames[k - 1] != "N/A")
                    {
                        string fpIV = output_dir_string + IVFileNames[k - 1] + local_string;

                        poDstDS = poDriver.Create(fpIV, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);//*
                        poDstDS.SetProjection(mapCRS);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");

                        poDstDS.SetGeoTransform(wAdfGeoTransform);

                        outPoBand = poDstDS.GetRasterBand(1);
                        outPoBand.SetNoDataValue(floatND);

                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                                if (!l.Active)
                                {
                                    pafScanline[(row_num - i) * col_num + j - 1] = floatND;
                                }
                                else if (l.Active)
                                {
                                    TmpBasalAreaT = 0;

                                    TmpTreeT = 0;

                                    for (int kk = 1; kk <= species_num; ++kk)
                                    {
                                        if (PlugIn.gl_spe_Attrs[kk].SpType >= 0)
                                        {
                                            specie local_specis = PlugIn.gl_sites[i, j].SpecieIndex(kk);

                                            int local_m_max = PlugIn.gl_spe_Attrs[kk].Longevity / time_step;

                                            for (int m = 1; m <= local_m_max; m++)
                                            {
                                                float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(kk, m, l.LtID);

                                                uint local_tree_num = local_specis.getTreeNum(m, kk);

                                                TmpBasalAreaT += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                                TmpTreeT += (int)local_tree_num;
                                            }
                                        }
                                    }

                                    TmpBasalAreaS = 0;

                                    specie local_spe = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                    for (int m = 1; m <= m_max; ++m)
                                    {
                                        float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                        TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_spe.getTreeNum(m, k);
                                    }

                                    if (TmpTreeT == 0 || TmpBasalAreaT < 0.0001)
                                        pafScanline[(row_num - i) * col_num + j - 1] = 0;
                                    else
                                        pafScanline[(row_num - i) * col_num + j - 1] = (float)(TmpTreesS / (double)TmpTreeT + TmpBasalAreaS / TmpBasalAreaT);
                                }
                            }
                        }

                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }



                if (PlugIn.gl_sites.GetOutputGeneralFlagArray((uint)k - 1, (int)enum1.Seeds) != 0)
                {
                    if (SeedsFileNames[k - 1] != "N/A")
                    {
                        string fpSeeds = output_dir_string + SeedsFileNames[k - 1] + local_string;//* change

                        poDstDS = poDriver.Create(fpSeeds, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_UInt16, papszOptions);
                        poDstDS.SetProjection(mapCRS);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");

                        outPoBand = poDstDS.GetRasterBand(1);
                        outPoBand.SetNoDataValue(integerND);

                        poDstDS.SetGeoTransform(wAdfGeoTransform);

                        for (uint i = (uint)row_num; i > 0; --i)
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                if (!PlugIn.gl_sites.locateLanduPt(i, j).Active)
                                {
                                    pintScanline[(row_num - i) * col_num + j - 1] = integerND;
                                }
                                else if (PlugIn.gl_sites.locateLanduPt(i, j).Active)
                                {
                                    pintScanline[(row_num - i) * col_num + j - 1] = (int)PlugIn.gl_sites[i, j].SpecieIndex(k).AvailableSeed;
                                }
                            }


                        outPoBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                        if (poDstDS != null)
                            poDstDS.Dispose();

                        //Console.ReadLine();
                    }
                }



                if (PlugIn.gl_sites.GetOutputGeneralFlagArray((uint)k - 1, (int)enum1.RDensity) != 0)
                {
                    if (RDFileNames[k - 1] != "N/A")
                    {
                        string fpRD = output_dir_string + RDFileNames[k - 1] + local_string;

                        poDstDS = poDriver.Create(fpRD, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);//*
                        poDstDS.SetProjection(mapCRS);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");

                        outPoBand = poDstDS.GetRasterBand(1);
                        outPoBand.SetNoDataValue(floatND);
                        poDstDS.SetGeoTransform(wAdfGeoTransform);

                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                landunit l = PlugIn.gl_sites.locateLanduPt(i, j);
                                if (!l.Active)
                                {
                                    pafScanline[(row_num - i) * col_num + j - 1] = floatND;
                                }
                                else if (l.Active)
                                {
                                    if (PlugIn.gl_sites[i, j].specAtt(k).SpType >= 0)
                                    {
                                        float temp = 0.0f;

                                        site local_site = PlugIn.gl_sites[i, j];

                                        speciesattr local_speciesattr = local_site.specAtt(k);
                                        specie local_species = local_site.SpecieIndex(k);

                                        int loop_num = local_speciesattr.Longevity / time_step;

                                        for (int jj = 1; jj <= loop_num; jj++)
                                        {
                                            temp += (float)Math.Pow((PlugIn.gl_sites.GetGrowthRates(k, jj, l.LtID) / 25.4), 1.605) * local_species.getTreeNum(jj, k);
                                        }

                                        temp *= local_speciesattr.MaxAreaOfSTDTree / cellsize_square;

                                        pafScanline[(row_num - i) * col_num + j - 1] = temp;
                                    }
                                    else
                                        pafScanline[(row_num - i) * col_num + j - 1] = 0.0f;
                                }
                            }
                        }

                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }

            }//end if


            int Bio_flag = PlugIn.gl_sites.GetOutputGeneralFlagArray(species_num, (int)enum1.Bio);
            int Car_flag = PlugIn.gl_sites.GetOutputGeneralFlagArray(species_num, (int)enum1.Car);
            int BA_flag = PlugIn.gl_sites.GetOutputGeneralFlagArray(species_num, (int)enum1.BA);
            int RDenflag = PlugIn.gl_sites.GetOutputGeneralFlagArray(species_num, (int)enum1.RDensity);
            int TPA_flag = PlugIn.gl_sites.GetOutputGeneralFlagArray(species_num, (int)enum1.TPA);

            if (Bio_flag != 0 || Car_flag != 0 || BA_flag != 0 || RDenflag != 0 || TPA_flag != 0)
            {
                //below is for total 

                //1.Bio
                pafScanline1 = new float[total_size];
                pafScanline2 = new float[total_size];
                pafScanline3 = new float[total_size];
                pafScanline4 = new float[total_size];
                pafScanline5 = new float[total_size];
                pintScanline = new int[total_size];

                if (Bio_flag != 0)
                {
                    string fpTotalBiomass = output_dir_string + "TotalBio" + local_string;

                    poDstDS1 = poDriver.Create(fpTotalBiomass, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS1.SetProjection(mapCRS);
                    if (poDstDS1 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand1 = poDstDS1.GetRasterBand(1);

                    poDstDS1.SetGeoTransform(wAdfGeoTransform);
                }


                if (Car_flag != 0)
                {
                    string fpTotalcarbon = output_dir_string + "TotalCarbon" + local_string;

                    poDstDS2 = poDriver.Create(fpTotalcarbon, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS2.SetProjection(mapCRS);
                    if (poDstDS2 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand2 = poDstDS2.GetRasterBand(1);

                    poDstDS2.SetGeoTransform(wAdfGeoTransform);
                }


                if (BA_flag != 0)
                {
                    string fptotalbasl = output_dir_string + "TotalBA" + local_string;

                    poDstDS3 = poDriver.Create(fptotalbasl, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS3.SetProjection(mapCRS);
                    if (poDstDS3 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand3 = poDstDS3.GetRasterBand(1);

                    poDstDS3.SetGeoTransform(wAdfGeoTransform);
                }


                if (TPA_flag != 0)
                {
                    string fptotaltrees = output_dir_string + "TotalTrees" + local_string;

                    poDstDS4 = poDriver.Create(fptotaltrees, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_UInt32, papszOptions);
                    poDstDS4.SetProjection(mapCRS);
                    if (poDstDS4 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand4 = poDstDS4.GetRasterBand(1);

                    poDstDS4.SetGeoTransform(wAdfGeoTransform);
                }



                if (RDenflag != 0)
                {
                    string fpRD = output_dir_string + "RelativeDensity" + local_string;

                    poDstDS5 = poDriver.Create(fpRD, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS5.SetProjection(mapCRS);
                    if (poDstDS5 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand5 = poDstDS5.GetRasterBand(1);

                    poDstDS5.SetGeoTransform(wAdfGeoTransform);
                }




                for (uint i = (uint)row_num; i > 0; i--)
                {
                    for (uint j = 1; j <= col_num; j++)
                    {
                        landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                        TmpBiomassT = 0;

                        TmpBasalAreaT = 0;

                        TmpTreeT = 0;

                        TmpCarbon = 0;

                        TmpCarbonTotal = 0;

                        site local_site = PlugIn.gl_sites[i, j];

                        for (int k = 1; k <= species_num; k++)
                        {
                            TmpCarbon = 0;

                            speciesattr local_speciesattr = PlugIn.gl_spe_Attrs[k];

                            if (local_speciesattr.SpType >= 0)
                            {
                                int m_max = local_speciesattr.Longevity / time_step;

                                float tmp_term1 = PlugIn.gl_sites.GetBiomassData(local_speciesattr.BioMassCoef, 1);
                                float tmp_term2 = PlugIn.gl_sites.GetBiomassData(local_speciesattr.BioMassCoef, 2);

                                specie local_specie = local_site.SpecieIndex(k);

                                for (int m = 1; m <= m_max; m++)
                                {
                                    float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);
                                    double local_value = Math.Exp(tmp_term1 + tmp_term2 * Math.Log(local_grow_rate));

                                    uint local_tree_num = local_specie.getTreeNum(m, k);

                                    double tmp_mulply = local_value * local_tree_num;

                                    if (local_grow_rate >= Biomass_threshold && Bio_flag != 0)
                                        TmpBiomassT += tmp_mulply / 1000.00;

                                    if (BA_flag != 0)
                                        TmpBasalAreaT += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                    if (TPA_flag != 0)
                                        TmpTreeT += (int)local_tree_num;

                                    if (Car_flag != 0)
                                        TmpCarbon += tmp_mulply;

                                }

                            }

                            if (Car_flag != 0)
                                TmpCarbonTotal += TmpCarbon * local_speciesattr.CarbonCoef;

                        }

                        long local_index = (row_num - i) * col_num + j - 1;

                        if (Bio_flag != 0)
                            pafScanline1[local_index] = (float)TmpBiomassT;

                        if (Car_flag != 0)
                            pafScanline2[local_index] = (float)TmpCarbonTotal / 1000.00f;

                        if (BA_flag != 0)
                            pafScanline3[local_index] = (float)TmpBasalAreaT;

                        if (RDenflag != 0)
                            pafScanline5[local_index] = local_site.RD;

                        if (TPA_flag != 0)
                            pintScanline[local_index] = TmpTreeT;
                    }

                }


                if (Bio_flag != 0)
                {
                    outPoBand1.WriteRaster(0, 0, col_num, row_num, pafScanline1, col_num, row_num, 0, 0);

                    if (poDstDS1 != null)
                        poDstDS1.Dispose();
                }


                if (Car_flag != 0)
                {
                    outPoBand2.WriteRaster(0, 0, col_num, row_num, pafScanline2, col_num, row_num, 0, 0);

                    if (poDstDS2 != null)
                        poDstDS2.Dispose();
                }

                if (BA_flag != 0)
                {
                    outPoBand3.WriteRaster(0, 0, col_num, row_num, pafScanline3, col_num, row_num, 0, 0);

                    if (poDstDS3 != null)
                        poDstDS3.Dispose();
                }

                if (TPA_flag != 0)
                {
                    outPoBand4.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                    //if (itr > 29)
                    //{
                    //    for (int i = 0; i < total_size; i++)
                    //    {
                    //        if (pintScanline[i] != 0)
                    //            Console.Write("{0} ", pintScanline[i]);
                    //    }
                    //}

                    if (poDstDS4 != null)
                        poDstDS4.Dispose();
                }

                if (RDenflag != 0)
                {
                    outPoBand5.WriteRaster(0, 0, col_num, row_num, pafScanline5, col_num, row_num, 0, 0);

                    if (poDstDS5 != null)
                        poDstDS5.Dispose();
                }
            }



            //below is for species age range
            int Agerangeage1 = 0, Agerangeage2 = 0, Agerangecount = 0, Agerangeii = 0;

            if (PlugIn.gl_sites.FlagAgeRangeOutput != 0)
            {
                for (int k = 1; k <= species_num; k++)
                {
                    int gl_spe_spe_attr_longevity = PlugIn.gl_spe_Attrs[k].Longevity;

                    if (PlugIn.gl_sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)enum1.Bio) != 0)
                    {
                        Agerangecount = PlugIn.gl_sites.GetAgerangeCount(k - 1);


                        float tmp_term1 = PlugIn.gl_sites.GetBiomassData(PlugIn.gl_spe_Attrs[k].BioMassCoef, 1);
                        float tmp_term2 = PlugIn.gl_sites.GetBiomassData(PlugIn.gl_spe_Attrs[k].BioMassCoef, 2);


                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            PlugIn.gl_sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (BioMassFileNames[k - 1] != "N/A")
                            {
                                string fpbiomass = output_dir_string + BioMassFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;

                                poDstDS = poDriver.Create(fpbiomass, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);//*
                                poDstDS.SetProjection(mapCRS);
                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                int beg = Math.Min(gl_spe_spe_attr_longevity, Agerangeage1) / time_step;
                                int end = Math.Min(gl_spe_spe_attr_longevity, Agerangeage2) / time_step;

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                                        TmpBiomassS = 0;

                                        specie local_specie = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                        for (int m = beg; m <= end; m++)
                                        {
                                            float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);
                                            double local_value = Math.Exp(tmp_term1 + tmp_term2 * Math.Log(local_grow_rate));

                                            if (local_grow_rate >= Biomass_threshold)
                                                TmpBiomassS += local_value * local_specie.getTreeNum(m, k);
                                        }

                                        TmpBiomassS /= 1000.00;

                                        pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBiomassS;
                                    }

                                }

                                outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();

                            }

                        }

                    }



                    if (PlugIn.gl_sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)enum1.BA) != 0)
                    {
                        Agerangecount = PlugIn.gl_sites.GetAgerangeCount(k - 1);

                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            PlugIn.gl_sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (BasalFileNames[k - 1] != "N/A")
                            {
                                string fpbasal = output_dir_string + BasalFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;

                                poDstDS = poDriver.Create(fpbasal, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                                poDstDS.SetProjection(mapCRS);
                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                int beg = Math.Min(gl_spe_spe_attr_longevity, Agerangeage1) / time_step;
                                int end = Math.Min(gl_spe_spe_attr_longevity, Agerangeage2) / time_step;

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                                        TmpBasalAreaS = 0;

                                        specie local_specie = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                        for (int m = beg; m <= end; m++)
                                        {
                                            float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                            TmpBasalAreaS += local_grow_rate * local_grow_rate * local_specie.getTreeNum(m, k);
                                        }

                                        TmpBasalAreaS *= local_const;

                                        pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBasalAreaS;
                                    }
                                }

                                outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();
                            }

                        }

                    }


                    if (PlugIn.gl_sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)enum1.TPA) != 0)
                    {
                        Agerangecount = PlugIn.gl_sites.GetAgerangeCount(k - 1);

                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            PlugIn.gl_sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (TreesFileNames[k - 1] != "N/A")
                            {
                                string fptree = output_dir_string + TreesFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;

                                poDstDS = poDriver.Create(fptree, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_UInt32, papszOptions);
                                poDstDS.SetProjection(mapCRS);
                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                int beg = Math.Min(gl_spe_spe_attr_longevity, Agerangeage1) / time_step;
                                int end = Math.Min(gl_spe_spe_attr_longevity, Agerangeage2) / time_step;

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        TmpTreesS = 0;

                                        specie local_specie = PlugIn.gl_sites[i, j].SpecieIndex(k);

                                        for (int m = beg; m <= end; m++)
                                            TmpTreesS += (int)local_specie.getTreeNum(m, k);

                                        pafScanline[(row_num - i) * col_num + j - 1] = TmpTreesS;

                                        //if (TmpTreesS != 0)
                                        //    Console.Write("{0} ", TmpTreesS);
                                    }

                                }


                                outPoBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();
                            }

                        }

                    }



                    if (PlugIn.gl_sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)enum1.IV) != 0)
                    {

                        Agerangecount = PlugIn.gl_sites.GetAgerangeCount(k - 1);

                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            PlugIn.gl_sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (IVFileNames[k - 1] != "N/A")
                            {
                                string fpIV = output_dir_string + IVFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;
                                poDstDS = poDriver.Create(fpIV, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                                poDstDS.SetProjection(mapCRS);
                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        landunit l = PlugIn.gl_sites.locateLanduPt(i, j);
                                        TmpBasalAreaT = 0;

                                        TmpTreeT = 0;

                                        site local_site = PlugIn.gl_sites[i, j];

                                        for (int kk = 1; kk <= species_num; kk++)
                                        {
                                            specie local_specie = local_site.SpecieIndex(kk);

                                            if (PlugIn.gl_spe_Attrs[kk].SpType >= 0)
                                            {
                                                int loop_num = PlugIn.gl_spe_Attrs[kk].Longevity / time_step;

                                                for (int m = 1; m <= loop_num; m++)
                                                {
                                                    float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(kk, m, l.LtID);
                                                    uint local_tree_num = local_specie.getTreeNum(m, kk);

                                                    TmpBasalAreaT += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                                    TmpTreeT += (int)local_tree_num;
                                                }
                                            }
                                        }

                                        int local_loop_num = gl_spe_spe_attr_longevity / time_step;

                                        TmpBasalAreaS = 0;
                                        TmpTreesS = 0;

                                        specie tmp_local_specie = PlugIn.gl_sites[i, j].SpecieIndex(k);


                                        for (int m = 1; m <= local_loop_num; m++)
                                        {
                                            float local_grow_rate = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);

                                            uint local_tree_num = tmp_local_specie.getTreeNum(m, k);

                                            TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                            TmpTreesS += (int)local_tree_num;
                                        }


                                        if (TmpTreeT == 0 || TmpBasalAreaT < 0.0001)
                                            pafScanline[(row_num - i) * col_num + j - 1] = 0;
                                        else
                                            pafScanline[(row_num - i) * col_num + j - 1] = (float)(TmpTreesS / (double)TmpTreeT + TmpBasalAreaS / TmpBasalAreaT);
                                    }

                                }
                                //fpIV.Close();
                                outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();
                            }

                        }

                    }



                }


                //below is for age range  total 
                int Bio_endagerange_flag = PlugIn.gl_sites.GetOutputAgerangeFlagArray(species_num, (int)enum1.Bio);
                int Car_endagerange_flag = PlugIn.gl_sites.GetOutputAgerangeFlagArray(species_num, (int)enum1.Car);
                int BA_endagerange_flag = PlugIn.gl_sites.GetOutputAgerangeFlagArray(species_num, (int)enum1.BA);
                int TPA_endagerange_flag = PlugIn.gl_sites.GetOutputAgerangeFlagArray(species_num, (int)enum1.TPA);
                int RDenendagerange_flag = PlugIn.gl_sites.GetOutputAgerangeFlagArray(species_num, (int)enum1.RDensity);

                if (Bio_endagerange_flag != 0)
                {
                    string fpTotalBiomass = output_dir_string + "TotalBio_AgeRange" + local_string;

                    poDstDS1 = poDriver.Create(fpTotalBiomass, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS1.SetProjection(mapCRS);
                    if (poDstDS1 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand1 = poDstDS1.GetRasterBand(1);

                    poDstDS1.SetGeoTransform(wAdfGeoTransform);
                }


                if (TPA_endagerange_flag != 0)
                {
                    string fpTotaltrees = output_dir_string + "TotalTrees_AgeRange" + local_string;
                    poDstDS2 = poDriver.Create(fpTotaltrees, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_UInt32, papszOptions);
                    poDstDS2.SetProjection(mapCRS);
                    if (poDstDS2 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand2 = poDstDS2.GetRasterBand(1);

                    poDstDS2.SetGeoTransform(wAdfGeoTransform);
                }


                if (BA_endagerange_flag != 0)
                {
                    string fpTotalbasl = output_dir_string + "TotalBasal_AgeRange" + local_string;
                    poDstDS3 = poDriver.Create(fpTotalbasl, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS3.SetProjection(mapCRS);
                    if (poDstDS3 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand3 = poDstDS3.GetRasterBand(1);

                    poDstDS3.SetGeoTransform(wAdfGeoTransform);
                }



                if (Car_endagerange_flag != 0)
                {
                    string fpTotalcarbon = output_dir_string + "TotalCarbon_AgeRange" + local_string;

                    poDstDS4 = poDriver.Create(fpTotalcarbon, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);
                    poDstDS4.SetProjection(mapCRS);
                    if (poDstDS4 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand4 = poDstDS4.GetRasterBand(1);

                    poDstDS4.SetGeoTransform(wAdfGeoTransform);
                }



                if (RDenendagerange_flag != 0)
                {
                    string fpTotalRD = output_dir_string + "TotalRD_AgeRange" + local_string;

                    poDstDS5 = poDriver.Create(fpTotalRD, col_num, row_num, 1, OSGeo.GDAL.DataType.GDT_Float32, papszOptions);

                    if (poDstDS5 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand5 = poDstDS5.GetRasterBand(1);

                    poDstDS5.SetGeoTransform(wAdfGeoTransform);
                }



                for (uint i = (uint)row_num; i > 0; i--)
                {
                    for (uint j = 1; j <= col_num; j++)
                    {
                        landunit l = PlugIn.gl_sites.locateLanduPt(i, j);

                        TmpBiomassT = 0;

                        TmpBasalAreaT = 0;

                        TmpTreeT = 0;

                        TmpCarbon = 0;

                        TmpCarbonTotal = 0;

                        TmpRDTotal = 0;

                        site local_site = PlugIn.gl_sites[i, j];

                        for (int k = 1; k <= species_num; k++)
                        {
                            TmpCarbon = 0;

                            Agerangecount = PlugIn.gl_sites.GetAgerangeCount(k - 1);

                            speciesattr local_speciesattr = local_site.specAtt(k);

                            speciesattr gl_spe_Attrs_k = PlugIn.gl_spe_Attrs[k];

                            float tmp_term1 = PlugIn.gl_sites.GetBiomassData(gl_spe_Attrs_k.BioMassCoef, 1);
                            float tmp_term2 = PlugIn.gl_sites.GetBiomassData(gl_spe_Attrs_k.BioMassCoef, 2);


                            for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                            {
                                PlugIn.gl_sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                                if (gl_spe_Attrs_k.SpType >= 0)
                                {
                                    int beg = Math.Min(gl_spe_Attrs_k.Longevity, Agerangeage1) / time_step;
                                    int end = Math.Min(gl_spe_Attrs_k.Longevity, Agerangeage2) / time_step;

                                    for (int m = beg; m <= end; m++)
                                    {
                                        float local_term1 = PlugIn.gl_sites.GetGrowthRates(k, m, l.LtID);
                                        uint local_term2 = PlugIn.gl_sites[i, j].SpecieIndex(k).getTreeNum(m, k);

                                        double combination1 = Math.Exp(tmp_term1 + tmp_term2 * Math.Log(local_term1)) * local_term2;

                                        if (local_term1 >= Biomass_threshold)
                                            TmpBiomassT += combination1 / 1000.00;

                                        TmpBasalAreaT += local_term1 * local_term1 * local_const * local_term2;

                                        TmpTreeT += (int)local_term2;

                                        TmpCarbon += combination1;

                                        TmpRDTotal += Math.Pow((local_term1 / 25.4), 1.605) * local_term2;
                                    }

                                    TmpRDTotal *= local_speciesattr.MaxAreaOfSTDTree / cellsize_square;

                                }

                                TmpCarbonTotal += TmpCarbon * gl_spe_Attrs_k.CarbonCoef;

                            }


                        }

                        long local_index = (row_num - i) * col_num + j - 1;

                        if (Bio_endagerange_flag != 0)
                            pafScanline1[local_index] = (float)TmpBiomassT;

                        if (Car_endagerange_flag != 0)
                            pafScanline4[local_index] = (float)TmpCarbonTotal / 1000.00f;

                        if (BA_endagerange_flag != 0)
                            pafScanline3[local_index] = (float)TmpBasalAreaT;

                        if (RDenendagerange_flag != 0)
                            pafScanline5[local_index] = (float)TmpRDTotal;

                        if (TPA_endagerange_flag != 0)
                            pintScanline[local_index] = TmpTreeT;

                    }

                }



                if (Bio_endagerange_flag != 0)
                {
                    outPoBand1.WriteRaster(0, 0, col_num, row_num, pafScanline1, col_num, row_num, 0, 0);

                    if (poDstDS1 != null)
                        poDstDS1.Dispose();
                }

                if (Car_endagerange_flag != 0)
                {
                    outPoBand4.WriteRaster(0, 0, col_num, row_num, pafScanline4, col_num, row_num, 0, 0);

                    if (poDstDS4 != null)
                        poDstDS4.Dispose();
                }

                if (BA_endagerange_flag != 0)
                {
                    outPoBand3.WriteRaster(0, 0, col_num, row_num, pafScanline3, col_num, row_num, 0, 0);

                    if (poDstDS3 != null)
                        poDstDS3.Dispose();
                }

                if (TPA_endagerange_flag != 0)
                {
                    outPoBand2.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                    if (poDstDS2 != null)
                        poDstDS2.Dispose();
                }

                if (RDenendagerange_flag != 0)
                {
                    outPoBand5.WriteRaster(0, 0, col_num, row_num, pafScanline5, col_num, row_num, 0, 0);

                    if (poDstDS5 != null)
                        poDstDS5.Dispose();
                }

            }


            pafScanline = null;
            pafScanline1 = null;
            pafScanline2 = null;
            pafScanline3 = null;
            pafScanline4 = null;
            pafScanline5 = null;


            papszOptions = null;
            pintScanline = null;

            DateTime a2 = DateTime.Now;
            Console.WriteLine("Finish 7.0 Style writing output at {0}", a2);

            Console.WriteLine("it took {0} ", a2 - a1);

        }




        //This will write out a Landis description for this iteration and replicate.
        public static void putOutput(int rep, int itr, int[] freq)
        {
            //string local_string = (itr * freq[4] * succession.gl_sites.TimeStep).ToString();
            string local_string = (itr * time_step).ToString();

            map8 m = new map8(PlugIn.gl_sites.Header);

            DateTime ltime, ltimeTemp;
            ltime = DateTime.Now;

            Console.WriteLine("Start 6.0 Style writing output at {0}", ltime);

            if (rep == 0)
            {
                for (int i = 1; i <= species_num; i++)
                {
                    string gl_spe_attrs_i_name = PlugIn.gl_spe_Attrs[i].Name;

                    //Console.Write("creating {0} {1} {2} {3}\n", PlugIn.gl_sites.SpecNum, rep, gl_spe_attrs_i_name, "age map");

                    reclass.speciesAgeMap(m, gl_spe_attrs_i_name);

                    m.CellSize = PlugIn.gl_param.CellSize;

                    string name = output_dir + "/" + gl_spe_attrs_i_name + "_" + local_string;

                    m.write(name, red2, green2, blue2);
                }
            }

            reclass.ageReclass(m);
            m.CellSize = PlugIn.gl_param.CellSize;
            string str = output_dir + "/ageOldest" + local_string;
            m.write(str, red2, green2, blue2);


            reclass.ageReclassYoungest(m);
            m.CellSize = PlugIn.gl_param.CellSize;
            str = output_dir + "/ageYoungest" + local_string;
            m.write(str, red2, green2, blue2);


            ltimeTemp = DateTime.Now;

            Console.Write("\nFinish 6.0 Style writing output at {0}\n", ltimeTemp);

            Console.Write("it took {0} seconds\n\n", ltimeTemp - ltime);

            Console.WriteLine("========================================\n");
        }


        //This will read in the global variable sites from class file and simgFileint file.
        //Class file is a file containing site descriptions for a set of class values.
        //Map file contains the corresponding  img map.  Thus every value in mapFile
        //is represented by the cooresponding class descritption represented in classFile. 
        //public static void inputImgSpec(StreamReader classFile, Dataset simgFileint, int yDim, int xDim)
        public static void inputImgSpec(string ReclassInFile, Dataset simgFileint)
        {
            //int max = -100;
            Band poBand = simgFileint.GetRasterBand(1);

            int xDim = simgFileint.RasterXSize;
            int yDim = simgFileint.RasterYSize;

            double adMax;
            int bGotMax;

            poBand.GetMaximum(out adMax, out bGotMax);

            Console.WriteLine("max value is {0}", adMax);


            float[] pafScanline = new float[xDim * yDim];

            poBand.ReadRaster(0, 0, xDim, yDim, pafScanline, xDim, yDim, 0, 0);
            
            double noDataValue;
            int hasNoData;
            poBand.GetNoDataValue(out noDataValue, out hasNoData);

            Console.WriteLine("reading age cohort in inputImgSpec2: input file is {0}", ReclassInFile);

            int numCovers = 0;
            /*
            using (StreamReader rcFile = new StreamReader(ReclassInFile))
            {
                site tmp_site = new site();

                while (!system1.LDeof(rcFile))
                {
                    tmp_site.read(rcFile);
                    numCovers++;
                }
            }
            */
            var tmp = Landis.Data.Load<SpeciesParam>(ReclassInFile, new SpeciesParamParser());
            site[] s = new site[tmp.Count / species.NumSpec];
            if (tmp.Count % species.NumSpec != 0)
                throw new Exception("In_Output: number of lines in " + ReclassInFile + " cannot devided by number of species");
            numCovers = s.Length;
            int now = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                s[i] = new site();
                for (int j = 1; j <= s[0].Num_Species; ++j)
                {
                    s[i][j].VegPropagules = tmp.VegPropagules(now);
                    s[i][j].setAgeVector(tmp.Agevector(now));
                    now += 1;
                    if (now >= tmp.Count)
                        break;
                }
            }
            /*
            using (StreamReader rcFile = new StreamReader(ReclassInFile))
            {
                for (int i = 0; i < numCovers; i++)
                {
                    s[i] = new site();
                    s[i].read(rcFile);
                }
            }*/


            int[] combineMatrix = null;

            if (pro0or401 == 0) // for degug??
            {
                PlugIn.gl_sites.SortedIndex = new List<site>();
                for (int i = 0; i < numCovers; i++)
                {
                    site local_site = new site();

                    local_site.copy(s[i]);
                    PlugIn.gl_sites.SortedIndex.Add(local_site);
                }

                combineMatrix = new int[numCovers];

                for (int i = 0; i < numCovers; i++)
                {
                    combineMatrix[i] = i;
                }
            }

            if (pro0or401 == 0) //for debug??
            {
                lookupredundant(combineMatrix, numCovers);
            }

            Console.Write("Reading species composition map in inputImgSpec1\n");

            for (uint i = (uint)yDim; i > 0; i--)
            {
                for (uint j = 1; j <= xDim; j++)
                {
                    int coverType = (int)pafScanline[(yDim - i) * xDim + j - 1];

                    if (coverType < numCovers && coverType >= 0)
                    {
                        if (pro0or401 == 0)
                        {
                            site local_site = PlugIn.gl_sites.SortedIndex[combineMatrix[coverType]];
                            PlugIn.gl_sites.fillinSitePt(i, j, local_site);
                            local_site.NumSites++;
                        }
                        else
                        {
                            PlugIn.gl_sites[i, j] = new site();
                            PlugIn.gl_sites[i, j].copy(s[coverType]);
                            //BRM
                            Library.BiomassCohorts.SiteCohorts siteCohorts = new Library.BiomassCohorts.SiteCohorts();
                            site local_site = PlugIn.gl_sites[i, j];
                            specie t = local_site.first();
                            while (t != null)
                            {
                                for (uint p = 0; p < t.Length; p++)
                                {
                                    uint treeNum = t.getAgeVector((int)p);
                                    if (treeNum > 0)
                                    {
                                        siteCohorts.AddNewCohort(t.Species, (ushort)((p+1) * time_step), 0);
                                        //siteCohorts.AddNewCohort(t.Species);
                                    }
                                }
                                t = local_site.next();
                            }
                            //SiteVars.Cohorts[PlugIn.ModelCore.Landscape.GetSite((int)i, (int)j)] = s[coverType];
                            SiteVars.Cohorts[PlugIn.ModelCore.Landscape.GetSite((int)i, (int)j)] = siteCohorts;
                            //BRM
                        }

                    }
                    else
                    {
                        s = null;
                        throw new Exception("Error reading in coverType from the map file");
                    }
                }
            }

            if (pro0or401 == 0) // for debug??
            {
                Console.Write("releasing redundant memory\n");

                deleteRedundantInitial(combineMatrix, numCovers);

                PlugIn.gl_sites.SITE_sort();

                combineMatrix = null;
            }

            s = null;
        }


        //This will read in all LANDIS global variables.
        //public static void getInput(int[] freq, string[] reMethods, string[] ageMap, pdp ppdp, int BDANo, double[] wAdfGeoTransform)
        //public static void getInput(int[] freq, string[] ageMap, pdp ppdp, int BDANo, double[] wAdfGeoTransform)
        public static void getInput(int[] freq, pdp ppdp)
        {
            //BinaryReader ltMapFile = new BinaryReader(File.Open(succession.gl_param.landUnitMapFile, FileMode.Open)),
            Dataset simgFile;
            if ((simgFile = (Dataset)Gdal.Open(PlugIn.gl_param.SiteImgFile, Access.GA_ReadOnly)) == null)
                throw new Exception("species img map input file not found.");

            double[] adfGeoTransform = new double[6];
            
            
            string mapProj = simgFile.GetProjectionRef();
            
            simgFile.GetGeoTransform(adfGeoTransform);

            for (int i = 0; i < 6; i++)
                wAdfGeoTransform[i] = adfGeoTransform[i];
            
            mapCRS = simgFile.GetProjectionRef();
            Gdal.AllRegister();

            Console.WriteLine("Reading input.");

            Console.WriteLine("output dir is {0}", output_dir);
            DirectoryInfo dir = new DirectoryInfo(output_dir);
            dir.Create();

            Timestep.Set_SpecNum(PlugIn.gl_spe_Attrs.NumAttrs);


            for (int x = 1; x <= PlugIn.gl_spe_Attrs.NumAttrs; x++)
                Timestep.Set_longevity(x, (uint)PlugIn.gl_spe_Attrs[x].Longevity);

            if ((gDLLMode & defines.G_HARVEST) != 0)
                PlugIn.NumSpecies = PlugIn.gl_spe_Attrs.NumAttrs;

            uint[] dest = new uint[32];

            uint nCols = (uint)simgFile.RasterXSize;
            uint nRows = (uint)simgFile.RasterYSize;

            PlugIn.gl_sites.MaxDistance = PlugIn.gl_spe_Attrs.MaxDistance;

            PlugIn.gl_sites.MaxShadeTolerance = PlugIn.gl_spe_Attrs.MaxShade;

            PlugIn.gl_sites.dim(PlugIn.gl_spe_Attrs.NumAttrs, nRows, nCols);
            PlugIn.gl_sites.Read70OutputOption(PlugIn.gl_param.OutputOption70);

            
            ppdp.set_parameters(gDLLMode, PlugIn.gl_sites.numColumns, PlugIn.gl_sites.numRows);

            inputImgSpec(PlugIn.gl_param.ReclassInFile, simgFile);
            simgFile.Dispose();

            var freqOfOutput = new StreamReader(PlugIn.gl_param.Freq_out_put);

            int c = 0;

            while (!system1.LDeof(freqOfOutput))
            {
                string temp = system1.read_string(freqOfOutput);

                freq[c] = int.Parse(temp);

                if (freq[c] > PlugIn.gl_param.Num_Iteration)
                    throw new Exception("frequency value cannot be larger than number of iterations");

                if (freq[c] < 0)
                    throw new Exception("frequency value cannot be smaller than zero");

                c++;
            }

            Console.WriteLine();

            //if (freq[0] < succession.gl_sites.TimeStep)
            if (freq[0] <= 1)
            {
                freq[0] = 1;
                Console.Write("Species maps output every------>{0} years.\n", freq[0] * time_step);
                Console.Out.Flush();
                Console.Write("Age maps output every---------->{0} years.\n", freq[0] * time_step);
                Console.Out.Flush();
            }
            else
            {
                Console.Write("Species map outputs for year--->{0}.\n", freq[0] * time_step);
                Console.Out.Flush();
                Console.Write("Age map outputs for year------->{0}.\n", freq[0] * time_step);
                Console.Out.Flush();
            }


            //if (freq[4] < succession.gl_sites.TimeStep)
            if (freq[4] <= 1)
            {
                freq[4] = 1;
                Console.Write("Age group maps output every---->{0} years.\n", freq[4] * time_step);
                Console.Out.Flush();
            }
            else
            {
                Console.Write("Age group map outputs for year->{0}.\n", freq[4] * time_step);
                Console.Out.Flush();
            }

            Console.WriteLine();

            freqOfOutput.Close();


            //Set age colors to a spectrum.
            if ((gDLLMode & defines.G_WIND) != 0)
            {
                for (int i = 2; i < 16; i++)
                    HSV_to_RGB((float)(i - 2) / 14.0f * 360.0f, 1.0f, 1.0f, ref red2[i], ref green2[i], ref blue2[i]);
            }


            //Write landtype map
            map8 m = new map8(PlugIn.gl_sites.Header);

            reclass.luReclass(m);

            m.CellSize = PlugIn.gl_param.CellSize;

            m.write(output_dir + "/lu", red, green, blue);
        }




        //This will print the landis parameter file format on CRT.
        void help()
        {
            Console.Write("-------------------------LANDIS PARAMETER INPUT FILE--------------------------");

            Console.Write("Species Attribute File   #Species vital attribute file#");

            Console.Write("Landtype Attribute File  #Attributes of each landtype class#");

            Console.Write("Landtype Map File        #Input map of landtype#");

            Console.Write("Species Map File         #Input map of species and their age classes#");

            Console.Write("Map Attribute File       #Attributes of each map elements#");

            Console.Write("Maps Indexes File        #Species name indexes for map output#");

            Console.Write("Age Index File           #Species name indexes for age map output#");

            Console.Write("Output Directory         #Path for Landis output#");

            Console.Write("Disturbance File         #Disturbance Input File#");

            Console.Write("default.plt              #Required in the parameter file directory#");

            Console.Write("freq_out.put             #Setting for alternative output#");

            Console.Write("Iteration Number         #Number of iterations of 10 year step#");

            Console.Write("Random Number Seed       #0: real time seed, others: fixed seed#");

            Console.Write("Cell Size                #Cell size in meters#");

            Console.Write("Seed Dispersal Method    #The name of the seed dispersal routine#");

            Console.Write("Wind Switch              #0-no, 1-standard, 2-mean, 3-strong, 4-light#");

            Console.Write("Fire Switch              #0-no, 1-standard, 2-mean, 3-strong, 4-light#");

            Console.Write("Harvesting Switch        #0-harvesting off, 1-harvesting on#");

            if ((gDLLMode & defines.G_HARVEST) != 0)
            {

                //#ifdef __HARVEST__

                Console.Write("Harvest Event Switch     #0-harvesting module off, 1-harvesting moduel on#");

                Console.Write("Stand Adjacency Flag     #0-off, 1-on#");

                Console.Write("n                        #decade span to consider an adjacent stand recently harvested#");

                Console.Write("p                        #Proportion of cells cut in the last n decades is at least p#");

                Console.Write("Harvest Event File       #User defined harvest scenarios#");

                Console.Write("Stand Map                #Stand identifier map#");

                Console.Write("Management Area Map      #Management area identifier map#");

                Console.Write("Stand Log File           #Harvest log by stands#");

                Console.Write("Management Area Log File #Harvest log by management areas#");

                //#endif

            }

        }




        //This will print the valid argument list on CRT
        public static void argHelp()
        {
            Console.WriteLine("LANDIS Version Pro 7.0 ");

            Console.WriteLine("University of Missouri--Columbia ");

            Console.WriteLine("(C) Copyright 2000-2009");

            Console.WriteLine(" ");

            Console.WriteLine("Usage: LANDIS [-argument [...]] <input file> ");

            Console.WriteLine(" ");

            Console.WriteLine("       argument:");

            Console.WriteLine("       -e: the years environmental change interpreted");

            Console.WriteLine("       -h: this help menu");

            Console.WriteLine("       -p: input file help");

            Console.WriteLine("       -r: the year for reclassification");

        }




        public static void OutputScenario()
        {
            Console.Write("Output Landis Scenario.txt....\n\n");

            StreamWriter pfScenario = new StreamWriter(output_dir + "/Scenario.txt");

            pfScenario.Write("Landis Scenario.txt\n\n");

            pfScenario.Write("Landis version:		Pro 1.0\n");

            pfScenario.Write("Output dir:		{0}\n", output_dir);

            pfScenario.Write("specAttrFile:		{0}\\{1}\n", output_dir, PlugIn.gl_param.ExtraSpecAtrFile);

            pfScenario.Write("landUnitFile:		{0}\\{1}\n", output_dir, PlugIn.gl_param.LandUnitFile);

            //pfScenario.Write("landUnitMapFile:	{0}\\{1}\n", succession.gl_param.outputDir, succession.gl_param.landUnitMapFile);

            pfScenario.Write("siteInFile:		{0}\\{1}\n", output_dir, "(null)");


            pfScenario.Write("\nMAIN PARAMETERS:----------------------------------------\n");


            if (PlugIn.gl_param.RandSeed != 0)
                pfScenario.Write("Random:			repeatable (1)\n");
            else
                pfScenario.Write("Random:			NOT repeatable (0)\n");


            pfScenario.Write("numberOfIterations:	{0}\n", PlugIn.gl_param.Num_Iteration);

            pfScenario.Write("Map size:		Row: {0} x Col: {1}\n", PlugIn.gl_sites.numRows, PlugIn.gl_sites.numColumns);



            pfScenario.Write("\n\nDLLs----------------------------------------\n");

            if ((gDLLMode & defines.G_BDA) != 0)
                pfScenario.Write("BDA ...............is turned on\n");

            if ((gDLLMode & defines.G_WIND) != 0)
                pfScenario.Write("Wind ..............is turned on \n");

            if ((gDLLMode & defines.G_HARVEST) != 0)
                pfScenario.Write("Harvest ...........is turned on \n");

            if ((gDLLMode & defines.G_FUEL) != 0)
                pfScenario.Write("Fuel ..............is turned on \n");

            if ((gDLLMode & defines.G_FUELMANAGEMENT) != 0)
                pfScenario.Write("Fuel management....is turned on \n");

            if ((gDLLMode & defines.G_FIRE) != 0)
                pfScenario.Write("Fire ..............is turned on \n");

            pfScenario.Write("\n");

            if ((gDLLMode & defines.G_BDA) == 0)
                pfScenario.Write("BDA ....................  off\n");

            if ((gDLLMode & defines.G_WIND) == 0)
                pfScenario.Write("Wind .................... off \n");

            if ((gDLLMode & defines.G_HARVEST) == 0)
                pfScenario.Write("Harvest ..................off \n");

            if ((gDLLMode & defines.G_FUEL) == 0)
                pfScenario.Write("Fuel .................... off\n");

            if ((gDLLMode & defines.G_FUELMANAGEMENT) == 0)
                pfScenario.Write("Fuel management ..........off \n");

            if ((gDLLMode & defines.G_FIRE) == 0)
                pfScenario.Write("Fire .................... off\n");

            pfScenario.Close();
        }

    }

    public class SpeciesParam
    {
        private List<short> vegPropagules = new List<short>();
        public int Count
        {
            get { return vegPropagules.Count; }
        }
        private List<uint[]> agevector = new List<uint[]>();
        public void addVegPropagules(short x)
        {
            vegPropagules.Add(x);
        }
        public void addAgevector(uint[] x)
        {
            agevector.Add(x);
        }
        public short VegPropagules(int i)
        {
            return vegPropagules[i];
        }
        public uint[] Agevector(int i)
        {
            return agevector[i];
        }
    }

    class SpeciesParamParser : Landis.TextParser<SpeciesParam>
    {
        public override string LandisDataValue
        {
            get { return "Initial Communities"; }
        }

        protected override SpeciesParam Parse()
        {
            ReadLandisDataVar();
            SpeciesParam ret = new SpeciesParam();
            InputVar<int> int_val = new InputVar<int>("int value");
            while (!AtEndOfInput)
            {
                Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);
                ReadValue(int_val, currentLine);
                ret.addVegPropagules((short)int_val.Value.Actual);
                List<uint> tmp = new List<uint>();
                tmp.Add(0);
                while (currentLine.Peek() != -1)
                {
                    ReadValue(int_val, currentLine);
                    tmp.Add((uint)int_val.Value.Actual);
                    Landis.Utilities.TextReader.SkipWhitespace(currentLine);
                }
                ret.addAgevector(tmp.ToArray());
                GetNextLine();
            }
            return ret;
        }
    }
}
