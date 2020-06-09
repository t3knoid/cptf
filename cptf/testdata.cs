using RoboSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace cptf
{
    /// <summary>
    /// Defines a delegate that is used for a callback from within the TestData class
    /// </summary>
    /// <param name="message"></param>
    public delegate void CopyTaskCompleteDelegate(string message);

    /// <summary>
    /// 
    /// </summary>
    public class CopyTestData
    {
        volatile bool run = true;
        TestData testData { get; set; }
        Task copyTask { get; set; }

        public CopyTestData()
        {  }
        /// <summary>
        /// Starts the copy
        /// </summary>
        /// <param name="p">Copy parameters</param>
        public void Start(CopyParameters p)
        {
            CopyTaskCompleteDelegate callback = new CopyTaskCompleteDelegate(CopyTaskComplete);
            // Copy test data and make sure to handle CTRL-C and make sure the RoboCOpy
            // thread is shutdown if CTRL-C is pressed.
            testData = new TestData
            {
                TaskCompleteCallBack = callback,
                CopyParameters = p,
            };

            try
            {
                run = true;
                LogHelper.Log(LogLevel.INFO, String.Format("Starting copy of {0} to {1}.", p.Name, p.Project));
                copyTask = testData.Copy(); // Perform copy
                Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress); // CTRL-C handler

                while (run)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Method that will be called by the callback delegate from within the TestData class
        /// </summary>
        /// <param name="message"></param>
        public void CopyTaskComplete(string message)
        {
            Console.WriteLine("Copy completed!");
            run = false;
        }
        /// <summary>
        /// Event that handles CTRL-C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            LogHelper.Log(LogLevel.INFO, "User pressed cancel.");
            if (copyTask.IsCompleted)
            {
                run = false;
                return;
            }
            else
            {
                LogHelper.Log(LogLevel.INFO, "Pausing copy.");
                testData.RoboCopy.Pause();
            }
            Console.WriteLine("\nCancel (Y/N)?");
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Y)
            {
                if (!copyTask.IsCompleted)  // Check if RoboCopy is running
                {
                    LogHelper.Log(LogLevel.INFO, "Shutting down RoboCopy task.");
                    testData.RoboCopy.Stop();  // Stop RoboCopy
                }
                Console.WriteLine("\n Copy aborted");
                LogHelper.Log(LogLevel.INFO, "User cancelled copy.");
                run = false;
            }
            else
            {
                LogHelper.Log(LogLevel.INFO, "Resuming copy.");
                Console.WriteLine("Resuming copy");
                if (testData.RoboCopy.IsPaused)
                {
                    testData.RoboCopy.Resume();
                }
                run = true;
            }

        }

    }
    /// <summary>
    /// A class that defines the test data. This includes methods
    /// that copies test data to a given destination
    /// </summary>
    public class TestData
    {
        public CopyParameters CopyParameters { get; set; }
        public RoboCommand RoboCopy { get; set; }
        public CopyTaskCompleteDelegate TaskCompleteCallBack { get; internal set; }

        public TestData()
        {
        }
        /// <summary>
        /// Checks if a folder exists
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        bool DirExists(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    LogHelper.Log(LogLevel.ERROR, "The folder is missing, " + dir);
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogHelper.Log(LogLevel.ERROR, "Error while checking if the folder exists, " + dir, ex);
                throw new Exception("Error while checking if the folder exists. " + dir);
            }
            return true;
        }
        /// <summary>
        /// The main method that performs the copy
        /// </summary>
        /// <returns></returns>
        public Task Copy()
        {
            if (!DirExists(CopyParameters.TestDataRepoRootDir))
            {
                throw new Exception("Test data repository root directory not found, " + CopyParameters.TestDataRepoRootDir);
            }

            // Append project name to destination folder
            string destinationDir = Path.Combine(CopyParameters.DestinationRootDir, CopyParameters.Project);

            if (!DirExists(destinationDir))
            {
                throw new Exception("Destination root directory not found, " + destinationDir);
            }

            // Append test data name to test data repository root
            string sourceDir = Path.Combine(CopyParameters.TestDataRepoRootDir, CopyParameters.Name);

            if (!DirExists(sourceDir))
            {
                throw new Exception("Test data directory to copy not found, " + sourceDir);
            }

            destinationDir = Path.Combine(destinationDir, CopyParameters.Name);   // Add the test data name to destination

            LogHelper.Log(LogLevel.INFO, "Source folder = " + sourceDir);
            LogHelper.Log(LogLevel.INFO, "Destination folder = " + destinationDir);

            RoboCopy = new RoboCommand();

            // events
            RoboCopy.OnFileProcessed += rbc_OnFileProcessed;
            RoboCopy.OnCopyProgressChanged += rbc_OnCopyProgressChanged;
            RoboCopy.OnCommandCompleted += rbc_OnCommandCompleted;
            // copy options

            RoboCopy.CopyOptions.MultiThreadedCopiesCount = 16;
            RoboCopy.CopyOptions.Source = sourceDir;
            RoboCopy.CopyOptions.Destination = destinationDir;
            RoboCopy.CopyOptions.CopySubdirectories = true;
            RoboCopy.CopyOptions.UseUnbufferedIo = true;
            RoboCopy.CopyOptions.Mirror = true;
            RoboCopy.CopyOptions.EnableRestartMode = true;
            RoboCopy.CopyOptions.UseUnbufferedIo = true;
            RoboCopy.CopyOptions.InterPacketGap = 0;
            // retry options
            RoboCopy.RetryOptions.RetryCount = 1;
            RoboCopy.RetryOptions.RetryWaitTime = 2;
            var cpTask = RoboCopy.Start();
            return cpTask;
        }
        /// <summary>
        /// Event thats called to provide copy progress of a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbc_OnCopyProgressChanged(object sender, CopyProgressEventArgs e)
        {
            Console.Write(".");
        }
        /// <summary>
        /// Event that is called when copy has completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbc_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            TaskCompleteCallBack("Complete"); // A callback using a delegate that calls a method when the copy completes
        }
        /// <summary>
        /// Event that is called whenever a file is copied
        /// This provides status 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbc_OnFileProcessed(object sender, FileProcessedEventArgs e)
        {
            switch (e.ProcessedFile.FileClass)
            {
                case "System Message":
                    Console.WriteLine(e.ProcessedFile.Name);
                    break;
                case "New Dir":
                    Console.WriteLine(e.ProcessedFile.Name);
                    break;
                case "New File":
                    Console.WriteLine("");
                    Console.Write("Copying " + e.ProcessedFile.Name);
                    break;
                default:
                    break;
            }
            
        }
    }
}
