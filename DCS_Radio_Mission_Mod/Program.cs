using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace DCS_Radio_Mission_Mod
{
    class Program
    {
        static void Main(string[] args)
        {

            foreach (string X in Y)
            {
                Console.WriteLine("");

                // F-14

                string file =  "mission";
                string search = "";
                string replace = "";
                File.WriteAllText(file, File.ReadAllText(file).Replace(search, replace));
                
            }

        }
    }
}
