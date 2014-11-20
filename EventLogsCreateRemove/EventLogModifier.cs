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
    [MenuClass("Event Log Modifier Menu")]
    public class EventLogModifier
    {
        private static EventLogsSection _eventLogsConfig = EventLogsSection.Settings;

        [MenuMethod("Create event logs and sources specified in config")]
        public static void CreateEventLogsAndSources()
        {
            bool isLocalMachine = IsLocalMachine(_eventLogsConfig);

            Console.WriteLine();

            foreach (EventLogElement eventLog in _eventLogsConfig.Add.EventLogs)
            {
                if (eventLog.EventSources == null || eventLog.EventSources.Count == 0)
                {
                    throw new ArgumentException("No event sources specified for new event log."
                        + "  Cannot create event log without an event source.");
                }

                if (eventLog.Name == null || eventLog.Name.Trim().Length == 0)
                {
                    throw new ArgumentException("Cannot create event log without a name.");
                }

                foreach (EventSourceElement eventSource in eventLog.EventSources)
                {
                    if (eventSource.Name == null || eventSource.Name.Trim().Length == 0)
                    {
                        throw new ArgumentException("Cannot create event source without a name.");
                    }

                    if (isLocalMachine)
                    {
                        Console.WriteLine("Creating event source {0} for event log {1} on local machine...",
                            eventSource.Name, eventLog.Name);
                        try
                        {
                            if (EventLog.SourceExists(eventSource.Name))
                            {
                                Console.WriteLine("Source already exists.");
                            }
                            else
                            {
                                EventLog.CreateEventSource(eventSource.Name, eventLog.Name);
                                Console.WriteLine("Done.");
                            }
                        }
                        catch (SecurityException ex)
                        {
                            DisplaySecurityMessage(ex);
                        }
                    }
                    else
                    {                        
                        Console.WriteLine("Creating event source {0} for event log {1} on machine {2}...",
                            eventSource.Name, eventLog.Name, _eventLogsConfig.MachineName);
                        try
                        {
                            if (EventLog.SourceExists(eventSource.Name, 
                                _eventLogsConfig.MachineName))
                            {
                                Console.WriteLine("Source already exists.");
                            }
                            else
                            {
                                EventLog.CreateEventSource(eventSource.Name, eventLog.Name,
                                    _eventLogsConfig.MachineName);
                                Console.WriteLine("Done.");
                            }
                        }
                        catch (SecurityException ex)
                        {
                            DisplaySecurityMessage(ex);
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        [MenuMethod("Remove event logs and sources specified in config")]
        public static void RemoveEventLogsAndSources()
        {
            bool isLocalMachine = IsLocalMachine(_eventLogsConfig);

            Console.WriteLine();

            foreach (EventLogElement eventLog in _eventLogsConfig.Remove.EventLogs)
            {
                if (eventLog.Name == null || eventLog.Name.Trim().Length == 0)
                {
                    throw new ArgumentException("Cannot remove event log without a name.");
                }

                if (isLocalMachine)
                {
                    Console.WriteLine("Removing event log {0} on local machine...",
                        eventLog.Name);                    
                    try
                    {
                        if (!EventLog.Exists(eventLog.Name))
                        {
                            Console.WriteLine("Log does not exist.");
                        }
                        else
                        {
                            EventLog.Delete(eventLog.Name);
                            Console.WriteLine("Done.");
                        }
                    }
                    catch (SecurityException ex)
                    {
                        DisplaySecurityMessage(ex);
                    }
                }
                else
                {
                    Console.WriteLine("Removing event log {0} on machine {1}...",
                        eventLog.Name, _eventLogsConfig.MachineName);
                    try
                    {
                        if (!EventLog.Exists(eventLog.Name, _eventLogsConfig.MachineName))
                        {
                            Console.WriteLine("Log does not exist.");
                        }
                        else
                        {
                            EventLog.Delete(eventLog.Name, _eventLogsConfig.MachineName);
                            Console.WriteLine("Done.");
                        }
                    }
                    catch (SecurityException ex)
                    {
                        DisplaySecurityMessage(ex);
                    }
                }
                Console.WriteLine();
            }

            foreach (EventSourceElement eventSource in _eventLogsConfig.Remove.EventSources)
            {
                if (eventSource.Name == null || eventSource.Name.Trim().Length == 0)
                {
                    throw new ArgumentException("Cannot remove event source without a name.");
                }

                if (isLocalMachine)
                {
                    Console.WriteLine("Removing event source {0} on local machine...",
                        eventSource.Name);
                    try
                    {
                        EventLog.DeleteEventSource(eventSource.Name);
                        Console.WriteLine("Done.");
                    }
                    catch (SecurityException ex)
                    {
                        DisplaySecurityMessage(ex);
                    }
                }
                else
                {
                    Console.WriteLine("Removing event source {0} on machine {1}...",
                        eventSource.Name, _eventLogsConfig.MachineName);
                    try
                    {
                        EventLog.DeleteEventSource(eventSource.Name, _eventLogsConfig.MachineName);
                        Console.WriteLine("Done.");
                    }
                    catch (SecurityException ex)
                    {
                        DisplaySecurityMessage(ex);
                    }
                }
                Console.WriteLine();
            }
        }

        private static bool IsLocalMachine(EventLogsSection eventLogsConfig)
        {
            if (eventLogsConfig.MachineName == null || eventLogsConfig.MachineName.Trim().Length == 0)
            {
                return true;
            }

            string lowerCaseMachineName = eventLogsConfig.MachineName.Trim().ToLower();

            if (lowerCaseMachineName == "localhost"
                || lowerCaseMachineName == "(localhost)"
                || lowerCaseMachineName == "[localhost]"
                || lowerCaseMachineName == "{localhost}")
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
