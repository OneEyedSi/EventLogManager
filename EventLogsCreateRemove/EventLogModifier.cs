using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventLogsCreateRemove.CustomConfigSection;
using MenuLibrary;

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

            foreach (EventLogElement eventLog in _eventLogsConfig.Add.EventLogs)
            {
                if (eventLog.EventSources == null || eventLog.EventSources.Count == 0)
                {
                    throw new ArgumentException("No event sources specified for new event log."
                        + "  Cannot create event log without an event source.");
                }

                if (string.IsNullOrWhiteSpace(eventLog.Name))
                {
                    throw new ArgumentException("Cannot create event log without a name.");
                }

                foreach (EventSourceElement eventSource in eventLog.EventSources)
                {
                    if (string.IsNullOrWhiteSpace(eventSource.Name))
                    {
                        throw new ArgumentException("Cannot create event source without a name.");
                    }

                    if (isLocalMachine)
                    {
                        Console.WriteLine("Creating event source {0} for event log {1} on local machine...",
                            eventSource.Name, eventLog.Name);
                        EventLog.CreateEventSource(eventSource.Name, eventLog.Name);
                    }
                    else
                    {
                        Console.WriteLine("Creating event source {0} for event log {1} on machine {2}...",
                            eventSource.Name, eventLog.Name, _eventLogsConfig.MachineName);
                        EventLog.CreateEventSource(eventSource.Name, eventLog.Name, 
                            _eventLogsConfig.MachineName);
                    }
                    Console.WriteLine("Done.");
                    Console.WriteLine();
                }
            }
        }

        [MenuMethod("Remove event logs and sources specified in config")]
        public static void RemoveEventLogsAndSources()
        {
            bool isLocalMachine = IsLocalMachine(_eventLogsConfig);

            foreach (EventLogElement eventLog in _eventLogsConfig.Remove.EventLogs)
            {
                if (string.IsNullOrWhiteSpace(eventLog.Name))
                {
                    throw new ArgumentException("Cannot remove event log without a name.");
                }

                if (isLocalMachine)
                {
                    Console.WriteLine("Removing event log {0} on local machine...",
                        eventLog.Name);
                    EventLog.Delete(eventLog.Name);
                }
                else
                {
                    Console.WriteLine("Removing event log {0} on machine {1}...",
                        eventLog.Name, _eventLogsConfig.MachineName);
                    EventLog.Delete(eventLog.Name, _eventLogsConfig.MachineName);
                }
                Console.WriteLine("Done.");
                Console.WriteLine();
            }

            foreach (EventSourceElement eventSource in _eventLogsConfig.Remove.EventSources)
            {
                if (string.IsNullOrWhiteSpace(eventSource.Name))
                {
                    throw new ArgumentException("Cannot remove event source without a name.");
                }

                if (isLocalMachine)
                {
                    Console.WriteLine("Removing event source {0} on local machine...",
                        eventSource.Name);
                    EventLog.Delete(eventSource.Name);
                }
                else
                {
                    Console.WriteLine("Removing event source {0} on machine {1}...",
                        eventSource.Name, _eventLogsConfig.MachineName);
                    EventLog.Delete(eventSource.Name, _eventLogsConfig.MachineName);
                }
                Console.WriteLine("Done.");
                Console.WriteLine();
            }
        }

        private static bool IsLocalMachine(EventLogsSection eventLogsConfig)
        {
            if (string.IsNullOrWhiteSpace(eventLogsConfig.MachineName))
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
    }
}
