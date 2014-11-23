using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Text;

using EventLogsCreateRemove.CustomConfigSection;
using MenuLibrary;
using Utilities.DisplayHelper;

namespace EventLogsCreateRemove
{
    [MenuClass("Event Log Manager Menu")]
    public class EventLogManager
    {
        private static EventLogsSection _eventLogsConfig = EventLogsSection.Settings;

        [MenuMethod("Create event logs and sources specified in config", 
            DisplayOrder = 1)]
        public static void CreateEventLogsAndSources()
        {
            string machineNameDisplayText = GetMachineNameDisplayText(_eventLogsConfig);

            Console.WriteLine();

            foreach (EventLogElement eventLogToCreate in _eventLogsConfig.Add.EventLogs)
            {
                if (eventLogToCreate.EventSources == null || eventLogToCreate.EventSources.Count == 0)
                {
                    throw new ArgumentException("No event sources specified for new event log."
                        + "  Cannot create event log without an event source.");
                }

                if (IsNullOrWhiteSpace(eventLogToCreate.Name))
                {
                    throw new ArgumentException("Cannot create event log without a name.");
                }

                foreach (EventSourceElement eventSourceToCreate in eventLogToCreate.EventSources)
                {
                    if (IsNullOrWhiteSpace(eventSourceToCreate.Name))
                    {
                        throw new ArgumentException("Cannot create event source without a name.");
                    }

                    Console.WriteLine("Creating event source {0} for event log {1} on {2}...",
                            eventSourceToCreate.Name, eventLogToCreate.Name, machineNameDisplayText);
                    try
                    {
                        if (EventLog.SourceExists(eventSourceToCreate.Name,
                            _eventLogsConfig.MachineName))
                        {
                            Console.WriteLine("Source already exists.");
                        }
                        else
                        {
                            EventSourceCreationData sourceCreationData = 
                                new EventSourceCreationData(eventSourceToCreate.Name, 
                                    eventLogToCreate.Name);
                            sourceCreationData.MachineName = _eventLogsConfig.MachineName;
                            EventLog.CreateEventSource(sourceCreationData);
                            Console.WriteLine("Done.");
                        }
                    }
                    catch (SecurityException ex)
                    {
                        DisplaySecurityMessage(ex);
                    }
                    Console.WriteLine();
                }
            }
        }

        [MenuMethod("Remove event logs and sources specified in config", 
            DisplayOrder = 2)]
        public static void RemoveEventLogsAndSources()
        {
            string machineNameDisplayText = GetMachineNameDisplayText(_eventLogsConfig);

            Console.WriteLine();

            foreach (EventLogElement eventLogToDelete in _eventLogsConfig.Remove.EventLogs)
            {
                if (IsNullOrWhiteSpace(eventLogToDelete.Name))
                {
                    throw new ArgumentException("Cannot remove event log without a name.");
                }

                Console.WriteLine("Removing event log {0} on {1}...",
                        eventLogToDelete.Name, machineNameDisplayText);
                try
                {
                    if (!EventLog.Exists(eventLogToDelete.Name, _eventLogsConfig.MachineName))
                    {
                        Console.WriteLine("Log does not exist.");
                    }
                    else
                    {
                        EventLog.Delete(eventLogToDelete.Name, _eventLogsConfig.MachineName);
                        Console.WriteLine("Done.");
                    }
                }
                catch (SecurityException ex)
                {
                    DisplaySecurityMessage(ex);
                }
                Console.WriteLine();
            }

            foreach (EventSourceElement eventSourceToDelete in _eventLogsConfig.Remove.EventSources)
            {
                if (IsNullOrWhiteSpace(eventSourceToDelete.Name))
                {
                    throw new ArgumentException("Cannot remove event source without a name.");
                }

                Console.WriteLine("Removing event source {0} on {1}...",
                        eventSourceToDelete.Name, machineNameDisplayText);
                try
                {
                    EventLog.DeleteEventSource(eventSourceToDelete.Name, _eventLogsConfig.MachineName);
                    Console.WriteLine("Done.");
                }
                catch (SecurityException ex)
                {
                    DisplaySecurityMessage(ex);
                }
                Console.WriteLine();
            }
        }

