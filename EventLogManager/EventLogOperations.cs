using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using DisplayHelper;
using EventLogManager.CustomConfigSection;
using Microsoft.Win32;

namespace EventLogManager
{
    public class EventLogOperations
    {
        public static void CreateEventLogsAndSources(string machineName,
            List<EventLogInfo> eventLogsToCreate)
        {
            string machineNameDisplayText = EventLogHelper.GetMachineNameDisplayText(machineName);

            Console.WriteLine();

            foreach (EventLogInfo eventLogToCreate in eventLogsToCreate)
            {
                if (eventLogToCreate.EventSourceNames == null
                    || eventLogToCreate.EventSourceNames.Count == 0)
                {
                    throw new ArgumentException("No event sources specified for new event log."
                        + "  Cannot create event log without an event source.");
                }

                if (EventLogHelper.IsNullOrWhiteSpace(eventLogToCreate.Name))
                {
                    throw new ArgumentException("Cannot create event log without a name.");
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

        public static void RemoveEventLogsAndSources(string machineName,
            List<string> eventLogNamesToRemove, List<string> eventSourceNamesToRemove)
        {
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
                    }
                    else
                    {
                        EventLog.DeleteEventSource(eventSourceNameToRemove, machineName);
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

        public static void CheckEventLogsAndSources(string machineName,
            List<string> eventLogNamesToCheck, List<string> eventSourceNamesToCheck)
        {
            string errorMessage = null;
            bool errorOccurred = false;
            string indent = new string(' ', 4);
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
                        Console.WriteLine("{0}Log exists.", indent);
                        bool completedSuccessfully = EventLogOperations.ListSourcesForEventLog(
                            machineName, machineNameDisplayText, 
                            eventLogNameToCheck, indent);
                        errorOccurred |= !completedSuccessfully;
                    }
                    else
                    {
                        Console.WriteLine("{0}LOG NOT FOUND.", indent);
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
                        Console.WriteLine("{0}Source exists.", indent);
                        string associatedLogName =
                            EventLog.LogNameFromSourceName(eventSourceNameToCheck, machineName);
                        Console.WriteLine("{0}Associated Event Log: {1}",
                            indent, associatedLogName);
                    }
                    else
                    {
                        Console.WriteLine("{0}SOURCE NOT FOUND.", indent);
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

        public static void ListEventLogsAndSources(string machineName, 
            List<string> eventLogsToList)
        {
            string errorMessage = null;
            bool errorOccurred = false;
            string indent = new string(' ', 4);
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
                Console.WriteLine("COMPLETED WITH ERRORS - SEE ERROR MESSAGES ABOVE.");
            }
            else
            {
                Console.WriteLine("Completed.");
            }
        }

        public static bool ListSourcesForEventLog(string machineName,
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
                        + " for event log {0} on {1}.",
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
                        + " for event log {0} on machine {1}.",
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
    }
}