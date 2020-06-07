using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyTestFiles
{
    class Program
    {

        static void Main(string[] args)
        {
            Properties.Settings1 Settings = Properties.Settings1.Default;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var parser = new CommandLine();
            parser.Parse(args);

            if (parser.Arguments.Count > 0)
            {
                string testdata = String.Empty;
                string project = String.Empty;

                // get test data
                if (parser.Arguments.ContainsKey("testdata"))
                {
                    testdata = parser.Arguments["testdata"][0];
                }

                // Get project parameter
                if (parser.Arguments.ContainsKey("project"))
                {
                    testdata = parser.Arguments["project"][0];
                }

                // If any of the parameter is not specified exit
                if (String.IsNullOrWhiteSpace(testdata) || String.IsNullOrWhiteSpace(project))
                {
                    Usage();
                    Environment.Exit(1);
                }

                // Make sure test data folder exists (rooted from test data root folder)
                String TestDataDir = System.IO.Path.Combine(Settings.TestDataRepoRootDir, testdata);


            }
            else
            {
                Usage();
            }
        }

        /// <summary>
        /// Show usage message in the console
        /// </summary>
        static void Usage()
        {
            Console.WriteLine("usage: CopyTestFiles -testdata testdatadir -project projectname");

        }
    }
}