        [MenuMethod("Check existence of event logs and sources specified in config", 
            DisplayOrder = 3)]
        public static void CheckEventLogsAndSources()
        {
            string errorMessage = null;
            bool errorOccurred = false;
            string indent = new string(' ', 4);
            string machineNameDisplayText = GetMachineNameDisplayText(_eventLogsConfig);

            Console.WriteLine();

            foreach (EventLogElement eventLogToCheck in _eventLogsConfig.CheckExistence.EventLogs)
            {
                if (IsNullOrWhiteSpace(eventLogToCheck.Name))
                {
                    Console.WriteLine(
                        "ArgumentException: An event log listed in the config file has no name.");
                    Console.WriteLine();
                    errorOccurred = true;
                    continue;
                }

                Console.WriteLine("Checking event log {0} on {1}...",
                        eventLogToCheck.Name, machineNameDisplayText);
                try
                {
                    if (EventLog.Exists(eventLogToCheck.Name, _eventLogsConfig.MachineName))
                    {
                        Console.WriteLine("{0}Log exists.", indent);
                        bool completedSuccessfully = ListSourcesForEventLog(eventLogToCheck.Name,
                            machineNameDisplayText, indent);
                        errorOccurred |= !completedSuccessfully;                        
                    }
                    else
                    {
                        Console.WriteLine("{0}LOG NOT FOUND.", indent);
                    }
                }
                catch (SecurityException ex)
                {
                    DisplaySecurityMessage(ex);
                    errorOccurred = true;
                }
                Console.WriteLine();
            }

            foreach (EventSourceElement eventSourceToCheck in 
                _eventLogsConfig.CheckExistence.EventSources)
            {
                if (IsNullOrWhiteSpace(eventSourceToCheck.Name))
                {
                    Console.WriteLine(
                        "ArgumentException: An event source listed in the config file has no name.");
                    Console.WriteLine();
                    errorOccurred = true;
                    continue;
                }

                Console.WriteLine("Checking event source {0} on {1}...",
                        eventSourceToCheck.Name, machineNameDisplayText);
                try
                {
                    if (EventLog.SourceExists(eventSourceToCheck.Name,
                            _eventLogsConfig.MachineName))
                    {
                        Console.WriteLine("{0}Source exists.", indent);
                    }
                    else
                    {
                        Console.WriteLine("{0}SOURCE NOT FOUND.", indent);
                    }
                }
                catch (SecurityException ex)
                {
                    DisplaySecurityMessage(ex);
                    errorOccurred = true;
                }
                Console.WriteLine();
            }
        }

        [MenuMethod("List event logs and sources specified in config", 
            DisplayOrder = 4)]
        public static void ListEventLogsAndSources()
        {
            string errorMessage = null;
            bool errorOccurred = false;
            string indent = new string(' ', 4);
            string machineNameDisplayText = GetMachineNameDisplayText(_eventLogsConfig);

            Console.WriteLine();

            EventLog[] existingLogs = { };
            try
            {
                // All event logs on specified machine.
                existingLogs = EventLog.GetEventLogs(_eventLogsConfig.MachineName);
            }
            catch (SecurityException ex)
            {
                errorOccurred = true;
                errorMessage =
                    string.Format("Unable to get all event logs on {0}.", 
                        machineNameDisplayText);
                DisplaySecurityMessage(ex, errorMessage);
                return;
            }
            
            string[] logNameArray =
                Array.ConvertAll(existingLogs, delegate(EventLog log) { return log.Log; });
            List<string> existingLogNames = new List<string>(logNameArray);

            EventLogsCollection eventLogsInConfig = _eventLogsConfig.List.EventLogs;

            // If no event logs are specified in the config file list them all.
            bool listAllEventLogs = (eventLogsInConfig.Count <= 0);

            List<string> eventLogNamesToList = new List<string>();
            if (listAllEventLogs)
            {
                eventLogNamesToList = new List<string>(existingLogNames);
            }
            else
            {
                foreach (EventLogElement eventLogInConfig in eventLogsInConfig)
                {
                    eventLogNamesToList.Add(eventLogInConfig.Name);
                }
            }

            foreach (string logNameToList in eventLogNamesToList)
            {
                if (!existingLogNames.Contains(logNameToList))
                {
                    Console.WriteLine("Log {0}: [NOT FOUND ON MACHINE]", logNameToList);
                    continue;
                }

                Console.WriteLine("Log {0}:", logNameToList);

                bool completedSuccessfully = ListSourcesForEventLog(logNameToList, 
                    machineNameDisplayText, indent);
                errorOccurred |= !completedSuccessfully;
            }

            Console.WriteLine();
            if (errorOccurred)
            {
                Console.WriteLine("COMPLETED WITH ERRORS - SEE ERROR MESSAGES ABOVE.");
            }
            else
            {
                Console.WriteLine("Completed.");
            }
        }

