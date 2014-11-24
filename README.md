EventLogManager
===============

C# console application of utilities for working with event logs:
	1) Add: Adds event logs and sources specified in the config file to the specified machine;
	2) Remove: Removes event logs and sources specified in the config file from the specified machine;
	3) Check: Checks existence of the event logs and sources specified in the config file;
	4) List: Lists details of the event logs specified in the config file.

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

    <eventLogs >
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

The **add** element must contain an **eventLogs** collection.  Each **eventLog** in the collection contains a nested **eventSources** collection.  The eventSources collection must contain at least one **eventSource** element, otherwise an exception will be thrown.

The **remove** element can contain two optional elements, an **eventLogs** collection and an **eventSources** collection.  The **eventLogs** collection should contain at least one **eventLog** element.  The **eventSources** collection should contain at least one **eventSource** element.

When an event log is removed all associated event sources are automatically removed as well; there is no need to explicitly add the event sources to the **remove** element if the event log associated with them is being removed.  When an event source is being removed only the event source name needs to be specified; the built-in .NET code does not need the associated event log name if it is just deleting an event source.

The **checkExistence** element can contain two optional elements, an **eventLogs** collection and an **eventSources** collection.  The **eventLogs** collection should contain at least one **eventLog** element.  The **eventSources** collection should contain at least one **eventSource** element.

If an event log is found to exist all the event sources associated with that log will be listed.  If an event source is found to exist the event log associated with it will be displayed.

The **list** element can must contain an **eventLogs** collection.  The eventLogs should **not** contain nested event sources.  All event sources associated with the event log will be listed.  To list all event logs on a machine leave the list element empty: <list />.  

**NOTE:** The Application and System event logs may contain hundreds of event sources each.  Be aware of this when listing those event logs, or when listing all event logs.

To operate on the event logs and event sources of another machine, specify the optional **machineName** attribute of the top-level **eventLogs** element:

    <eventLogs machineName="MyServerName">
	  :
	</eventLogs>
