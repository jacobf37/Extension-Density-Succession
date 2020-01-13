using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Landis.Extension.Succession.Landispro
{

    public static class Timestep
    {
        private static uint[] countofagevector;

        private static uint time_step = 10;
        private static uint spec_num;

        public static uint timestep
        {
            get { return time_step; }
            set { time_step = value; }
        }

        public static uint get_countofagevector(int index)
        {
            return countofagevector[index];
        }



        public static void Set_SpecNum(uint in_spec_num)
        {
            spec_num = in_spec_num;

            countofagevector = new uint[spec_num];
        }


        public static void Set_longevity(int i, uint longevity)
        {
            if (i > spec_num || i < 0)
                throw new Exception("Illegal spec index in set longevity");

            countofagevector[i - 1] = longevity / time_step;
        }

    }



    /****************************************************************
    // there is operator = overload in c++ version, attention!!!!!!!!
    ****************************************************************/

    public class agelist
    {
        /// <summary>
        /// The following masks are used by the agelist class to process bits.  
        /// lowMask is a mask with the low bit set, highMask has the high bit set and fullMask has all bits sets
        /// Mask is an array with 32 elements, each having their respective bit set.
        /// </summary>
        //static readonly uint lowMask  = 0x00000001;
        //static readonly uint highMask = 0x80000000;
        //static readonly uint fullMask = 0XFFFFFFFF;

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

        protected uint[] agevector;
        private readonly int time_step = (int)Timestep.timestep;


        public agelist() { }


        public agelist(agelist in_agelist)
        {        
            this.copy(in_agelist);
        }


        public void copy(agelist in_agelist)
        {
            if(in_agelist == null)
                return;

            this.copy(in_agelist.agevector);
        }


        protected void copy(uint[] in_agevector)
        {
            if (in_agevector == null)
                return;
                
            uint count = in_agevector[0];

            agevector = new uint[count + 1];

            for (int i = 0; i <= count; i++)
                agevector[i] = in_agevector[i];
        }



        public uint getTreeNum(int n, int specIndex)
        {
            if (n == 0)
                return 0; //Add By Qia on March 23 2010 for 'n' index 0 bug

            if (n < 0 || n > Timestep.get_countofagevector(specIndex - 1))
                throw new Exception("Index age error in agelist 70 Pro");

            return agevector[n];
        }


        public void setTreeNum(int n, int specIndex, int num)
        {
            if (n < 1 || n > Timestep.get_countofagevector(specIndex - 1))
                throw new Exception("set age error in agelist 70 Pro");

            if (num < 0)
                agevector[n] = 0;
            else
                agevector[n] = (uint)num;
        }



        public void GrowTree()
        {
            for (uint i = agevector[0]; i > 1; i--)
                agevector[i] = agevector[i - 1];

            agevector[1] = 0;
        }


        public void Display()
        {
            for (uint i = 1; i <= agevector[0]; i++)
                Console.Write("{0} ", agevector[i]);
            Console.WriteLine();
        }



        public uint getAgeVector(int i)
        {
            if (i < 0)
                throw new Exception("getAgeVector error");

            return agevector[i];
        }



        public uint getAgeVectorNum()
        {
            return agevector[0];
        }


        public uint TimeStep
        {
            get { return Timestep.timestep; }
        }



        public void AGELISTAllocateVector(int index)
        {
            uint count = Timestep.get_countofagevector(index);

            agevector = new uint[count + 1];

            agevector[0] = count;

            //for (int i = 1; i <= count; i++)
            //    agevector[i] = 0;
        }






        //Clear agelist.
        public void clear()
        {
            if (agevector == null)
            {
                agevector = new uint[320 + 1];
                agevector[0] = 320;

                // for (i = 1; i <= agevector[0]; i++)
                //  agevector[i] = 0;
            }
            else
            {
                for (int i = 1; i <= agevector[0]; i++)
                    agevector[i] = 0;
            }


        }



        //Set an age to true.
        public void set(int age)
        {
            if (age % time_step == 0)
                age = age / time_step;
            else
                age = age / time_step + 1;


            if (age < 1 || age > 320 / time_step)
                throw new Exception("Illegal age");


            int temp1 = (age - 1) / 32;
            int temp2 = (age - 1) % 32;

            if (age == 64)
                Console.WriteLine("something happened");

            agevector[temp1] |= mask[temp2];

        }





        //Reset an age to false.
        public void reset(int age)
        {
            if (age % time_step == 0)
                age = age / time_step;
            else
                age = age / time_step + 1;


            if (age < 1 || age > 320 / time_step)
                throw new Exception("Illegal age");

            if (age < 1 || age > agevector[0])
                throw new Exception("Agelist Reset Problem");

            agevector[age] = 0;
        }




        //Returns true if any age clases are present, false otherwise.
        public bool query()
        {
            for (int i = 1; i <= agevector[0]; i++)
            {
                if (0 == agevector[i])
                    continue;
                else
                    return true;
                //result = result || agevector[i];
            }

            return false;
        }



        public bool query(int age)
        {
            if (age % time_step == 0)
                age = age / time_step;
            else
                age = age / time_step + 1;


            if (age < 1 || age > 320 / time_step)
                throw new Exception("Illegal age");

            if (age < 1 || age > agevector[0])
                throw new Exception("Agelist Reset Problem");

            //return agevector[age];
            if (agevector[age] == 0)
                return false;
            else
                return true;
        }





        //Return true if an age exists in the range bounded by low and high, false otherwise.
        public bool query(int low, int high)
        {
            if (low < 1 || low > 320 / time_step)
                throw new Exception("Illegal lower bound");

            if (high < 1 || high > 320 / time_step)
                throw new Exception("Illegal upper bound");

            if (low > high)
                throw new Exception("Lower bound is greater than upper bound");



            if (low % time_step == 0)
                low = low / time_step;
            else
                low = low / time_step + 1;


            if (high % time_step == 0)
                high = high / time_step;
            else
                high = high / time_step + 1;


            for (int i = low; i <= high; i++)
            {
                if (i < 1 || i > agevector[0])
                    throw new Exception("Agelist Reset Problem");

                if (agevector[i] != 0)
                    return true;
            }

            return false;

        }




        //Returns the youngest age present.
        public int youngest()
        {
            for (int j = 1; j <= agevector[0]; j++)
            {
                if (agevector[j] > 0)
                    return j * time_step;
            }

            return 0;
        }





        //Returns the oldest age present.
        public uint oldest()
        {
            for (uint j = agevector[0]; j >= 1; j--)
            {
                if (agevector[j] > 0)
                    return (uint)(j * time_step);
            }

            return 0;
        }





        //Returns the number of age classes present.
        public int number()
        {
            int num = 0;

            for (int j = 1; j <= agevector[0]; j++)
            {
                if (agevector[j] > 0)
                    num++;
            }

            return num;
        }




        //This shall read an age list from a file.
        public void read(StreamReader infile)
        {
            int numSet = 0, buffer1 = 0, buffer2 = 0, barflag = 0;

            clear();

            int int_times = (int)time_step;

            system1.skipblanks(infile);

            for (int j = 0; numSet <= 320 && !infile.EndOfStream; j++)
            {
                char ch = (char)system1.LDfgetc(infile);

                if (Char.IsDigit(ch))
                {
                    if (barflag == 0)
                        buffer1 = buffer1 * 10 + ch - 48;
                    else
                        buffer2 = buffer2 * 10 + ch - 48;
                }
                else
                {
                    if (ch == '-')
                    {
                        barflag = 1;
                    }
                    else if (ch == ' ')
                    {
                        if (barflag == 1)
                        {
                            barflag = 0;

                            for (int temp = buffer1; temp <= buffer2; temp = temp + int_times)
                            {
                                if (temp >= int_times)
                                {
                                    agevector[temp / int_times] = 1;

                                    numSet++;
                                }

                            }

                            buffer1 = 0;
                            buffer2 = 0;
                        }
                        else
                        {
                            int temp = buffer1;

                            if (temp >= int_times)
                            {
                                agevector[temp / int_times] = 1;

                                numSet++;

                                buffer1 = 0;
                                buffer2 = 0;
                            }
                        }

                    }
                    else
                    {
                        if (barflag == 1)
                        {
                            barflag = 0;

                            for (int temp = buffer1; temp <= buffer2; temp = temp + int_times)
                            {
                                if (temp >= int_times)
                                {
                                    agevector[temp / int_times] = 1;
                                    numSet++;
                                }
                            }

                            buffer1 = 0;
                            buffer2 = 0;

                        }
                        else
                        {
                            int temp = buffer1;

                            if (temp >= int_times)
                            {
                                agevector[temp / int_times] = 1;

                                numSet++;

                                buffer1 = 0;
                                buffer2 = 0;

                            }

                        }

                        break;

                    }

                }//end else

            }//end for


        }




        public void readTreeNum(StreamReader infile, int specIndex)
        {
            int numSet = 0, buffer1 = 0, spaceflag = 0;

            clear();

            system1.skipblanks(infile);

            for (int j = 0; numSet < Timestep.get_countofagevector(specIndex) && !infile.EndOfStream; j++)
            {
                //char ch = (char)infile.Read();
                char ch = (char)system1.LDfgetc(infile);

                if (ch == '\n')
                    break;

                if (Char.IsDigit(ch))
                {
                    buffer1 = buffer1 * 10 + ch - 48;

                    spaceflag = 0;
                }
                else
                {
                    if (spaceflag == 0)
                    {
                        agevector[numSet + 1] = (uint)buffer1;

                        numSet++;

                        buffer1 = 0;

                        spaceflag = 1;
                    }

                }

            }


            if (numSet < 1 || numSet > Timestep.get_countofagevector(specIndex))
                throw new Exception("specie composition error");

        }





        //This will write an age list to a file.
        public void write(StreamWriter outfile)
        {
            for (int j = 0; j < 320 / time_step; j++)
            {
                int temp1 = j / 32;
                int temp2 = j % 32;

                uint ret = agevector[temp1] & mask[temp2];

                if (ret != 0)
                    outfile.Write("1");
                else
                    outfile.Write("0");

            }

        }




        //This will dump an age list to the CRT.
        public void dump()
        {
            for (int j = 0; j < 320 / time_step; j++)
            {
                int temp1 = j / 32;
                int temp2 = j % 32;

                uint ret = agevector[temp1] & mask[temp2];

                if (ret != 0)
                    Console.WriteLine("{1}: a", temp2);
                else
                    Console.WriteLine("{1}: b", temp2);
            }

            Console.WriteLine();
        }



    }
}
