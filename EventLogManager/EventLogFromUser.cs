using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConsoleMenu;
using DisplayHelper;
using EventLogManager.CustomConfigSection;

namespace EventLogManager
{
    [MenuClass("Event Log From User Input", ParentMenuName = "Event Log Manager Menu")]
    public class EventLogFromUser
    {

        [MenuMethod("Create event logs and sources on local machine",
           DisplayOrder = 1)]
        public static void CreateEventLogsAndSourcesOnLocalMachine()
        {
            CreateEventLogsAndSources(null);
        }

        [MenuMethod("Create event logs and sources on remote machine",
           DisplayOrder = 2)]
        public static void CreateEventLogsAndSourcesOnRemoteMachine()
        {
            string machineName = GetMachineName();
            CreateEventLogsAndSources(machineName);
        }

        private static void CreateEventLogsAndSources(string machineName)
        {
            DisplayCaseSensitivityWarning();
            Console.WriteLine("Enter comma-separated list of event logs in the form:");
            bool doNotWrapText = false;
            bool addNewLine = true;
            ConsoleDisplayHelper.ShowIndentedText(1, "Log1, Log2, ...", doNotWrapText, addNewLine);
            Console.WriteLine("One or more log sources may be included for a log in the form:");
            ConsoleDisplayHelper.ShowIndentedText(1, "Log1[log1Source1, log1Source2], Log2, ...",
                doNotWrapText, addNewLine);
            Console.WriteLine("([ENTER] to skip creating any logs or sources)");
            string commaSeparatedLogList = Console.ReadLine();

            List<EventLogInfo> eventLogsToCreate = ParseListOfLogsAndSources(commaSeparatedLogList);

            if (eventLogsToCreate == null || eventLogsToCreate.Count == 0)
            {
                Console.WriteLine("Cannot create logs, none specified.");
                return;
            }

            EventLogOperations.CreateEventLogsAndSources(machineName, eventLogsToCreate);
        }

        [MenuMethod("Remove event logs and sources on local machine",
            DisplayOrder = 3)]
        public static void RemoveEventLogsAndSourcesOnLocalMachine()
        {
            RemoveEventLogsAndSources(null);
        }

        [MenuMethod("Remove event logs and sources on remote machine",
            DisplayOrder = 4)]
        public static void RemoveEventLogsAndSourcesOnRemoteMachine()
        {
            string machineName = GetMachineName();
            RemoveEventLogsAndSources(machineName);
        }

        private static void RemoveEventLogsAndSources(string machineName)
        {
            DisplayCaseSensitivityWarning();
            string additionalInfoText = "([ENTER] to skip)";
            List<string> eventLogNamesToRemove
                = GetStringList("Enter comma-separated list of event logs to remove:",
                    additionalInfoText);
            List<string> eventSourceNamesToRemove
                = GetStringList("Enter comma-separated list of event sources to remove:",
                    additionalInfoText);

            if (eventLogNamesToRemove.Count == 0 && eventSourceNamesToRemove.Count == 0)
            {
                Console.WriteLine("Cannot remove logs or sources, none specified.");
                return;
            }

            EventLogOperations.RemoveEventLogsAndSources(machineName,
                eventLogNamesToRemove, eventSourceNamesToRemove);
        }

        [MenuMethod("Check existence of event logs and sources on local machine",
            DisplayOrder = 5)]
        public static void CheckEventLogsAndSourcesOnLocalMachine()
        {
            CheckEventLogsAndSources(null);
        }

        [MenuMethod("Check existence of event logs and sources on remote machine",
            DisplayOrder = 6)]
        public static void CheckEventLogsAndSourcesOnRemoteMachine()
        {
            string machineName = GetMachineName();
            CheckEventLogsAndSources(machineName);
        }

        private static void CheckEventLogsAndSources(string machineName)
        {
            DisplayCaseSensitivityWarning();
            string additionalInfoText = "([ENTER] to skip)";
            List<string> eventLogNamesToCheck
                = GetStringList("Enter comma-separated list of event logs to check:",
                    additionalInfoText);
            List<string> eventSourceNamesToCheck
                = GetStringList("Enter comma-separated list of event sources to check:",
                    additionalInfoText);

            if (eventLogNamesToCheck.Count == 0 && eventSourceNamesToCheck.Count == 0)
            {
                Console.WriteLine("Cannot check existence of logs or sources, none specified.");
                return;
            }

            EventLogOperations.CheckEventLogsAndSources(machineName,
                eventLogNamesToCheck, eventSourceNamesToCheck);
        }

