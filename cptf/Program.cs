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
        static void Main(string[] args)
        {
            // Settings
            Properties.Settings1 Settings = Properties.Settings1.Default;

            // Identify the application so we can log it
            String AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            LogHelper.Log(LogLevel.INFO, "Starting " + AppName + " " + AppVersion.ToString());

            try
            {
                // Check settings
                LogHelper.Log(LogLevel.INFO,"Project source root directory setting = " + Settings.DestinationRootDir);
                LogHelper.Log(LogLevel.INFO,"Test data repository root directory setting = " + Settings.TestDataRepoRootDir);

                if ((!Directory.Exists(Settings.DestinationRootDir)) &
                        (!Directory.Exists(Settings.TestDataRepoRootDir)))
                {
                    Console.WriteLine("Check the application settings and make sure the specified folders exists.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(LogLevel.ERROR, "Error checking settings", ex);           
            }

            try
            {
                var parser = new CommandLine();
                parser.Parse(args);

                if (parser.Arguments.Count > 0)
                {
                    string testdataname = String.Empty;
                    string project = String.Empty;

                    // get test data
                    if (parser.Arguments.ContainsKey("testdata"))
                    {
                        testdataname = parser.Arguments["testdata"][0];
                    };

                    // Get project parameter
                    if (parser.Arguments.ContainsKey("project"))
                    {
                        project = parser.Arguments["project"][0];
                    };

                    LogHelper.Log(LogLevel.INFO,"Test data parameter set to " + testdataname);
                    LogHelper.Log(LogLevel.INFO, "Project parameter set to " + project);

                    // If any of the parameter is not specified exit
                    if (String.IsNullOrWhiteSpace(testdataname) || String.IsNullOrWhiteSpace(project))
                    {
                        Usage();
                        LogHelper.Log(LogLevel.INFO,"Test data or Project incorrectly specified");
                        Environment.Exit(1);
                    };

                    // Copy test data and make sure to handle CTRL-C and make sure the RoboCOpy
                    // thread is shutdown if CTRL-C is pressed.
                    TestData testdata = new TestData { Name = testdataname, Project = project, TestDataRepoRootDir = Settings.TestDataRepoRootDir, DestinationRootDir = Settings.DestinationRootDir };
                    try
                    {
                        LogHelper.Log(LogLevel.INFO, String.Format("Starting copy of {0} to {1}.", testdataname, project));
                        var task = testdata.Copy(); // Perform copy
                        var autoResetEvent = new AutoResetEvent(false);
                        Console.CancelKeyPress += (sender, eventArgs) =>
                        {
                            // cancel the cancellation to allow the program to shutdown cleanly
                            eventArgs.Cancel = true;
                            autoResetEvent.Set();
                            LogHelper.Log(LogLevel.INFO, "User cancelled copy.");
                        };

                        // main blocks here waiting for ctrl-C
                        autoResetEvent.WaitOne();
                        if (!task.IsCompleted)  // Check if RoboCopy is running
                        {
                            LogHelper.Log(LogLevel.INFO, "Shutting down RoboCopy task.");
                            testdata.RoboCopy.Stop();  // Stop RoboCopy
                        }
                        LogHelper.Log(LogLevel.INFO, "Now shutting down");
                        Console.WriteLine("Now shutting down");
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
            Console.WriteLine("usage: CopyTestFiles -testdata testdatadir -project projectname");
        }
    }
}
