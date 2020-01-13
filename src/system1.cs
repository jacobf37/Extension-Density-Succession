using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace Landis.Extension.Succession.Landispro
{
    public static class system1
    {
        private const double PI = 3.141592654;

        private readonly static char DELIMIT = '#';

        private static int stringCommentFlag = 0;
        private static int charCommentFlag   = 0;

        private static Random fixRand = new Random();

        private const int RAND_MAX = 0x7fff;
        //==========================================================================

        [DllImport("msvcrt.dll")]
        public static extern int srand(int seed);

        [DllImport("msvcrt.dll")]
        public static extern int rand();

        // Given a string, if it is part of comment return TRUE, otherwise FALSE
        // private bool isStringComment(char *s)
        private static bool isStringComment(string s)
        {
            int count = s.Split(DELIMIT).Length - 1; //how many "DELIMIT" occurs in the string
            stringCommentFlag = (stringCommentFlag + count) % 2;


            if (stringCommentFlag == 1 || count > 0)
                return true;
            else
                return false;

        }




        // Given a charactor, if it is part of comment retunn TRUE, otherwise FALSE
        private static bool isCharComment(char ch)
        {
            if (DELIMIT == ch)
                charCommentFlag = (charCommentFlag + 1) % 2;

            if (charCommentFlag == 1 || ch == DELIMIT)
                return true;
            else
                return false;
        }



        //===============================================================================================
        //===============================================================================================
        //===============================================================================================
        //when used, remove any "#" in the parameter configuration file, attention



        private static char read_firstchar_nonspace(StreamReader sr)
        {
            while (true) //remove the possible blanks in the beginning 
            {
                char c = (char)sr.Read();

                if (char.IsWhiteSpace(c))
                    continue;
                else
                {
                    // Console.WriteLine("nonspace {0}  {1}", (int)c, c);
                    return c;
                }
            }
        }


        public static int read_int(StreamReader sr)
        {
            char[] content = new char[32];

            content[0] = read_firstchar_nonspace(sr);

            int count = 1;

            while (true)
            {
                //char c = (char)sr.Peek();
                int tmp_ret_val = sr.Peek();

                if (-1 == tmp_ret_val)//end of file
                    break;

                char c = (char)tmp_ret_val;

                if (char.IsDigit(c))
                {
                    sr.Read();
                    content[count] = c;
                    count++;

                }
                else
                {
                    break;
                }
            }

            string str_val = new string(content);
            int ret_val;
            try
            {
                ret_val = Convert.ToInt32(str_val);
            }
            catch
            {
                Console.WriteLine("convert to int failure: the content is {0}", content);
                throw new Exception();
            }

            return ret_val;
        }



        public static float read_float(StreamReader sr)
        {
            char[] content = new char[32];

            content[0] = read_firstchar_nonspace(sr);

            int count = 1;
            int point = 0;

            while (true)
            {
                //char c = (char)sr.Peek();
                int tmp_ret_val = sr.Peek();

                if (-1 == tmp_ret_val)//end of file
                    break;

                char c = (char)tmp_ret_val;


                if ('.' == c)
                {
                    sr.Read();
                    content[count] = c;
                    count++; point++;
                    //Console.WriteLine("{0}  {1}", (int)c, c);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    sr.Read();
                    content[count] = c;
                    count++;
                    //Console.WriteLine("{0}  {1}", (int)c, c);
                }
                else
                {
                    //Console.WriteLine("float {0}  {1}", (int)c, c);
                    break;
                }


            }

            string str_val = new string(content);
            float ret_val;
            try
            {
                ret_val = Convert.ToSingle(str_val);
            }
            catch
            {
                Console.WriteLine("convert to float failure: the content is {0}", content);
                throw new Exception();
            }

            return ret_val;
        }



        //the length of the string is of 256 chars at most
        public static string read_string(StreamReader sr)
        {
            char[] content = new char[256];

            content[0] = read_firstchar_nonspace(sr);

            int count = 1;

            while (true)
            {
                int ret_val = sr.Peek();
                
                if (-1 == ret_val)//end of file
                    break;

                char c = (char)ret_val;

                //do not know how to process "#" , so remove "#" in the file please, otherwise will be problematic
                if (char.IsWhiteSpace(c))
                    break;

                sr.Read();
                content[count] = c;
                count++;
                //                Console.WriteLine("string {0}  {1}   {2}", (int)c, c, count);
            }

            char[] new_content = new char[count];
            Array.Copy(content, 0, new_content, 0, count);


            string str_val;
            try
            {
                str_val = new string(new_content);
            }
            catch
            {
                Console.WriteLine("convert to string failure: the content is {0}", content);
                throw new Exception();
            }

            return str_val;
        }
        //===============================================================================================
        //===============================================================================================
        //===============================================================================================


        //This will skip all white space and the comment delimited by # pairs in infile.
        public static void skipblanks(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                char ch = (char)sr.Peek();

                if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\v' || ch == '\f' || ch == '\r' || (isCharComment(ch)))
                {
                    ch = (char)sr.Read();
                    continue;
                }
                else
                    break;
            }
        }




        //This will return TRUE if eof is encountered and FALSE otherwise.  It is
        //different from the feof library function in that it skips all white space
        //and the comment delimited by # pairs before detecting the end of file condition.
        public static bool LDeof(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                char ch = (char)sr.Peek();

                if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\v' || ch == '\f' || ch == '\r' || (isCharComment(ch)))
                {
                    ch = (char)sr.Read();
                    continue;
                }
                else
                    break;
            }

            if (!sr.EndOfStream)
            {
                return false; // Console.WriteLine("not end of file");
            }
            else
            {
                return true; // Console.WriteLine("end of file");
            }

        }


        //Returns minimun of a and b.
        public static int min(int a, int b)
        {
            if (a < b)
                return a;
            else
                return b;
        }


        //Returns minimum of a and b.
        public static float LDmin(float a, float b)
        {
            if (a < b)
                return a;
            else
                return b;
        }


        //Returns maximum of a and b.
        public static int max(int a, int b)
        {
            if (a > b)
                return a;
            else
                return b;
        }



        //Returns maximum of a and b.
        public static float LDmax(float a, float b)
        {
            if (a > b)
                return a;
            else
                return b;
        }



        //This will seed the random number generator.
        public static void fseed(int seed)
        {
            srand(seed);
            //fixRand = new Random(seed); //srand(seed);
            //Console.WriteLine("Seed {0}", seed);
        }        

        //This will return a random number.
        public static float frand()
        {
            //Console.WriteLine("frand()");
            //return (float)fixRand.NextDouble();
            //return 0.5f;
            return (float)rand() / (float)RAND_MAX;
        }




        //add to replace all (float)((double)rand()/(double)(RAND_MAX+1)) in fuelsites.cpp
        public static float frand1()
        {
            //Console.WriteLine("frand1()");
            //return (float)fixRand.NextDouble();
            //return 0.5f;
            return (float)((double)rand() / (double)(RAND_MAX + 1));
        }



        //Add to replace all rand() in BDA.cpp
        public static int frandrand()
        {
            //Console.WriteLine("frandrand()");
            return rand();
            //return fixRand.Next();
            //return 1;
        }



        public static float frandrandSuccesstion(int timestep, char argu)
        {            
            float sign = (float)argu;

            //float result = (float)fixRand.NextDouble();
            float result = (float)rand() / (float)RAND_MAX;
            
            float variation = frand1() / 2 * sign;

            result = result / (1 + variation);
            result = result * 10 / timestep;

            return result;
        }


        //This will return a random number.
        public static double drand()
        {
            //Console.WriteLine("drand()");
            //return fixRand.NextDouble();
            //return 0.5f;
            return (double)rand() / (double)RAND_MAX;
        }



        //This will return a random integer between a and b inclusive.
        public static int irand(int a, int b)
        {
            //Console.WriteLine("irand()");
            // return (int)(fixRand.Next() % (b - a + 1) + a);
            //return fixRand.Next(a, b);
            return (int)(rand() % (b - a + 1) + a);
        }



        //This returns a random number normally distributed around 0.0.
        public static float frandNorm()
        {
            //Console.WriteLine("frandNorm()");
            float a1 = 0, a2 = 0;

            while (0 == a1)
            {
                a1 = frand();
                a2 = frand();
            }

            double first_item = Math.Sqrt(-0.75 * Math.Log(a1)) * Math.Sin(PI * PI * a2);

            return (float)first_item - 0.14f;
        }





        public static float franCos()
        {
            //Console.WriteLine("franCos()");
            float a1 = frand();

            return (float)Math.Cos(PI * PI * a1);
        }





        //This will swap the values of a and b.
        public static void swap(ref float a, ref float b)
        {
            float t;

            t = a;
            a = b;
            b = t;
        }





        public static int factorial(int k)
        {
            if (k == 0)
                return 1;

            int tmp = 1;

            for (int i = k; i >= 1; i--)
                tmp *= i;

            return tmp;
        }



        // int LDfread(char* dest, int i, int j, FILE* f)
        // {
        // 	size_t num = fread(dest, i, j, f);
        // 	assert(num <= INT_MAX && num >= INT_MIN);

        // 	return (int)num;


        // 	byte[] buffer = new byte[8192];
        // 	int bytesRead = stream.Read(buffer, 0, buffer.Length);
        // }


        
        public static string LDfgets(StreamReader sr)
        {
            // return fgets(str, n, fstream);
            return sr.ReadLine();
        }


        //might need to convert the value to char
        public static int LDfgetc(StreamReader sr)
        {
            int FirstChar = sr.Read();

            //in the original c++ version, the end of a line is only 10, but in c#, it is 13&10
            //therefore, here 13 will be removed
            while (13 == FirstChar)
            {
                FirstChar = sr.Read();
            }

            return FirstChar;
        }



        

#if __HARVEST__
		float gasdev()
		{
			static int iset = 0;

			static float gset;

			float fac,rsq,v1,v2;


			if  (iset == 0) 
			{
				do {

					v1 = 2.0f * frand() - 1.0f;
					v2 = 2.0f * frand() - 1.0f;

					rsq = v1 * v1 + v2 * v2;

				} while (rsq >= 1.0 || rsq == 0.0);

				fac = (float)Math.Sqrt(-2.0 * Math.Log(rsq) / rsq);

				gset = v1 * fac;

				iset = 1;

				return v2 * fac;
			} 
			else 
			{
				iset=0;

				return gset;
			}

		}



		float gasdev(float mean, float sd)
		{
			float gset = gasdev() * sd + mean;

			return gset;
		}
#endif

    }
}
