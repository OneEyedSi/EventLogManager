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

        [MenuMethod("Create event logs and sources specified in config")]
        public static void CreateEventLogsAndSources()
        {
            bool isLocalMachine = IsLocalMachine(_eventLogsConfig);

            string machineNameDisplayText = "";
            if (isLocalMachine)
            {
                machineNameDisplayText = "local machine";
            }
            else
            {
                machineNameDisplayText = "machine " + _eventLogsConfig.MachineName;
            }

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

        [MenuMethod("Remove event logs and sources specified in config")]
        public static void RemoveEventLogsAndSources()
        {
            bool isLocalMachine = IsLocalMachine(_eventLogsConfig);

            string machineNameDisplayText = "";
            if (isLocalMachine)
            {
                machineNameDisplayText = "local machine";
            }
            else
            {
                machineNameDisplayText = "machine " + _eventLogsConfig.MachineName;
            }

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
            bool wrapText = true;
            bool includeNewLine = true;
            Console.WriteLine();
            ConsoleDisplayHelper.ShowHeadedText(0,
                "SecurityException: You need to run this application as Administrator."
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
