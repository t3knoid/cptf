using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cptf
{
    public class Logger
    {
        /// <summary>
        /// This is a semaphore that will be used to synchronize logging calls
        /// </summary>
        protected readonly object logObj = new object();
        /// <summary>
        /// A default constructor that will create a log file using the assembly name
        /// and folder location for the log file name and location. Each log filename will
        /// be prefixed using the assembly filename (without the extension) followed by
        /// a string representing the date and time when the application started.
        /// </summary>
        public Logger()
        {}

        /// <summary>
        /// Writes a message and optional exception as an error to the log file 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="module"></param>
        public void Error(string message, Exception ex = null, string module = "")
        {
            if (ex != null)
            {
                WriteEntry(ex.Message, "[ERROR]", module);
                WriteEntry(ex.StackTrace, "[ERROR]", module);            
            }
            WriteEntry(message, "[ERROR]", module);
        }
        /// <summary>
        /// Writes a given message as a warning to the log file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="module"></param>
        public void Warning(string message,string module = "")
        {
            WriteEntry(message, "[WARNING]", module);
        }
        /// <summary>
        /// Writes a given message as an information to the log file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="module"></param>
        public void Info(string message, string module = "")
        {
            WriteEntry(message, "[INFO]", module);
        }
        /// <summary>
        /// A helper method to write a log entry.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="module"></param>
        private void WriteEntry(string message, string type, string module)
        {
            Logfile logfile = new Logfile();

            lock (logObj)
            {

                using (StreamWriter streamWriter = new StreamWriter(logfile.Path,true))
                {
                    streamWriter.WriteLine(string.Format("{0},{1},{2},{3}",
                                  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                  type,
                                  module,
                                  message));
                    streamWriter.Close();
                }
            }

        }
    }
    /// <summary>
    /// Defines the log file
    /// </summary>
    public class Logfile
    {
        public string Path { get; private set; }
        public string Filename { get; private set; }

        public Logfile()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            Filename = fvi.FileDescription;
            string version = fvi.FileVersion;
            string fileDir = System.IO.Path.GetTempPath();
            //Path = String.Format("{0}_{1}.log", System.IO.Path.Combine(fileDir, Filename), DateTime.Now.ToString("yyyyMMddHHmmss"));
            Path = String.Format("{0}.log", System.IO.Path.Combine(fileDir, Filename));
        }
    }
    /// <summary>
    /// A class that provides a static method that can be used to write to the log file 
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Logger class
        /// </summary>
        private static Logger logger = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        /// <param name="ex"></param>
        /// <param name="module"></param>
        public static void Log(LogLevel logLevel, string logMessage, Exception ex = null, [CallerMemberName]string module = "")
        {
            switch (logLevel)
            {
                case LogLevel.ERROR:
                    logger = new Logger();
                    logger.Error(logMessage, ex, module);
                    break;
                case LogLevel.INFO:
                    logger = new Logger();
                    logger.Info(logMessage, module);
                    break;
                case LogLevel.WARNING:
                    logger = new Logger();
                    logger.Warning(logMessage, module);
                    break;
                default:
                    return;
            }
        }
    }

    /// <summary>
    /// Defines log levels
    /// </summary>
    public enum LogLevel
    {
        INFO,
        ERROR,
        WARNING
    }
    
}