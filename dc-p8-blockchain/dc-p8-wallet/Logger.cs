// Filename: Logger.cs
// Project:  DC Assignment (COMP3008)
// Purpose:  Logging  
// Author:   Kevin Le (19472960)
//
// Date:     25/05/2020

using System;
using System.Diagnostics;
using System.Globalization;

namespace dc_p8_wallet
{
    class Logger
    {
        //Count for number of tasks completed 
        private static uint logNumber = 0;

        /// <summary>
        /// Logs tasks by outputting task string provided with timestamp and source of task. 
        /// </summary>
        /// <param name="logString">String to output and write to log file</param>
        public static void Log(string logString)
        {
            logNumber++;

            string outDate = DateTime.Now.ToString(new CultureInfo("en-GB"));

            Debug.WriteLine(outDate + " | Task: " + logString);
            Debug.WriteLine(outDate + " | Tasks performed so far: " + logNumber.ToString());
        }

        /// <summary>
        /// Logs errors by outputting error string provided with timestamp and source of error. 
        /// </summary>
        /// <param name="errString">String to output and write to log file</param>
        public static void Error(string errString)
        {
            logNumber++;

            string outDate = DateTime.Now.ToString(new CultureInfo("en-GB"));

            Debug.WriteLine(outDate + " | Error: " + errString);
            Debug.WriteLine(outDate + " | Tasks performed so far: " + logNumber.ToString());
        }
    }
}
