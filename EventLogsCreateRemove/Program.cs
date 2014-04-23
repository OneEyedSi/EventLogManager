using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenuLibrary;

namespace EventLogsCreateRemove
{
    /// <summary>
    /// Application that creates or removes event logs and event sources.
    /// </summary>
    /// <remarks>The application must be run as administrator (right click on executable and 
    /// select "Run as administrator" from the context menu.  If it is not run as administrator 
    /// then it will not have the required permissions to operate on event logs.
    /// 
    /// The event logs and sources to create or delete are specified in the config file.
    /// 
    /// The application can operate on either the local machine it's running on or a remote 
    /// machine.  To operate on a remote machine specify the machineName attribute of the 
    /// eventLogs element in the config file.  Note that to operate on a remote machine you 
    /// will need the appropriate permissions.
    /// 
    /// When an event log is removed all its associated event sources are removed as well; 
    /// there is no need to remove the event sources separately.</remarks>
    class Program
    {
        static void Main(string[] args)
        {
            MenuGenerator.Run();
        }
    }
}
