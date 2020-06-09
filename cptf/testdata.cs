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
    public class TestData
    {

        public CopyParameters CopyParameters { get; set; }

        public RoboCommand RoboCopy { get; set; }

        public TestData()
        {
        }

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
            RoboCopy.CopyOptions.Source = sourceDir;
            RoboCopy.CopyOptions.Destination = destinationDir;
            RoboCopy.CopyOptions.CopySubdirectories = true;
            RoboCopy.CopyOptions.UseUnbufferedIo = true;
            RoboCopy.CopyOptions.Mirror = true;
            RoboCopy.CopyOptions.EnableRestartMode = true;
            RoboCopy.CopyOptions.UseUnbufferedIo = true;
            // retry options
            RoboCopy.RetryOptions.RetryCount = 1;
            RoboCopy.RetryOptions.RetryWaitTime = 2;
            var cpTask = RoboCopy.Start();
            return cpTask;
        }

        private void rbc_OnCopyProgressChanged(object sender, CopyProgressEventArgs e)
        {
            Console.Write(".");
        }

        private void rbc_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            Console.WriteLine("Copy Complete!");

        }

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
