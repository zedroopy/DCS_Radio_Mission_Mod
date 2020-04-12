using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO.Compression;


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

        static string[] ReplaceRadioPreset(string[] a, string template_file)
        {
            XmlDocument template = new XmlDocument();
            template.Load(template_file);
            XmlElement root = template.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("aircraft");
            foreach (XmlNode node in nodes)
            {
                // Look forward through the string[] to find the matching field
                int coalition = Array.FindIndex(a, s => s.Contains("[\"coalition\"]")); // Has to follow ["coalition"]
                int country = Array.FindIndex(a, coalition, s => s.Contains("[\"country\"]"));  // Has to follow ["country"]
                int units = Array.FindIndex(a, country, s => s.Contains("[\"units\"]"));    // Has to follow ["units"]
                int type = Array.FindIndex(a, units, s => s.Contains("[\"type\"]"));    // Has to follow ["type"]

                int name = Array.FindIndex(a, type, s => s.Contains("\"" + node.Attributes["name"].Value + "\"")); // Match the aircraft type
                
                foreach (XmlNode radio in node.ChildNodes)
                {
                    int radios = Array.FindIndex(a, name, s => s.Contains("[\"Radio\"]"));   // Has to follow ["Radio"]
                    int radio_nbr = Array.FindIndex(a, radios, s => s.Contains("[" + radio.Attributes["script-nbr"].Value + "]"));    // Match the radio number
                    foreach (XmlNode preset in radio.ChildNodes)
                    {
                        int channels = Array.FindIndex(a, radio_nbr, s => s.Contains("[\"channels\"]"));    // Has to follow ["channels"]
                        int presets = Array.FindIndex(a, channels, s => s.Contains("[" + preset.Attributes["nbr"].Value + "]"));  // Match the channel number

                        // Found the preset, display old version
                        Console.Write(RemoveLeadingSpace(a[presets]) + " => ");
                        Regex regex = new Regex("\\d+(?=,|(\r\n))"); // look for at least 1 digit which is directly followed by a , or a \r\n
                        a[presets] = regex.Replace(a[presets], preset.InnerText); // replace with new frequency
                        // Display modified version
                        Console.WriteLine(RemoveLeadingSpace(a[presets]));
                    }
                }
            }
            
            return a;
        }

        static void Main(string[] args)
        {

            // TO-DO: input mission file as args[0]
            // OPTION TO-DO: input template file as args[1]
            string mission_file = @"C:\Users\ze_dr\source\repos\DCS_Radio_Mission_Mod\DCS_Radio_Mission_Mod\mission";
            string template_file = @"C:\Users\ze_dr\source\repos\DCS_Radio_Mission_Mod\DCS_Radio_Mission_Mod\template.xml";

            Console.WriteLine("==== Radio presets batch replacement ====");

            string zipPath = @"C:\Users\ze_dr\source\repos\DCS_Radio_Mission_Mod\DCS_Radio_Mission_Mod\Test Fichier Mission.miz";

            ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
            ZipArchiveEntry entry = archive.GetEntry("mission");
            entry.ExtractToFile(mission_file,true);

            //StreamWriter writer = new StreamWriter(entry.Open());
            //writer.WriteLine("");
            //entry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
            

           

            // Store the file in a string[] and replace the fields defined by the template
            string[] missionA = File.ReadAllLines(mission_file);
            ReplaceRadioPreset(missionA, template_file);

            // Write the array back to the file
            File.WriteAllLines(mission_file, missionA);
            // Zip it back up
            entry.Delete();
            //archive.CreateEntryFromFile(mission_file, "mission");
            File.Delete(mission_file);

           // Display results before exiting
            Console.WriteLine("Appuyez sur une touche pour quitter...");
            Console.ReadLine();
        }
    }
}
