using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cptf
{

    public static class CopyTestData
    {
        static volatile bool run = true;
        static TestData testData { get; set; }
        static Task copyTask { get; set; }

        public static void Start(CopyParameters p)
        {
            // Copy test data and make sure to handle CTRL-C and make sure the RoboCOpy
            // thread is shutdown if CTRL-C is pressed.
            testData = new TestData
            {
                CopyParameters = p
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

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
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

}
