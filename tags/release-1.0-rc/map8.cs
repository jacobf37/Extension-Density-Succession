using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using OSGeo.GDAL;
using OSGeo.OSR;

namespace Landis.Extension.Succession.Landispro
{
    //*******************************************
    //in original map8.cpp, there is "=" operator overload, attention
    //please use "copy" function instead
    //*******************************************
    public class map8
    {
        public static readonly int maxLeg             = 700;
        public static readonly int MapmaxValue        = 700;
        public static readonly uint MaxValueforLegend = 700;

        //private int flag8or16; //Pointer to data array.


        private uint numCols, numRows;      //Map dimensions.

        private ushort[] data;       //Pointer to data array.

        //private char[] title = new char[80];            //Title.
        private string title;

        //private char[,] legend = new char[MapmaxValue, 64];   //Legend nodes.
        private string[] legend = new string[MapmaxValue];

        private ushort largeCell;   //Largest cell value.

        private float cellSize;            //Cell size in meters.

        //Default red, white and blue color values.
        private byte[] red = new byte[maxLeg];
        private byte[] green = new byte[maxLeg];
        private byte[] blue = new byte[maxLeg];

        private uint[] header = new uint[32];  //Default file header.



        public map8()
        {
            title = "NO TITLE";

            for (int i = 0; i < MapmaxValue; i++)
                legend[i] = i.ToString();

            largeCell = 0;

            data = null;

            Array.Clear(header, 0, header.Length);
        }



        //Constructor.  Initializes map coordinates
        public map8(uint[] in_dest) : this()
        {
            //Array.Copy(in_dest, header, in_dest.Length);
            Array.Copy(in_dest, header, header.Length);
        }



        //Constructor.  Dimensions given.
        public map8(uint r, uint c) : this()
        {
            //this might be problematic : -> original: sprintf(legend[i], "<%d>", i);
            dim(r, c);
        }


        ~map8() { data = null; }




        //Read map from a file.
        //Input file is an ERDAS GIS File with no extension.
        public void read(string fn)
        {
            using (BinaryReader br = new BinaryReader(File.Open(fn, FileMode.Open)))
            {
                for (int i = 0; i < 32; i++)
                    header[i] = br.ReadUInt32();

                cellSize  = br.ReadSingle();
                cellSize *= br.ReadSingle();

                uint xDim = header[4]; 
                uint yDim = header[5]; 

                dim(yDim, xDim);


                for (uint i = yDim; i > 0; i--)
                {
                    for (uint j = 1; j <= xDim; j++)
                    {
                        this[i, j] = (ushort)br.Read(); //fread((char*)(&c), 1, 1, fp);						

                        if (this[i, j] > largeCell)
                            largeCell = this[i, j];
                    }

                }

            }



            /*TRAILER FILE*/
            string str = fn + ".trl";
            using (BinaryReader br2 = new BinaryReader(File.Open(str, FileMode.Open)))
            {
                byte[] dest = new byte[128];

                br2.Read(dest, 0, 128);

                br2.Read(green, 0, 128);
                br2.Read(green, 128, 128);
                br2.Read(red, 0, 128);
                br2.Read(red, 128, 128);
                br2.Read(blue, 0, 128);
                br2.Read(blue, 128, 128);

                br2.Read(dest, 0, 128); /*Empty Record*/

                for (int i = 9; i <= 16; i++)                  /*Histogram*/
                    br2.Read(dest, 0, 128);


                byte[] tmp_str = new byte[32];

                for (int i = 0; i <= (int)largeCell; i++)
                {
                    br2.Read(tmp_str, 0, 32);//fread(legend[i], 1, 32, fp2);

                    string full_string = Encoding.ASCII.GetString(tmp_str);

                    int index = full_string.IndexOf("~");

                    if (index > 0)
                        legend[i] = full_string.Substring(0, index);

                }
            }

        }




