using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using DisplayHelper;
using EventLogManager.CustomConfigSection;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;

namespace EventLogManager
{
    /// <summary>
    /// Performs operations related to event logs and event log sources, such as creating or 
    /// removing logs.
    /// </summary>
    public class EventLogOperations
    {
        /// <summary>
        /// Creates the specified event logs and sources on the specified machine.
        /// </summary>
        /// <param name="machineName">The name of the machine to create the logs and sources on.  A 
        /// value of null, an empty string or "." specifies the local machine this application is 
        /// running on.</param>
        /// <param name="eventLogsToCreate">A list of event logs and associated sources to create.
        /// </param>
        public static void CreateEventLogsAndSources(string machineName,
            List<EventLogInfo> eventLogsToCreate)
        {
            machineName = CleanMachineName(machineName);
            string machineNameDisplayText = EventLogHelper.GetMachineNameDisplayText(machineName);

            Console.WriteLine();

            foreach (EventLogInfo eventLogToCreate in eventLogsToCreate)
            {
                if (EventLogHelper.IsNullOrWhiteSpace(eventLogToCreate.Name))
                {
                    throw new ArgumentException("Cannot create event log without a name.");
                }

                if (eventLogToCreate.EventSourceNames == null
                    || eventLogToCreate.EventSourceNames.Count == 0)
                {
                    eventLogToCreate.EventSourceNames = new List<string>();
                    eventLogToCreate.EventSourceNames.Add(eventLogToCreate.Name);
                }

                foreach (string eventSourceNameToCreate in eventLogToCreate.EventSourceNames)
                {
                    if (EventLogHelper.IsNullOrWhiteSpace(eventSourceNameToCreate))
                    {
                        throw new ArgumentException("Cannot create event source without a name.");
                    }

                    Console.WriteLine("Creating event source {0} for event log {1} on {2}...",
                            eventSourceNameToCreate, eventLogToCreate.Name, machineNameDisplayText);
                    try
                    {
                        if (EventLog.SourceExists(eventSourceNameToCreate, machineName))
                        {
                            Console.WriteLine("Source already exists.");
                        }
                        else
                        {
                            EventSourceCreationData sourceCreationData =
                                new EventSourceCreationData(eventSourceNameToCreate,
                                    eventLogToCreate.Name);
                            sourceCreationData.MachineName = machineName;
                            EventLog.CreateEventSource(sourceCreationData);
                            Console.WriteLine("Done.");
                        }
                    }
                    catch (SecurityException ex)
                    {
                        EventLogHelper.DisplaySecurityMessage(ex);
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Removes the specified event logs and sources from the specified machine.
        /// </summary>
        /// <param name="machineName">The name of the machine to remove the logs and sources from.  
        /// A value of null, an empty string or "." specifies the local machine this application is 
        /// running on.</param>
        /// <param name="eventLogNamesToRemove">A list of event logs to remove.  The list may be 
        /// empty, in which case only the specified event sources will be removed.</param>
        /// <param name="eventSourceNamesToRemove">A list of event sources to remove.  The list may 
        /// be empty.</param>
        /// <remarks>It is not possible to remove sources which have the same name as their 
        /// associated event logs.  This is a limitation of the .NET EventLog.DeleteEventSource 
        /// method.  Instead of removing the source the user should remove the log of the same 
        /// name.
        /// 
        /// When an event log is removed all its associated event sources are automatically 
        /// removed as well. There is no need to specify the event sources separately if the logs 
        /// they are associated with are going to be removed.</remarks>
        public static void RemoveEventLogsAndSources(string machineName,
            List<string> eventLogNamesToRemove, List<string> eventSourceNamesToRemove)
        {
            machineName = CleanMachineName(machineName);
            string machineNameDisplayText = EventLogHelper.GetMachineNameDisplayText(machineName);

            Console.WriteLine();

            foreach (string eventLogNameToRemove in eventLogNamesToRemove)
            {
                if (EventLogHelper.IsNullOrWhiteSpace(eventLogNameToRemove))
                {
                    throw new ArgumentException("Cannot remove event log without a name.");
                }

                Console.WriteLine("Removing event log {0} on {1}...",
                        eventLogNameToRemove, machineNameDisplayText);
                try
                {
                    if (!EventLog.Exists(eventLogNameToRemove, machineName))
                    {
                        Console.WriteLine("Log does not exist.");
                    }
                    else
                    {
                        EventLog.Delete(eventLogNameToRemove, machineName);
                        Console.WriteLine("Done.");
                    }
                }
                catch (SecurityException ex)
                {
                    EventLogHelper.DisplaySecurityMessage(ex);
                }
                Console.WriteLine();
            }

            foreach (string eventSourceNameToRemove in eventSourceNamesToRemove)
            {
                if (EventLogHelper.IsNullOrWhiteSpace(eventSourceNameToRemove))
                {
                    throw new ArgumentException("Cannot remove event source without a name.");
                }

                Console.WriteLine("Removing event source {0} on {1}...",
                        eventSourceNameToRemove, machineNameDisplayText);
                try
                {
                    if (!EventLog.SourceExists(eventSourceNameToRemove, machineName))
                    {
                        Console.WriteLine("Event source does not exist.");
                        Console.WriteLine();
                        continue;
                    }

                    string associatedLogName =
                        EventLog.LogNameFromSourceName(eventSourceNameToRemove, machineName);

                    if (string.Equals(eventSourceNameToRemove, associatedLogName, 
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        string message = string.Format("Cannot delete event source because it has "
                                                       + "the same name as the event log.{0}"
                                                       + "If you wish to delete this event source "
                                                       + "and its associated log try again but "
                                                       + "delete the log, not the source.", 
                                                       Environment.NewLine);
                        Console.WriteLine(message);
                        Console.WriteLine();
                        continue;
                    }

                    EventLog.DeleteEventSource(eventSourceNameToRemove, machineName);
                    Console.WriteLine("Done.");
                }
                catch (SecurityException ex)
                {
                    EventLogHelper.DisplaySecurityMessage(ex);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Checks the existence of the specified event logs and sources on the specified machine.
        /// </summary>
        /// <param name="machineName">The name of the machine to check the logs and sources on.  
        /// A value of null, an empty string or "." specifies the local machine this application is 
        /// running on.</param>
        /// <param name="eventLogNamesToCheck">A list of event logs to check.</param>
        /// <param name="eventSourceNamesToCheck">A list of event sources to check.</param>
        /// <remarks>If a specified log is found the sources associated with it are listed.  If 
        /// a specified source is found the log associated with it is displayed.</remarks>
        public static void CheckEventLogsAndSources(string machineName,
            List<string> eventLogNamesToCheck, List<string> eventSourceNamesToCheck)
        {
            string errorMessage = null;
            bool errorOccurred = false;

            machineName = CleanMachineName(machineName);
            string machineNameDisplayText = EventLogHelper.GetMachineNameDisplayText(machineName);

            Console.WriteLine();

            foreach (string eventLogNameToCheck in eventLogNamesToCheck)
            {
                if (EventLogHelper.IsNullOrWhiteSpace(eventLogNameToCheck))
                {
                    Console.WriteLine(
                        "ArgumentException: A specified event log has no name.");
                    Console.WriteLine();
                    errorOccurred = true;
                    continue;
                }

                Console.WriteLine("Checking event log {0} on {1}...",
                    eventLogNameToCheck, machineNameDisplayText);
                try
                {
                    if (EventLog.Exists(eventLogNameToCheck, machineName))
                    {
                        Console.WriteLine("{0}Log exists.", GlobalConstant.Indent);
                        bool completedSuccessfully = EventLogOperations.ListSourcesForEventLog(
                            machineName, machineNameDisplayText,
                            eventLogNameToCheck, GlobalConstant.Indent);
                        errorOccurred |= !completedSuccessfully;
                    }
                    else
                    {
                        Console.WriteLine("{0}LOG NOT FOUND.", GlobalConstant.Indent);
                    }
                }
                catch (SecurityException ex)
                {
                    EventLogHelper.DisplaySecurityMessage(ex);
                    errorOccurred = true;
                }
                Console.WriteLine();
            }

            foreach (string eventSourceNameToCheck in eventSourceNamesToCheck)
            {
                if (EventLogHelper.IsNullOrWhiteSpace(eventSourceNameToCheck))
                {
                    Console.WriteLine(
                        "ArgumentException: A specified event source has no name.");
                    Console.WriteLine();
                    errorOccurred = true;
                    continue;
                }

                Console.WriteLine("Checking event source {0} on {1}...",
                    eventSourceNameToCheck, machineNameDisplayText);
                try
                {
                    if (EventLog.SourceExists(eventSourceNameToCheck, machineName))
                    {
                        Console.WriteLine("{0}Source exists.", GlobalConstant.Indent);
                        string associatedLogName =
                            EventLog.LogNameFromSourceName(eventSourceNameToCheck, machineName);
                        Console.WriteLine("{0}Associated Event Log: {1}",
                            GlobalConstant.Indent, associatedLogName);
                    }
                    else
                    {
                        Console.WriteLine("{0}SOURCE NOT FOUND.", GlobalConstant.Indent);
                    }
                }
                catch (SecurityException ex)
                {
                    EventLogHelper.DisplaySecurityMessage(ex);
                    errorOccurred = true;
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Lists the specified event logs and their associated sources on the specified machine.
        /// </summary>
        /// <param name="machineName">The name of the machine to list the logs and sources on.  A 
        /// value of null, an empty string or "." specifies the local machine this application is 
        /// running on.</param>
        /// <param name="eventLogsToList">A list of event logs to list.  If the list is empty then 
        /// all event logs and all their associated sources will be listed.</param>
        /// <remarks>If all event logs on a machine are listed hundreds of sources may be listed, 
        /// as the Application and System event logs may contain hundreds of sources each.</remarks>
        public static void ListEventLogsAndSources(string machineName, 
            List<string> eventLogsToList)
        {
            string errorMessage = null;
            bool errorOccurred = false;
            string indent = new string(' ', 4);
            machineName = CleanMachineName(machineName);
            string machineNameDisplayText = EventLogHelper.GetMachineNameDisplayText(machineName);

            Console.WriteLine();

            EventLog[] existingLogs = { };
            try
            {
                // All event logs on specified machine.
                existingLogs = EventLog.GetEventLogs(machineName);
            }
            catch (SecurityException ex)
            {
                errorOccurred = true;
                errorMessage =
                    string.Format("Unable to get all event logs on {0}.",
                        machineNameDisplayText);
                EventLogHelper.DisplaySecurityMessage(ex, errorMessage);
                return;
            }

            string[] logNameArray =
                Array.ConvertAll(existingLogs, delegate(EventLog log) { return log.Log; });
            List<string> existingLogNames = new List<string>(logNameArray);

            // If no event logs are specified list them all.
            bool listAllEventLogs = (eventLogsToList.Count <= 0);

            List<string> eventLogNamesToList = new List<string>();
            if (listAllEventLogs)
            {
                eventLogNamesToList = new List<string>(existingLogNames);
            }
            else
            {
                eventLogNamesToList = eventLogsToList;
            }

            foreach (string logNameToList in eventLogNamesToList)
            {
                if (!existingLogNames.Contains(logNameToList))
                {
                    Console.WriteLine("Log {0}: [NOT FOUND ON MACHINE]", logNameToList);
                    continue;
                }

                Console.WriteLine("Log {0}:", logNameToList);

                bool completedSuccessfully = EventLogOperations.ListSourcesForEventLog(
                    machineName, machineNameDisplayText, logNameToList, indent);
                errorOccurred |= !completedSuccessfully;
            }

            Console.WriteLine();
            if (errorOccurred)
            {
                ConsoleColor originalTextColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("COMPLETED WITH ERRORS - SEE ERROR MESSAGES ABOVE.");
                Console.ForegroundColor = originalTextColour;
            }
            else
            {
                Console.WriteLine("Completed.");
            }
        }

        private static bool ListSourcesForEventLog(string machineName,
            string machineNameDisplayText, string logName, string indent)
        {
            string errorMessage = null;
            bool completedSuccessfully = true;
            bool completedWithError = false;

            Console.WriteLine("{0}Sources:", indent);

            RegistryKey logRegistryKey = null;
            try
            {
                logRegistryKey =
                    EventLogHelper.GetEventLogRegistryKey(logName, machineName);
            }
            catch (SecurityException ex)
            {
                errorMessage =
                    string.Format("Unable to access registry key"
                        + " for event log '{0}' on {1}.",
                        logName, machineNameDisplayText);
                EventLogHelper.DisplaySecurityMessage(ex, errorMessage);
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
                        + " for event log '{0}' on {1}.",
                        logName, machineNameDisplayText);
                EventLogHelper.DisplaySecurityMessage(ex, errorMessage);
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

        private static string CleanMachineName(string rawMachineName)
        {
            if (EventLogHelper.IsLocalMachine(rawMachineName))
            {
                return ".";
            }

            return rawMachineName.Trim();
        }
    }
}