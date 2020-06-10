using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cptf
{
    class Program
    {
        public static string Status { get; private set; }

        static void Main(string[] args)
        {
            // Settings
            Properties.Settings1 Settings = Properties.Settings1.Default;

            // Identify the application so we can log it
            String AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            // Copy Paramters
            CopyParameters copyParameters = new CopyParameters();
            copyParameters.DestinationRootDir = Settings.DestinationRootDir;
            copyParameters.TestDataRepoRootDir = Settings.TestDataRepoRootDir;

            LogHelper.Log(LogLevel.INFO, "Starting " + AppName + " " + AppVersion.ToString());

            try
            {
                var parser = new CommandLine();
                parser.Parse(args);

                if (parser.Arguments.Count > 0)
                {

                    // get test data
                    if (parser.Arguments.ContainsKey("testdata"))
                    {
                        copyParameters.Name = parser.Arguments["testdata"][0];
                    };

                    // Get project parameter
                    if (parser.Arguments.ContainsKey("project"))
                    {
                        copyParameters.Project = parser.Arguments["project"][0];
                    };

                    LogHelper.Log(LogLevel.INFO, string.Format("Running cptf.exe -testdata \"{0}\" -project \"{1}\"", copyParameters.Name, copyParameters.Project));

                    // If any of the parameter is not specified exit
                    if (String.IsNullOrWhiteSpace(copyParameters.Name) || String.IsNullOrWhiteSpace(copyParameters.Project))
                    {
                        Usage();
                        LogHelper.Log(LogLevel.INFO, "Test data or Project incorrectly specified");
                        Environment.Exit(1);
                    };

                    // Start copy here
                    try
                    {
                        CopyTestData copyTestData = new CopyTestData();
                        copyTestData.Start(copyParameters);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }
                else
                {
                    Usage();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(LogLevel.ERROR, "Error in getting command-line parameters", ex);
            }
        }



        /// <summary>
        /// Show usage message in the console
        /// </summary>
        static void Usage()
        {
            Console.WriteLine("usage: cptf -testdata testdatadir -project projectname");
        }

    }
}