        //Write map to file.  There is no extension on the file name.
        //Three arrays of colors cooresponding to the palette (RGB) are also included.
        public void write(string fn, byte[] red, byte[] green, byte[] blue)
        {
            /*for (uint i = numRows; i > 0; i--)
			{
				for (uint j = 1; j <= numCols; j++)
				{
					if (this[i, j] > largeCell)
						largeCell = this[i, j];
				}
			}

            byte[] tmp_header = new byte[4 * 32];
            Buffer.BlockCopy(header, 0, tmp_header, 0, tmp_header.Length);

			string str0 = "HEAD74";
			
			for(int i=0; i<str0.Length; i++)
	        		tmp_header[i] = (byte)str0[i];

            tmp_header[6] = 2;
            tmp_header[7] = 0;
            tmp_header[8] = 1;

	       	int leg0 = 4 * sizeof(uint) - str0.Length - 3;
	        	
        	for(int i=0; i<leg0; i++)
            {
                //tmp_header[i + str0.Length + 3] = (byte)' ';
                tmp_header[i + str0.Length + 3] = 0;
            }
        	
        	byte[] cols_bytes = BitConverter.GetBytes(numCols);

        	for(int i=0; i<4; i++)
        		tmp_header[i + 4 * sizeof(uint)] = cols_bytes[i];


        	byte[] rows_bytes = BitConverter.GetBytes(numRows);

        	for(int i=0; i<4; i++)
        		tmp_header[i + 5 * sizeof(uint)] = rows_bytes[i];


        	byte[] cellsize_bytes = BitConverter.GetBytes(cellSize);

        	for(int i=0; i<4; i++)
        		tmp_header[i + 30 * sizeof(uint)] = cellsize_bytes[i];

        	

        	//header = tmp_header.Select(x => (uint)x).ToArray(); //bsl not sure
            for (int i = 0; i < tmp_header.Length; i+=4)            
                header[i/4] = BitConverter.ToUInt32(tmp_header, i);*/


            //for (int i = 0; i < 32; i++)
            //{
            //    Console.Write("{0}: {1} -- {2} -- {3} -- {4}  ", i, tmp_header[i * 4], tmp_header[i * 4 + 1], tmp_header[i * 4 + 2], tmp_header[i * 4 + 3]);
            //    Console.WriteLine("header[{0}] = {1}", i, header[i]);
            //}
            //Console.ReadLine();

            Driver poDriver = Gdal.GetDriverByName("HFA");
            if (poDriver == null)
                throw new Exception("start driver fail in map8/write");

            string[] papszMetadata = poDriver.GetMetadata("");

            Dataset poDstDS = null;//*

            Band outPoBand = null; //*

            double[] wAdfGeoTransform = new double[6] { 0.00, cellSize, 0.00, 600.00, 0.00, -cellSize };//*

            string[] papszOptions = null;//*

            //float[] pafScanline = null;

            int[] pintScanline = null; //*

            string pszSRS_WKT = null;//*

            SpatialReference oSRS = new SpatialReference(null);//*

            oSRS.SetUTM(11, 1);//*

            oSRS.SetWellKnownGeogCS("HEAD74");//*NAD27

            oSRS.ExportToWkt(out pszSRS_WKT);//*

            poDstDS = poDriver.Create(fn + ".img", (int)numCols, (int)numRows, 1, DataType.GDT_UInt32, papszOptions);//*

            pintScanline = new int[numRows * numCols];

            if (poDstDS == null)
            {
                Console.Write("could not create file for img file output");//*

                return;
            }

            poDstDS.SetGeoTransform(wAdfGeoTransform);//*

            outPoBand = poDstDS.GetRasterBand(1);//*

            //string str = fn + ".gis";

            //using (BinaryWriter bw = new BinaryWriter(File.Open(str, FileMode.Create)))
            {
                //bw.Write(tmp_header);//fwrite((char*)header, 4, 32, fp);

                for (uint i = numRows; i > 0; i--)
                {
                    for (uint j = 1; j <= numCols; j++)
                    {
                        //bw.Write(this[i, j]);
                        pintScanline[(numRows - i) * numCols + j - 1] = this[i, j];
                        //Console.WriteLine(this[i, j]);
                    }
                }
            }
            //System.IO.File.WriteAllLines(fn+".txt", pintScanline.Select(tb => tb.ToString()));
            /*

	        str = fn + ".trl";
	        using (BinaryWriter bw2 = new BinaryWriter(File.Open(str, FileMode.Create)))
	        {
	        	//Trailer files.
				//Record 1:

	        	string str1 = "TRAIL74";

	        	char[] dest = new char[128];

	        	for(int i=0; i<str1.Length; i++)
	        		dest[i] = str1[i];

	        	int temp_leng = 18 * 4;
	        	// int leg = temp_leng - str1.Length;
	        	
	        	// for(int i=0; i<leg; i++)
	        	// 	dest[i + str1.Length] = ' ';

	        	for(int i=temp_leng; i<temp_leng+title.Length; i++)
	        		dest[i] = title[i-temp_leng];


	        	dest[temp_leng+title.Length] = '~';
	        	
	        	//not necessary, because they are 0
	        	// for(int i=temp_leng+title.Length+1; i<32*4; i++)
	        	// 	dest[i] = ' ';

	        	bw2.Write(dest);


                //Record 2-7:
                bw2.Write(green);
                bw2.Write(red);
                bw2.Write(blue);

				
				//Record 8:
				for(int i=0; i<str1.Length; i++)
	        		dest[i] = str1[i];

	        	for(int i=str1.Length; i<32*4; i++)
                {
                    //dest[i] = ' ';
                    dest[i] = (char)0;
                }

	        	bw2.Write(dest);


				//Record 9-16:
				Array.Clear(dest, 0, dest.Length);

				for (int i = 9; i <= 16; i++)
					bw2.Write(dest);
					

				//Record 17-56: maximum map legend 50
                for (int i = 0; i < MapmaxValue; i++)
                {
                    //char[] tmp = new char[32];
                    //tmp = (legend[i] + "~").ToCharArray();
                    //bw2.Write(tmp);

                    byte[] bArray = new byte[32];

                    bArray = Encoding.ASCII.GetBytes((legend[i] + "~").PadRight(32, ' '));                    
                    
                    //Console.WriteLine("bArray size = {0} size, {1}", bArray.Length, Encoding.Default.GetString(bArray));
                    bw2.Write(bArray, 0, 32);
                }

	        }*/
            outPoBand.WriteRaster(0, 0, (int)numCols, (int)numRows, pintScanline, (int)numCols, (int)numRows, 0, 0);//*

            //GDALClose((GDALDatasetH)poDstDS);//*
            poDstDS.Dispose();
        }




