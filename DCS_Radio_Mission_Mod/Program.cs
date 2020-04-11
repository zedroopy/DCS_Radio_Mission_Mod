using System;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace DCS_Radio_Mission_Mod
{
    class Program
    {
        // Cosmetic function: remove all whitespaces before first character
        static string RemoveLeadingSpace(string s)
        {
            while (s.StartsWith(" ")) { s = s.Substring(1); }
            return s;
        }

        static string[] ReplaceRadioPreset(string[] a, Modif m)
        {
            // Look forward through the string[] to find the matching field
            int coalition = Array.FindIndex(a, s => s.Contains("[\"coalition\"]")); // Has to follow ["coalition"]
            int country = Array.FindIndex(a, coalition, s => s.Contains("[\"country\"]"));  // Has to follow ["country"]
            int units = Array.FindIndex(a, country, s => s.Contains("[\"units\"]"));    // Has to follow ["units"]
            int type = Array.FindIndex(a, units, s => s.Contains("[\"type\"]"));    // Has to follow ["type"]

            int name = Array.FindIndex(a, type, s => s.Contains("\""+m.type+"\"")); // Match the aircraft type
            int radio = Array.FindIndex(a, name, s => s.Contains("[\"Radio\"]"));   // Has to follow ["Radio"]

            int radio_nbr = Array.FindIndex(a, radio, s => s.Contains("["+m.radio+"]"));    // Match the radio number
            int channels = Array.FindIndex(a, radio_nbr, s => s.Contains("[\"channels\"]"));    // Has to follow ["channels"]
            int presets = Array.FindIndex(a, channels, s => s.Contains("["+m.preset+"]"));  // Match the channel number

            // Found the preset, display old version
            Console.Write(RemoveLeadingSpace(a[presets]) + " => ");

            Regex regex = new Regex("\\d+(?=,|(\r\n))"); // look for at least 1 digit which is directly followed by a , or a \r\n
            a[presets] = regex.Replace(a[presets], m.freq); // replace with new frequency

            // Display modified version
            Console.WriteLine(RemoveLeadingSpace(a[presets]));
            return a;
        }

        public struct Modif
        {
            public string type { get; set; }
            public string radio { get; set; }
            public string preset { get; set; }
            public string freq { get; set; }
        }

        static Modif ParseXML(string[] xml)
        {
            Modif m = new Modif();

            // TO-DO: Parse XML Template file to fill struct 'Modif'

            // Debug const values:
            m.type="F-14B";
            m.radio = "1";
            m.preset = "1";
            m.freq = "200";
            // End Debug

            return m;
        }

        static void Main(string[] args)
        {

            // TO-DO: input mission file as args[0]
            // OPTION TO-DO: input template file as args[1]

            Console.WriteLine("==== Radio presets batch replacement ====");

            string mission_file = "C:\\Users\\ze_dr\\source\\repos\\DCS_Radio_Mission_Mod\\DCS_Radio_Mission_Mod\\debug_mission";
            string template_file = "C:\\Users\\ze_dr\\source\\repos\\DCS_Radio_Mission_Mod\\DCS_Radio_Mission_Mod\\debug_template.xml";
            
            // Parsing XML Template to fill in STRUCT 'change'
            Modif change = ParseXML(File.ReadAllLines(template_file));

            // Store the file in a strin[] and replace the fields defined by 'change' in it
            string[] missionA = File.ReadAllLines(mission_file);
            ReplaceRadioPreset(missionA, change);

            // Write the array back to the file
            File.WriteAllLines(mission_file, missionA);

           // Display results before exiting
            Console.WriteLine("Appuyez sur une touche pour quitter...");
            Console.ReadLine();
        }
    }
}
