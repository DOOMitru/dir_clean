using System;
using System.Collections.Generic;
using System.IO;

namespace dir_clean
{
    class Program
    {
        static Dictionary<string, bool> options = new Dictionary<string, bool>();
        static int passes = 5;
        static int pass = 0;
        static bool removed = true;

        static void CleanDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);
            
            // Check if the current directory is empty
            if (files.Length == 0 && dirs.Length == 0)
            {
                // No other files or directories found, so it must be empty
                if(options["confirm"])
                {
                    Console.Write("{0} is empty. Remove? Y/N: ", directory);
                    var response = Console.ReadKey();
                    Console.WriteLine();

                    if(response.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine("DELETING: {0}", directory);

                        // Delete it
                        Directory.Delete(directory, false);
                    }
                }
                else
                {
                    if(options["verbose"])
                        Console.WriteLine("DELETING: {0}", directory);

                    // Delete it
                    Directory.Delete(directory, false);
                }

                removed = true;
                return;
            }

            // Recursively clean each subdirectory
            foreach (var dir in dirs)
            {
                CleanDirectory(dir);
            }
        }

        static void PrintUsage()
        {
            string name = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.WriteLine("\nThis program will recursively remove all empty directories within given directories");
            Console.WriteLine("If no directories provided, it will try to clean the current directory\n");
            Console.WriteLine("Usage: {0} [-hvcs[p N]] [DIR1 DIR2 DIR3 ...]");
            Console.WriteLine("  OPTIONS:");
            Console.WriteLine("    -h = help       display this help and exit");
            Console.WriteLine("    -v = verbose    show directories being removed");
            Console.WriteLine("    -c = confirm    ask for confirmation before deleting directory");
            Console.WriteLine("    -s = suppress   supress warnings about cleaning current directory");
            Console.WriteLine("    -p = passes     number of passes to make. Default is 5");
        }

        static List<string> ProcessArguments(string[] args)
        {
            List<string> directories = new List<string>();
            int start = 0;

            if (args.Length > 0)
            {
                string opts = args[0].ToLower();

                if(opts.StartsWith("-"))
                {
                    if (opts.Contains("h"))
                        options["help"] = true;

                    if (opts.Contains("v"))
                        options["verbose"] = true;

                    if (opts.Contains("c"))
                        options["confirm"] = true;

                    if (opts.Contains("s"))
                        options["supress"] = true;

                    if (opts.Contains("p"))
                    {
                        if (args.Length < 2)
                        {
                            // problem: N was not passed
                            options["help"] = true;
                            Console.WriteLine("-p requires N = number of passes");
                            return directories;
                        }

                        try
                        {
                            passes = int.Parse(args[1]);
                        }
                        catch (Exception e)
                        {
                            passes = -1;
                        }

                        if (passes <= 0)
                        {
                            options["help"] = true;
                            Console.WriteLine("N = number of passes must be greater than zero.");
                            return directories;
                        }

                        start = 2;
                    }
                    else start = 1;
                }
            }

            for (int i = start; i < args.Length; i++)
                directories.Add(args[i]);

            return directories;
        }

        static void Main(string[] args)
        {
            options.Add("help", false);
            options.Add("confirm", false);
            options.Add("verbose", false);
            options.Add("supress", false);
            
            string current = Directory.GetCurrentDirectory();
            List<string> directories = ProcessArguments(args);

            if (options["help"])
            {
                PrintUsage();
                return;
            }

            if (directories.Count == 0)
            {
                if (!options["supress"])
                {
                    Console.Write("Are you sure you want to clean the current directory? Y/N: ");
                    var response = Console.ReadKey();
                    Console.WriteLine();

                    if (response.Key == ConsoleKey.Y)
                    {
                        for (pass = 0; pass < passes && removed; pass++)
                        {
                            removed = false;
                            Console.WriteLine("PASS #{0}: Cleaning current directory...", pass + 1);
                            CleanDirectory(current);
                        }
                    }
                    else
                    {
                        PrintUsage();
                        return;
                    }
                }
                else
                {
                    for (pass = 0; pass < passes && removed; pass++)
                    {
                        if (options["verbose"])
                            Console.WriteLine("PASS #{0}: Cleaning current directory...", pass + 1);
                        removed = false;
                        CleanDirectory(current);
                    }
                }
            }
            else
            {
                for (pass = 0; pass < passes && removed; pass++)
                {
                    foreach (string directory in directories)
                    {
                        // If given just the directory name, 
                        // look for directory in current directory
                        // by building the absolute path
                        string path = Path.Combine(current, directory);

                        if (Directory.Exists(path))
                        {
                            // relative path to current directory
                            if (options["verbose"])
                                Console.WriteLine("PASS #{0}: Cleaning directory {1}... ", pass + 1, path);

                            removed = false;
                            CleanDirectory(path);
                        }
                        else if (Directory.Exists(directory))
                        {
                            // absolute path given as argument
                            if (options["verbose"])
                                Console.WriteLine("PASS #{0}: Cleaning directory {1}... ", pass + 1, path);

                            removed = false;
                            CleanDirectory(directory);
                        }
                        else if (options["verbose"])
                            Console.WriteLine("{0} is not a directory...", directory);
                    }
                }
            }
            
            if (options["verbose"])
            {
                if (pass >= passes)
                {
                    Console.WriteLine("All {0} passes required. There may be some empty directories remaining...", pass);
                }
                else
                {
                    Console.WriteLine("Only {0} pass(es) needed...", pass);
                }

                Console.WriteLine("Done. Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
