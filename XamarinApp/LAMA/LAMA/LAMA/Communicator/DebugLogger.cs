using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LAMA.Communicator
{
    public class DebugLogger
    {
        readonly private bool shouldLog;
        private object loggingLock = new object();

        /// <summary>
        /// Logger capable of writing into a file
        /// </summary>
        /// <param name="shouldLog">Is logging turned on</param>
        public DebugLogger(bool shouldLog)
        {
            this.shouldLog = shouldLog;
        }

        /// <summary>
        /// Log a debug message into a text file
        /// </summary>
        /// <param name="logMessage">Message to be logged (time included automatically)</param>
        public void LogWrite(string logMessage)
        {
            if (shouldLog)
            {
                string m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                lock (loggingLock)
                {
                    using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                    {
                        Log(logMessage, w);
                    }
                }
            }
        }

        private void Log(string logMessage, TextWriter txtWriter)
        {
            txtWriter.WriteLine("{0} {1} : {2}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(), logMessage);
            txtWriter.WriteLine("----------------------------------");
        }
    }
}
