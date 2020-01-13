using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

//corresponding to Defines.h in landis pro

namespace Landis.Extension.Succession.Landispro
{
    public static class defines
    {
        public const int MAX_SPECIES = 30;

        public const int MAX_LANDUNITS = 65535; //Changed on Jan 15 2009

        public static readonly int MAX_RECLASS = 50;


        public static readonly int FNSIZE = 256;


        public static readonly int G_SUCCESSION = 1;  //0x0000001 

        public static readonly int G_BDA = 2;  //0x0000010

        public static readonly int G_WIND = 4;  //0x0000100 

        public static readonly int G_HARVEST = 8;  //0x0001000

        public static readonly int G_FUEL = 16; //0x0010000

        public static readonly int G_FIRE = 32; //0x0100000

        public static readonly int G_BGROWTH = 64; //0x1000000

        public static readonly int G_FUELMANAGEMENT = 128;//0x10000000
    }
}
