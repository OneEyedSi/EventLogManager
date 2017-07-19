using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using ConsoleMenu;
using DisplayHelper;
using EventLogManager.CustomConfigSection;
using Microsoft.Win32;

namespace EventLogManager
{
    [MenuClass("Event Log From Config File", ParentMenuName = "Event Log Manager Menu")]
    public class EventLogFromConfig
    {
        private static EventLogsSection _eventLogsConfig = EventLogsSection.Settings;

        [MenuMethod("Create event logs and sources specified in config",
            DisplayOrder = 1)]
        public static void CreateEventLogsAndSources()
        {
            string machineName = _eventLogsConfig.MachineName;
            List<EventLogInfo> eventLogsToCreate = new List<EventLogInfo>();
            
            foreach (EventLogElement eventLogToCreate in _eventLogsConfig.Add.EventLogs)
            {
                List<string> eventSourceNamesToCreate = new List<string>();
                foreach (EventSourceElement eventSourceToCreate in eventLogToCreate.EventSources)
                {
                    eventSourceNamesToCreate.Add(eventSourceToCreate.Name);
                }
                EventLogInfo eventLogInfo = new EventLogInfo(eventLogToCreate.Name, 
                    eventSourceNamesToCreate);
                eventLogsToCreate.Add(eventLogInfo);
            }

            EventLogOperations.CreateEventLogsAndSources(machineName, eventLogsToCreate);
        }

        [MenuMethod("Remove event logs and sources specified in config",
            DisplayOrder = 2)]
        public static void RemoveEventLogsAndSources()
        {
            string machineName = _eventLogsConfig.MachineName;
            List<string> eventLogNamesToRemove = new List<string>();
            List<string> eventSourceNamesToRemove = new List<string>();

            EventLogsCollection eventLogsToRemove = _eventLogsConfig.Remove.EventLogs;
            foreach (EventLogElement eventLogToRemove in eventLogsToRemove)
            {
                eventLogNamesToRemove.Add(eventLogToRemove.Name);
            }

            EventSourcesCollection eventSourcesToRemove = _eventLogsConfig.Remove.EventSources;
            foreach (EventSourceElement eventSourceToRemove in eventSourcesToRemove)
            {
                eventSourceNamesToRemove.Add(eventSourceToRemove.Name);
            }

            EventLogOperations.RemoveEventLogsAndSources(machineName,
                eventLogNamesToRemove, eventSourceNamesToRemove);
        }

        [MenuMethod("Check existence of event logs and sources specified in config",
            DisplayOrder = 3)]
        public static void CheckEventLogsAndSources()
        {
            string machineName = _eventLogsConfig.MachineName;
            List<string> eventLogNamesToCheck = new List<string>();
            List<string> eventSourceNamesToCheck = new List<string>();

            EventLogsCollection eventLogsToCheck = _eventLogsConfig.CheckExistence.EventLogs;
            foreach (EventLogElement eventLogToCheck in eventLogsToCheck)
            {
                eventLogNamesToCheck.Add(eventLogToCheck.Name);
            }

            EventSourcesCollection eventSourcesToCheck = 
                _eventLogsConfig.CheckExistence.EventSources;
            foreach (EventSourceElement eventSourceToCheck in eventSourcesToCheck)
            {
                eventSourceNamesToCheck.Add(eventSourceToCheck.Name);
            }

            EventLogOperations.CheckEventLogsAndSources(machineName, 
                eventLogNamesToCheck, eventSourceNamesToCheck);
        }

        [MenuMethod("List event logs and sources specified in config",
            DisplayOrder = 4)]
        public static void ListEventLogsAndSources()
        {
            string machineName = _eventLogsConfig.MachineName;
            List<string> eventLogsNamesToList = new List<string>();
            EventLogsCollection eventLogsInConfig = _eventLogsConfig.List.EventLogs;
            foreach (EventLogElement eventLogInConfig in eventLogsInConfig)
            {
                eventLogsNamesToList.Add(eventLogInConfig.Name);
            }

            EventLogOperations.ListEventLogsAndSources(machineName, eventLogsNamesToList);
        }
    }
}