EventLogManager
===============

C# console application of utilities for working with event logs.

Data Source Options
-------------------
There are two data source options for specifying event logs and sources:

1. Config: Reads the event logs and sources from a config file.  This allows users to reliably 
perform the same operation on different servers (eg dev, test);
2. User Input: Prompts the user to enter the event log and source names.

The user selects the appropriate option from the application main menu.

Operations
----------
The are four event log operations that a user can choose from:

1. Add: Adds the specified event logs and sources to the specified machine;
2. Remove: Removes the specified event logs and sources from the specified machine;
3. Check: Checks existence of the specified event logs and sources on the specified machine;
4. List: Lists details of the specified event logs on the specified machine.

The user selects the appropriate operation from a secondary menu.

Warning
-------
This application needs to be run with administrator privileges.  The easiest way to do this is:

1. Right-click on the executable and select **Properties** from the context menu.  The **Properties** dialog will open;

2. In the **Properties** dialog select the **Compatibility** tab;

3. In the **Compatibility** tab, click **Change settings for all users**.  The **Compatibility for all users** dialog will open;

4. In the **Compatibility for all users** dialog, under Settings, tick the **Run this program as an administrator** checkbox;

5. **OK** each of the dialogs.

Why the application needs administrator privileges
--------------------------------------------------
When creating an event log or event source the built-in .NET code has to read all existing event logs.  This includes reading the Security log.  In Windows Vista and later versions of Windows only administrators have permissions to read the Security log.

When removing an event log or event source the built-in .NET code has to edit the registry.  Editing the registry requires administrator privileges.

Format of the config file
-------------------------
The config file contains a custom ConfigurationSection, EventLogsSection.  This section of the config file can contain four optional elements: **add**, **remove**, **checkExistence** and **list**:
<configuration>
  <configSections>
    <section name="eventLogs" type="EventLogManager.CustomConfigSection.EventLogsSection, EventLogManager"/>
  </configSections> 
  <eventLogs>
    <add>
      <eventLogs>
        <eventLog name="MyCustomEventLog">
          <eventSources>
            <eventSource name="CustomEventSource" />
          </eventSources>
        </eventLog>
      </eventLogs>
    </add>
    <remove>
      <eventLogs>
        <eventLog name="MyCustomEventLog" />
      </eventLogs>
      <eventSources>
        <eventSource name="EventSource1" />
        <eventSource name="EventSource2" />
      </eventSources>
    </remove>
    <checkExistence>
      <eventLogs>
        <eventLog name="MyCustomEventLog"/>
      </eventLogs>
      <eventSources>
        <eventSource name="EventSource1"/>
      </eventSources>
    </checkExistence>
    <list>
      <eventLogs>
        <eventLog name="MyCustomEventLog"/>
      </eventLogs>
    </list>
  </eventLogs>  
</configuration>

The **add** element must contain an **eventLogs** collection.  Each **eventLog** in the collection may optionally contain a nested **eventSources** collection with one or more **eventSource** elements.  

In addition to the creating the specified event sources, for each event log a default source will be created automatically with the same name as the log. 

The **remove** element can contain two optional elements, an **eventLogs** collection and an **eventSources** collection.  The **eventLogs** collection, if supplied, should contain at least one **eventLog** element.  The **eventSources** collection, if supplied, should contain at least one **eventSource** element.

When an event log is removed all associated event sources are automatically removed as well; there is no need to explicitly add the event sources to the **remove** element if the event log associated with them is being removed.  

When an event source is being removed only the event source name needs to be specified; the built-in .NET code does not need the associated event log name if it is just deleting an event source.

The **checkExistence** element can contain two optional elements, an **eventLogs** collection and an **eventSources** collection.  The **eventLogs** collection, if supplied, should contain at least one **eventLog** element.  The **eventSources** collection, if supplied, should contain at least one **eventSource** element.

If a specified event log is found to exist all the event sources associated with that log will be listed.  If a specified event source is found to exist the event log associated with it will be displayed.

The **list** element can contain an optional **eventLogs** collection.  The eventLogs should **not** contain nested event sources.  All event sources associated with the event log will be listed, if the event log exists.  

To list all event logs on a machine leave the list element empty: `<list />`.  

**NOTE:** The Application and System event logs may contain hundreds of event sources each.  Be aware of this when listing those event logs, or when listing all event logs.

To operate on the event logs and event sources of another machine, specify the optional **machineName** attribute of the top-level **eventLogs** element:

  <eventLogs machineName="MyServerName">
	:
  </eventLogs>
