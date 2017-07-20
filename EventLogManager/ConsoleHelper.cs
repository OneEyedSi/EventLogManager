using System;
using System.Security;
using DisplayHelper;

namespace EventLogManager
{
    public class ConsoleHelper
    {
        public static void DisplaySecurityMessage(SecurityException ex)
        {
            DisplaySecurityMessage(ex, null);
        }

        public static void DisplaySecurityMessage(SecurityException ex, string errorMessage)
        {
            bool wrapText = true;
            bool includeNewLine = true;

            if (errorMessage == null)
            {
                errorMessage = "";
            }
            else if (errorMessage.Trim().Length > 0)
            {
                errorMessage = "  " + errorMessage.Trim();
            }

            Console.WriteLine();

            ConsoleColor originalTextColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleDisplayHelper.ShowHeadedText(0,
                "SecurityException:"
                + errorMessage
                + "  You need to run this application as Administrator."
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
            Console.ForegroundColor = originalTextColour;
        }

        public static void DisplayErrorMessage(string message)
        {
            DisplayColorMessage(message, ConsoleColor.Red);
        }

        public static void DisplayWarningMessage(string message)
        {
            DisplayColorMessage(message, ConsoleColor.Yellow);
        }

        public static void DisplayColorMessage(string message, ConsoleColor textColour)
        {
            ConsoleColor originalTextColour = Console.ForegroundColor;
            Console.ForegroundColor = textColour;
            Console.WriteLine(message);
            Console.ForegroundColor = originalTextColour;
        }
    }
}