        private static bool ListSourcesForEventLog(string logName,
            string machineNameDisplayText, string indent)
        {
            string errorMessage = null;
            bool completedSuccessfully = true;
            bool completedWithError = false;
            
            Console.WriteLine("{0}Sources:", indent);

            RegistryKey logRegistryKey = null;
            try
            {
                logRegistryKey =
                    GetEventLogRegistryKey(logName, _eventLogsConfig.MachineName);
            }
            catch (SecurityException ex)
            {
                errorMessage =
                    string.Format("Unable to access registry key"
                        + " for event log {0} on {1}.",
                        logName, machineNameDisplayText);
                DisplaySecurityMessage(ex, errorMessage);
                return completedWithError;
            }

            if (logRegistryKey == null)
            {
                Console.WriteLine("{0}{0}[NONE FOUND]", indent);
                return completedSuccessfully;
            }

            string[] sourceNames = { };
            try
            {
                // Based on decompiled code in System.Diagnostics.EventLog.FindSourceRegistration().
                sourceNames = logRegistryKey.GetSubKeyNames();
            }
            catch (SecurityException ex)
            {
                errorMessage =
                string.Format("Unable to get all registry sub-keys"
                        + " for event log {0} on machine {1}.",
                        logName, machineNameDisplayText);
                DisplaySecurityMessage(ex, errorMessage);
                return completedWithError;
            }

            if (sourceNames == null || sourceNames.Length <= 0)
            {
                Console.WriteLine("{0}{0}[NONE FOUND]", indent);
                return completedSuccessfully;
            }

            foreach (string sourceName in sourceNames)
            {
                Console.WriteLine("{0}{0}{1}", indent, sourceName);
            }

            return completedSuccessfully;
        }

        /// <remarks>Based on decompiled code in System.Diagnostics.EventLog.GetEventLogRegKey()</remarks>
        private static RegistryKey GetEventLogRegistryKey(string logName, string machineName)
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

        private static bool IsLocalMachine(EventLogsSection eventLogsConfig)
        {
            if (eventLogsConfig.MachineName == null 
                || eventLogsConfig.MachineName.Trim().Length == 0)
            {
                return true;
            }

            string lowerCaseMachineName = eventLogsConfig.MachineName.Trim().ToLower();

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

        private static string GetMachineNameDisplayText(EventLogsSection eventLogsConfig)
        {
            bool isLocalMachine = IsLocalMachine(eventLogsConfig);

            string machineNameDisplayText = "";
            if (isLocalMachine)
            {
                machineNameDisplayText = "local machine";
            }
            else
            {
                machineNameDisplayText = "machine " + eventLogsConfig.MachineName;
            }

            return machineNameDisplayText;
        }

        /// <summary>
        /// Equivalent of .NET 4.0 string.IsNullOrWhiteSpace().  Allows this application to 
        /// be written in .NET 2.0 so it can run on servers without .NET 4.0 installed.
        /// </summary>
        private static bool IsNullOrWhiteSpace(string text)
        {
            if (text == null || text.Trim().Length == 0)
            {
                return true;
            }

            return false;
        }

        private static void DisplaySecurityMessage(SecurityException ex)
        {
            DisplaySecurityMessage(ex, null);
        }

        private static void DisplaySecurityMessage(SecurityException ex, string errorMessage)
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
    }
}
