using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO.Compression;
using System.Xml.Schema;
using System.Reflection;
using System.Net;
using System.Linq.Expressions;

namespace DCS_Radio_Mission_Mod
{
    class Program
    {
        static readonly string path = Directory.GetCurrentDirectory();
        static readonly string[] A10Radios = { "VHF_AM_RADIO", "UHF_RADIO", "VHF_FM_RADIO" };

        // Cosmetic function: remove all whitespaces before first character
        static string RemoveLeadingSpace(string s)
        {
            while (s.StartsWith(" ")) { s = s.Substring(1); }
            return s;
        }

        // Error display
        static void ExitWithNotFound()
        { 
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Node/Lua couldn't be found. Aborting. x_X");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit this window...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        // Core function for frequencies replacement
        static string[] ReplaceRadioPreset(string[] arr, XmlDocument template, ConsoleKeyInfo side)
        {
            XmlElement root = template.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("aircraft");
            string wpath = "", defaultFreq="";
            string[] a = arr;
            int units, skill, name, presets, type, radios, sortie, coalition, coalitionBlue, coalitionRed, lastLine;
            units = skill = name = presets = type = radios = sortie = coalition = coalitionBlue = coalitionRed = lastLine = 1;
            Regex RegUnit = new Regex("\\[\\d\\]");
            Regex RegClient = new Regex("\\[\\\"skill\\\"\\]\\s*=\\s*\\\"Client\\\"");
            Regex RegFreq = new Regex("\\d+(\\.\\d+)?(?=,|(\r\n))"); // look for at least 1 digit or digits.digits, directly followed by a , or a \r\n
            int count = 0;

            foreach (XmlNode node in nodes)
            {
                count = 0;
                // Look forward through the string[] to find the matching field
                coalition = Array.FindIndex(a, s => s.Contains("[\"coalition\"]")); // locate ["coalition"]
                // Find the upper boundary to skip the non-coalition fields
                sortie = Array.FindIndex(a, s => s.Contains("[\"sortie\"]"));

                // Find Coalitions boundaries
                coalitionBlue = Array.FindIndex(a, coalition, sortie - coalition, s => s.Contains("[\"blue\"]"));   // Locate ["blue"]
                coalitionRed = Array.FindIndex(a, coalition, sortie - coalition, s => s.Contains("[\"red\"]")); // Locate ["red"]
                if (side.Key == ConsoleKey.B)
                {
                    // BLUE: Force upper bound to "red" line
                    sortie = coalitionRed;
                }
                else
                {
                    // RED: Force lower bound to "red" line
                    coalition = coalitionRed;
                }

                lastLine = coalition;

                while (Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[\"country\"]")) != -1)
                {
                    lastLine = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[\"country\"]")); // Locate Next ["country"]
                    units = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[\"units\"]"));    // Locate ["units"]
                    if (units != -1)
                    {
                        lastLine = units;
                        while (Array.FindIndex(a, lastLine, sortie - lastLine, s => RegUnit.Match(s).Success) != -1)
                        {
                            lastLine = Array.FindIndex(a, lastLine, sortie - lastLine, s => RegUnit.Match(s).Success);
                            skill = Array.FindIndex(a, lastLine, sortie - lastLine, s => RegClient.Match(s).Success);    // Check Skill setting
                            if (skill != -1)
                            {
                                lastLine = skill;
                                type = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[\"type\"]"));    // Locate ["type"]
                                if (type != -1)
                                {
                                    lastLine = type;
                                    name = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("\"" + node.Attributes["name"].Value + "\""));
                                    if ( name != -1)
                                    {
                                        lastLine = name;
                                        // Aircraft is found in mission file
                                        count++;
                                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.WriteLine(@"¤¤¤¤¤ " + node.Attributes["name"].Value + @" #" + count.ToString() + @" ¤¤¤¤¤");
                                        Console.ResetColor();
                                        if (node.Attributes["name"].Value == "A-10C")
                                        {
                                            // Création de fichiers spécifiques au A-10C
                                            foreach (XmlNode radio in node.ChildNodes)
                                            {
                                                switch (radio.Attributes["designation"].Value)
                                                {
                                                    case "VHF/AM":
                                                        wpath = path + @"\" + A10Radios[0];
                                                        defaultFreq = "124000000";
                                                        Console.Write("|  VHF/AM  |  ");
                                                        break;
                                                    case "UHF":
                                                        wpath = path + @"\" + A10Radios[1];
                                                        defaultFreq = "251000000";
                                                        Console.Write("UHF  |  ");
                                                        break;
                                                    case "VHF/FM":
                                                        wpath = path + @"\" + A10Radios[2];
                                                        defaultFreq = "30000000";
                                                        Console.Write("VHF/FM");
                                                        break;
                                                    default:
                                                        Console.WriteLine("Radio designations incorrect or missing for the A-10C.");
                                                        wpath = "";
                                                        break;
                                                }
                                                if (wpath != "")
                                                {
                                                    if (!Directory.Exists(wpath))
                                                    {
                                                        Directory.CreateDirectory(wpath);
                                                    }
                                                    File.WriteAllText(wpath + @"\SETTINGS.lua", "-- A-10C " + radio.Attributes["designation"].Value + " Radio settings\n"
                                                        + "settings=\n{\n\t{\n\t\t\t[\"mode_dial\"]=0,\n\t\t\t[\"manual_frequency\"]=" + defaultFreq + ",\n\t\t\t[\"selection_dial\"]=0,\n"
                                                        + "\t\t\t[\"channel_dial\"]=0,\n\t},\n\t[\"presets\"]=\n\t{\n\t\t");

                                                    foreach (XmlNode preset in radio.ChildNodes)
                                                    {
                                                        File.AppendAllText(wpath + @"\SETTINGS.lua", "\t\t\t[" + preset.Attributes["nbr"].Value + "]=" + preset.InnerText + ",\n");
                                                        //DBG:Console.WriteLine("[" + preset.Attributes["nbr"].Value + "]=>" + preset.InnerText);
                                                    }
                                                    File.AppendAllText(wpath + @"\SETTINGS.lua", "\t\t},\n}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            radios = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[\"Radio\"]"));   // Locate ["Radio"]
                                            if (radios != 1)
                                            {
                                                lastLine = radios;
                                                Console.Write("|  ");
                                                foreach (XmlNode radio in node.ChildNodes)
                                                {
                                                    lastLine = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[" + radio.Attributes["script-nbr"].Value + "]"));    // Match the radio number
                                                    Console.Write(radio.Attributes["designation"].Value + "  |  ");
                                                    lastLine = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[\"channels\"]"));    // Has to follow ["channels"]
                                                    foreach (XmlNode preset in radio.ChildNodes)
                                                    {
                                                        presets = Array.FindIndex(a, lastLine, sortie - lastLine, s => s.Contains("[" + preset.Attributes["nbr"].Value + "]"));  // Match the channel number
                                                        //DBG:Found the preset, display old version
                                                        //DBG:Console.Write(RemoveLeadingSpace(a[presets]) + " => ");
                                                        a[presets] = RegFreq.Replace(a[presets], preset.InnerText); // replace with new frequency
                                                        //DBG:Display modified version
                                                        //DBG:Console.WriteLine(RemoveLeadingSpace(a[presets]));
                                                    }
                                                }
                                            }
                                        }
                                        lastLine++;
                                        Console.WriteLine("\n");
                                    }
                                }
                            }
                            lastLine++;
                        }
                    }
                    lastLine++;
                }
                if (count == 0)
                {
                    // XML Aircraft not found in mission file. Normal behaviour.
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("no client " + node.Attributes["name"].Value + " unit found.\n");
                    Console.ResetColor();
                }
            }
            return a;

        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: {0}", e.Message);
                    Console.ResetColor();
                    Console.WriteLine("Press any key to exit this window...");
                    Console.ReadKey();
                    Environment.Exit(0);
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    break;
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.BackgroundColor = ConsoleColor.Cyan;
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("=-=-=-=-=-=-= RADIO PRESETS BATCH REPLACEMENT =-=-=-=-=-=-=".PadRight(Console.WindowWidth - 1));
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("coded by Romain 'Dusty' T.");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                // Check if any argument given
                if (args.Length != 0)
                {
                    string zipPath = args[0];
                    string template_file;
                    // Check if 2nd argument given, otherwise use default template file
                    if (args.Length == 2) { template_file = args[1]; } else { template_file = path + @"\template.xml"; }

                    // Check that all files exists and have the expected extension
                    if (File.Exists(zipPath))
                    {
                        if (Path.GetExtension(zipPath) == ".miz")
                        {
                            if (File.Exists(template_file))
                            {
                                if (Path.GetExtension(template_file) == ".xml")
                                {

                                    // Validate the XML Template
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.Write("\nValidating the XML template...");

                                    // Get the assembly that contains the embedded xsd file
                                    var assembly = Assembly.GetExecutingAssembly();
                                    var stream = assembly.GetManifestResourceStream("DCS_Radio_Mission_Mod.template.xsd");
                                    // Load it into a XmlReader, add it to the settings and attach it to the XML Reader
                                    XmlReader schemaReader = XmlReader.Create(stream);
                                    XmlReaderSettings settings = new XmlReaderSettings();
                                    settings.Schemas.Add("", schemaReader);
                                    settings.ValidationType = ValidationType.Schema;
                                    XmlReader reader = XmlReader.Create(template_file, settings);

                                    // Exception handler if Validation fails
                                    ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

                                    // Load XML with previous settings, and validate it
                                    XmlDocument template = new XmlDocument();
                                    template.Load(reader);
                                    template.Validate(eventHandler);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(" => OK!");
                                    Console.ResetColor();

                                    Console.WriteLine("\nChoose side (coalition) on which you'd like to apply the presets template:\n\t- Blue: type 'b'\n\t- Red: type 'r'");
                                    ConsoleKeyInfo side;
                                    do
                                    {
                                        side = Console.ReadKey();
                                    }
                                    while (side.Key != ConsoleKey.R && side.Key != ConsoleKey.B);
                                    Console.WriteLine("");
                                    //
                                    // Now we get to work with the MIZ File, extracting the mission file and modifying it before zipping it back with added A-10C files
                                    //
                                    string mission_file = path + @"\mission";
                                    using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
                                    {
                                        // Extract 'mission' file
                                        ZipArchiveEntry entry = archive.GetEntry("mission");
                                        entry.ExtractToFile(mission_file, true);

                                        // Store the file in a string[] and replace the fields defined by the template
                                        string[] missionA = File.ReadAllLines(mission_file);

                                        // Core Function to parse and change the string[] according to XML template
                                        string[] missionA_mod = ReplaceRadioPreset(missionA, template, side);

                                        // Changes are done, write the array back to the file
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("~~ Overwriting mission file");
                                        File.WriteAllLines(mission_file, missionA_mod);

                                        // Zip it back up, replacing existing
                                        entry.Delete();
                                        archive.CreateEntryFromFile(mission_file, "mission");
                                        // Add A-10C folder structure
                                        ZipArchiveEntry existing = null;
                                        foreach (string f in A10Radios)
                                        {
                                            // !!!IMPORTANT!!!: WinRAR doesn't give 2 cts if the directory is a forward or backward slash, it will display the same folder
                                            // DCS DOES CARE: It looks for a #RADIO#/SETTINGS.lua folder with a FORWARD SLASH /
                                            existing = archive.GetEntry(f + @"/SETTINGS.lua");
                                            if (existing != null)
                                            {
                                                existing.Delete();
                                            }
                                            if (File.Exists(path + @"\" + f + @"\SETTINGS.lua"))
                                            {
                                                Console.WriteLine("~~ Creating A-10C presets file for " + f);
                                                archive.CreateEntryFromFile(path + @"\" + f + @"\SETTINGS.lua", f + @"/SETTINGS.lua");
                                            }
                                            // Dispose of temp folders
                                            if (Directory.Exists(path + @"\" + f))
                                            {
                                                Directory.Delete(path + @"\" + f, true);
                                            }
                                        }

                                        // Dispose of temp mission file
                                        Console.WriteLine("~~ Cleaning up");
                                        File.Delete(mission_file);
                                    }
                                }
                                else
                                {
                                    // ERROR: template isn't an xml file
                                    Console.WriteLine("Template file is not valid. It should be an *.xml file.");
                                }
                            }
                            else
                            {
                                // ERROR: Template file doesn't exist
                                Console.WriteLine("Template file not found.");
                            }
                        }
                        else
                        {
                            // ERROR: Zip file is not a *.miz extension
                            Console.WriteLine("Mission file is not valid. It should be a *.miz file.");
                        }
                    }
                    else
                    {
                        // ERROR: Zip file doesn't exist
                        Console.WriteLine("Mission *.miz file not found.");
                    }
                }
                else
                {

                    // ERROR: No argument given
                    Console.WriteLine("Path to Mission *.miz file is needed as argument.");
                }
                // Display results before exiting
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("== ALL DONE!");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit this window...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException) { ExitWithNotFound(); }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Aborting. x_X");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to exit this window...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
        }
    }
}