        [MenuMethod("List event logs and sources on local machine",
            DisplayOrder = 7)]
        public static void ListEventLogsAndSourcesOnLocalMachine()
        {
            ListEventLogsAndSources(null);
        }

        [MenuMethod("List event logs and sources on remote machine",
            DisplayOrder = 8)]
        public static void ListEventLogsAndSourcesOnRemoteMachine()
        {
            string machineName = GetMachineName();
            ListEventLogsAndSources(machineName);
        }

        private static void ListEventLogsAndSources(string machineName)
        {
            DisplayCaseSensitivityWarning();
            string warningMessages
                = string.Format("WARNING: Application and System event logs may contain hundreds of {0}"
                                + "{1}sources each.  Be aware of this when listing all event logs and sources.",
                    Environment.NewLine, GlobalConstant.Indent);
            Console.WriteLine(warningMessages);
            Console.WriteLine();

            string additionalInfoText = "([ENTER] to list all event logs on the machine)";
            List<string> eventLogsNamesToList
                = GetStringList("Enter comma-separated list of event logs to list:",
                    additionalInfoText);

            if (eventLogsNamesToList.Count == 0)
            {
                Console.WriteLine("Cannot list logs and sources, none specified.");
                return;
            }

            EventLogOperations.ListEventLogsAndSources(machineName, eventLogsNamesToList);
        }

        private static string GetMachineName()
        {
            Console.WriteLine("Enter the name of the machine to operate on ([ENTER] for local machine):");
            string machineName = Console.ReadLine();
            return machineName;
        }

        private static void DisplayCaseSensitivityWarning()
        {
            Console.WriteLine("WARNING: LOG AND SOURCE NAMES ARE CASE-SENSITIVE.");
            Console.WriteLine();
        }

        private static List<EventLogInfo> ParseListOfLogsAndSources(string commaSeparatedLogList)
        {
            // commaSeparatedLogList will be of the form:
            //  MyLog1, MyLog2, ...
            // Optionally logs may be followed by a list of sources:
            //  MyLog1[source1, source2], MyLog2, ...
 
            if (commaSeparatedLogList == null || commaSeparatedLogList.Trim().Length == 0)
            {
                return null;
            }

            List<string> rawLogList = ParseStringList(commaSeparatedLogList);

            // The split on commas will result in sources being split as well as logs.
            //  No doubt there is a way to replace commas in the source lists using regular 
            //  expressions but it doesn't seem trivial so instead we'll sort out the sources the 
            //  simple way.

            bool isInSourceList = false;
            List<EventLogInfo> logList = new List<EventLogInfo>();
            EventLogInfo logInfo = null;
            foreach (string rawLogDetails in rawLogList)
            {
                string logName;

                // Log name + start of source list
                if (rawLogDetails.Contains("["))
                {
                    isInSourceList = true;
                    string[] logInfoParts = rawLogDetails.Split(new char[] { '[' },
                        StringSplitOptions.RemoveEmptyEntries);
                    logName = logInfoParts[0];
                    List<string> sourceNames = new List<string>();
                    sourceNames.Add(logInfoParts[1]);
                    logInfo = new EventLogInfo(logName, sourceNames);
                    logList.Add(logInfo);
                }
                else if (isInSourceList)
                {
                    string sourceName = rawLogDetails;

                    // End of source list.
                    if (rawLogDetails.Contains("]"))
                    {
                        sourceName = sourceName.TrimEnd(new char[] { ']' });
                        isInSourceList = false;
                    }
                    logInfo.EventSourceNames.Add(sourceName);
                }
                else
                {
                    logName = rawLogDetails;
                    logInfo = new EventLogInfo(logName, null);
                    logList.Add(logInfo);
                }
            }

            return logList;
        }

        private static List<string> GetStringList(string promptText, string additionalInfoText)
        {
            Console.WriteLine(promptText);
            if (additionalInfoText != null)
            {
                Console.WriteLine(additionalInfoText);
            }
            string commaSeparatedList = Console.ReadLine();
            return ParseStringList(commaSeparatedList);
        }

        private static List<string> ParseStringList(string commaSeparatedList)
        {
            string[] items = commaSeparatedList.Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);

            // Targetting .NET 2.0 so no LINQ.  Trim the resulting items the old way in case there 
            //  were spaces before or after the commas.
            List<string> list = new List<string>();
            foreach (string item in items)
            {
                list.Add(item.Trim());
            }

            return list;
        }
    }
}