        //Write map to file.  There is no extension on the file name.  
        //Three arrays of colors cooresponding to the palette (RGB) are also included.
        public void write(string fn, int[] red, int[] green, int[] blue)
        {
            byte[] ured   = new byte[maxLeg];
            byte[] ugreen = new byte[maxLeg];
            byte[] ublue  = new byte[maxLeg];

            for (int i = 0; i < maxLeg; i++)
            {
                ured[i]   = (byte)red[i];
                ugreen[i] = (byte)green[i];
                ublue[i]  = (byte)blue[i];
            }

            write(fn, ured, ugreen, ublue);
        }



        //Write map to a file.  The color array is left unchanged from input
        public void write(string fn)
        {
            write(fn, red, green, blue);
        }


        //Read legend from a file.  First line of the file is a title.
        //All remaining lines are legends for up to fifteen classes.
        public void readLegend(string fn)
        {
            using (StreamReader sr = new StreamReader(fn))
            {
                title = sr.ReadLine();

                int i = 0;

                while (sr.Peek() >= 0)
                {
                    legend[i] = sr.ReadLine();

                    i++;
                }
            }
        }



        //This will copy the contents of another map into the current map.  
        //This includes legends, titles, and current display dimensions.
        public void copy(map8 n)
        {
            largeCell = n.largeCell;

            dim(n.numRows, n.numCols);

            title = n.title;

            for (int i = 0; i < maxLeg; i++)  //the original part might be problematic : for (int i = 0; i < 22; i++)
                legend[i] = n.legend[i];

            Array.Copy(n.data, data, n.data.Length);
        }



        //Returns a single map element.
        public ushort this[uint r, uint c]
        {
            get
            {
                if (r <= 0 || r > numRows || c <= 0 || c > numCols)
                    throw new Exception("MAP8:: illegal map coordinates");

                uint x = ((r - 1) * numCols) + c - 1;

                return data[x];
            }

            set
            {
                if (r <= 0 || r > numRows || c <= 0 || c > numCols)
                    throw new Exception("MAP8:: illegal map coordinates");

                uint x = ((r - 1) * numCols) + c - 1;
                
                //if (x == 180 && value != 699)
                //    x = x;//??????????????????
                
                data[x] = value;
            }
        }


        //This will dimension or redimension the map size.
        public void dim(uint r, uint c)
        {
            numCols = c;
            numRows = r;

            data = new ushort[numCols * numRows];
        }

        //Returns number of columns in map.
        public uint NumCols
        {
            get { return numCols; }
        }

        //Returns number of rows in map.
        public uint NumRows
        {
            get { return numRows; }
        }


