using System;
using System.Security;
using DisplayHelper;
using Microsoft.Win32;

namespace EventLogManager
{
    public class EventLogHelper
    {

        /// <remarks>Based on decompiled code in System.Diagnostics.EventLog.GetEventLogRegKey()</remarks>
        public static RegistryKey GetEventLogRegistryKey(string logName, string machineName)
        {
            bool writable = false;

            RegistryKey registryKey = (RegistryKey)null;
            try
            {
                if (machineName.Trim() == ".")
                {
                    registryKey = Registry.LocalMachine;
                }
                else
                {
                    registryKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName);
                }

                if (registryKey == null)
                {
                    return null;
                }

                // NB: Need to run this application as Administrator to access the Security event log registry key.
                return registryKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\" + logName, writable);
            }
            // No catch block.  Want exception to bubble up.
            finally
            {
                if (registryKey != null)
                {
                    registryKey.Close();
                }
            }
        }

        public static void DisplaySecurityMessage(SecurityException ex)
        {
            DisplaySecurityMessage(ex, null);
        }

        public static void DisplaySecurityMessage(SecurityException ex, string errorMessage)
        {
            bool wrapText = true;
            bool includeNewLine = true;

            if (errorMessage == null)
            {
                errorMessage = "";
            }
            else if (errorMessage.Trim().Length > 0)
            {
                errorMessage = "  " + errorMessage.Trim();
            }

            Console.WriteLine();

            ConsoleDisplayHelper.ShowHeadedText(0,
                "SecurityException:"
                + errorMessage
                + "  You need to run this application as Administrator."
                + "  Right-click the executable and select 'Run as administrator' from the"
                + " context menu.",
                wrapText, includeNewLine);
            Console.WriteLine();
            ConsoleDisplayHelper.ShowHeadedText(1, "Exception Details: {0}",
                wrapText, includeNewLine, ex.Message);
            if (ex.InnerException != null)
            {
                ConsoleDisplayHelper.ShowHeadedText(2, "Inner Exception - {0}",
                wrapText, includeNewLine, ex.InnerException.Message);
            }
        }

        public static bool IsLocalMachine(string machineName)
        {
            if (IsNullOrWhiteSpace(machineName))
            {
                return true;
            }

            string lowerCaseMachineName = machineName.Trim().ToLower();

            if (lowerCaseMachineName == "."
                || lowerCaseMachineName == "localhost"
                || lowerCaseMachineName == "(localhost)"
                || lowerCaseMachineName == "[localhost]"
                || lowerCaseMachineName == "{localhost}")
            {
                return true;
            }

            return false;
        }

        public static string GetMachineNameDisplayText(string machineName)
        {
            bool isLocalMachine = IsLocalMachine(machineName);

            string machineNameDisplayText = "";
            if (isLocalMachine)
            {
                machineNameDisplayText = "local machine";
            }
            else
            {
                machineNameDisplayText = "machine " + machineName;
            }

            return machineNameDisplayText;
        }

        /// <summary>
        /// Equivalent of .NET 4.0 string.IsNullOrWhiteSpace().  Allows this application to 
        /// be written in .NET 2.0 so it can run on servers without .NET 4.0 installed.
        /// </summary>
        public static bool IsNullOrWhiteSpace(string text)
        {
            if (text == null || text.Trim().Length == 0)
            {
                return true;
            }

            return false;
        }
    }
}