        //This will rename the map.
        //c++ version: public void rename(char* c) 
        public void rename(string c)
        {
            title = c; //strncpy(title, c, 45);
        }

        //This will set map coordinates
        public void setHeader(uint[] in_dest)
        {
            //Array.Copy(in_dest, header, in_dest.Length);
            Array.Copy(in_dest, header, header.Length);
        }

        //This will assign a new name to a legend element.
        //public void assignLeg(int pos, char *c)
        public void assignLeg(uint pos, string c)
        {
            legend[pos] = c; //strncpy(legend[pos], c, 50);

            if (largeCell < pos)
                largeCell = (ushort)(pos + 1);
        }

        //This will fill a map with a single value.
        public void fill(ushort c)
        {
            for (int i = 0; i < numCols * numRows; i++)
                data[i] = c;
        }


        public float CellSize
        {
            get { return cellSize; }
            set { cellSize = value; }
        }

        //This will return the highest cell value in map.
        public int high()
        {
            return largeCell;
        }


        //This will return the contents of a legend item.
        public string legendItem(uint pos)
        {
            return legend[pos];
        }

    }









    class map16
    {
        private uint numCols, numRows;       //Map dimensions.	   

        private byte flag16or32;

        private ushort[] data;        //Pointer to data array.
        private uint[] data32;

        private uint largeCell;    //Largest cell value. 

        private float cellSize;             //Cell size in meters.

        //private uint header[32];            //Default file header.
        private uint[] header = new uint[30]; //bsl changes it from 32 to 30


        //Constructor.  No dimensions.
        public map16()
        {
            largeCell = 0;

            data = null;
            data32 = null;

            Array.Clear(header, 0, header.Length);
        }


        //Constructor.  Dimensions given.
        public map16(uint r, uint c) : this()
        {
            dim(r, c);
        }


        ~map16()
        {
            data = null;
            data32 = null;
        }



        //Read map from a file. Input file is an 16-bit ERDAS GIS File with no extension.
        public void read(string fn)
        {
            flag16or32 = 16;


            using (BinaryReader br = new BinaryReader(File.Open(fn, FileMode.Open)))
            {
                for (int i = 0; i < 30; i++)
                    header[i] = br.ReadUInt32();


                byte b16or8;   //true: 16, false 8 bit

                if ((header[1] & 0xff0000) == 0x020000)
                    b16or8 = 16;
                else if ((header[1] & 0xff0000) == 0)
                    b16or8 = 8;
                else
                    throw new Exception("Error: IO: Landtype map is niether 16 bit or 8 bit.");


                cellSize = br.ReadSingle();
                cellSize *= br.ReadSingle();

                uint xDim = header[4]; //Nim: changed yDim to xDim
                uint yDim = header[5]; //Nim: changed xDim to yDim

                dim(yDim, xDim);


                for (uint i = yDim; i > 0; i--)
                {
                    for (uint j = 1; j <= xDim; j++)
                    {
                        if (b16or8 == 8)
                            this[i, j] = (ushort)br.Read();//fread((char*)(&c8bit), 1, 1, fp);						
                        else
                            this[i, j] = (ushort)br.ReadUInt16();//fread((char*)(&c16bit), 2, 1, fp);			


                        if (this[i, j] > largeCell)
                            largeCell = this[i, j];
                    }

                }

            }


        }


        //Returns number of columns in map.
        public uint NumCols
        {
            get { return numCols; }
        }


        //Returns number of rows in map.
        public uint NumRows
        {
            get { return numRows; }
        }



        //Returns a single map element.
        public ushort this[uint r, uint c]
        {
            get
            {
                if (r <= 0 || r > numRows || c <= 0 || c > numCols)
                    throw new Exception("MAP8:: illegal map coordinates");

                uint x = ((r - 1) * numCols) + c - 1;

                return data[x];
            }

            set
            {
                if (r <= 0 || r > numRows || c <= 0 || c > numCols)
                    throw new Exception("MAP8:: illegal map coordinates");

                uint x = ((r - 1) * numCols) + c - 1;

                data[x] = value;
            }

        }




        //This will dimension or redimension the map size.
        public void dim(uint r, uint c)
        {
            numCols = c;
            numRows = r;

            flag16or32 = 16;

            data = new ushort[numCols * numRows];
        }




        //Read map from a file.   Input file is an 16-bit ERDAS GIS File with no extension.
        int readImg(string fn, int giRow, int giCol)
        {
            //char c8bit;
            Dataset simgFile = null;

            Gdal.AllRegister();

            if ((simgFile = Gdal.Open(fn, Access.GA_ReadOnly)) == null) //*
                throw new Exception(" img map input file not found.");  //*

            //xDim = header[4]; //Nim: changed yDim to xDim
            //yDim = header[5]; //Nim: changed xDim to yDim

            int xDim = simgFile.RasterXSize; 
            int yDim = simgFile.RasterYSize; 

            if (giRow != yDim)            
                throw new Exception("stand/managearea map's row and visitation map's row does not match");

            if (giCol != xDim)
                throw new Exception("stand/managearea map's column and visitation map's column does not match");

            //System.Diagnostics.Debug.Assert(giRow == yDim);
            //System.Diagnostics.Debug.Assert(giCol == xDim);


            dim((uint)yDim, (uint)xDim);

            float[] pafScanline = new float[(xDim * yDim)];//*

            Band poBand = simgFile.GetRasterBand(1);//*

            poBand.ReadRaster(0, 0, xDim, yDim, pafScanline, xDim, yDim, 0, 0);//*


            int numread = 1;

            for (uint i = (uint)yDim; i > 0; i--)
            {
                for (uint j = 1; j <= xDim; j++)
                {
                    int mapValue = (int)pafScanline[(yDim - i) * xDim + j - 1];//*

                    Console.Write("{0} ", mapValue);
                    
                    // %%# Changed 13
                    this[i, j] = (ushort)mapValue;

                    if (numread > 0)
                    {
                        if ((ushort)mapValue > largeCell)
                            largeCell = (ushort)mapValue;
                    }

                }

                Console.WriteLine();

            }

            return 0;

        }



        public void dim32(uint r, uint c)
        {
            numCols = c;
            numRows = r;

            flag16or32 = 32; //added by suliang bu

            data32 = new uint[numCols * numRows];
        }



        void readtxt(string fn, int giRow, int giCol)
        {
            readtxt(fn);
            Debug.Assert(numCols == giCol);
            Debug.Assert(numRows == giRow);
        }

        

	    //This will fill a map with a single value.
	    public void fill(ushort c)
		{
			for (int i = 0; i < numCols * numRows; i++)
				data[i] = c;
		}

	    //This will return the highest cell value in map.
	    public int high()
	    {
	    	return (int)largeCell;
	    }


	    public bool inMap(uint r, uint c)
	    {
	    	bool result = (r >= 1 && r <= numRows && c >= 1 && c <= numCols);
            
			return result;
	    }


		public void freeMAPdata()
		{
			if (flag16or32 == 16)
			{
				data = null;
			}
			else
			{
				Debug.Assert(flag16or32 == 32);

				data32 = null;
			}
		}


		public void readtxt(string fn)
		{
			flag16or32 = 32;
			
			
			using (StreamReader sr = new StreamReader(fn)) 
            {
                system1.read_string(sr);
                uint xDim = (uint)system1.read_int(sr);

                system1.read_string(sr);
                uint yDim = (uint)system1.read_int(sr);

                system1.read_string(sr);
                system1.read_string(sr);
                system1.read_string(sr);
                system1.read_string(sr);
                system1.read_string(sr);

                cellSize = system1.read_float(sr);
                
                system1.read_string(sr);
                system1.read_string(sr);


				dim32(yDim, xDim);


				for (uint i = yDim; i > 0; i--)
				{
					for (uint j = 1; j <= xDim; j++)
					{
						uint mapValue = (uint)system1.read_int(sr);

						putvalue32in(i, j, mapValue);

						if ((uint)mapValue > largeCell)
							largeCell = mapValue;
						
					}

				}
            }

	
		}


		public void putvalue32in(uint r, uint c, uint value)
		{
			if (r <= 0 || r > numRows || c <= 0 || c > numCols)			
				throw new Exception("MAP8:: illegal map coordinates");

			uint x = ((r - 1) * numCols) + c - 1;

			data32[x] = value;
		}


		public int getvalue32out(uint r, uint c)
		{
			if (r <= 0 || r > numRows || c <= 0 || c > numCols)			
					throw new Exception("MAP8:: illegal map coordinates");
	
			uint x = ((r - 1) * numCols) + c - 1;

			if (flag16or32 == 16)
			{
				return this[r, c];
			}
			else
			{
				Debug.Assert(flag16or32 == 32);
				
				return (int)data32[x];
			}
		}
	

	}
}
