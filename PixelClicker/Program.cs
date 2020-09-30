using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsAPI;

namespace PixelClicker
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Serialize portions of the screen as files.
            //TODO: Compare that porions of the screen match serialized files.

            //Add Python scripting functionality.

            //Create script to test the BOYUM addon.
            OptionSet os = new OptionSet()
            {
                { "m|mouse", "Displays the current mouse posiiton on the screen.", v => DisplayCurrentMousePosition() },
                { "c|console", "Opens a Python console to execute commands.", v => PythonConsole() },
            };
            os.Parse(args);
        }

        /// <summary>
        /// Displays a Python console to the user
        /// </summary>
        private static void PythonConsole()
        {
            ScriptEngine py = Python.CreateEngine();
            ScriptScope scope = py.CreateScope();
            
            bool consoleRunning = true;

            //Add quit command
            Action quit = new Action(() => consoleRunning = false);
            scope.SetVariable("exit", quit);
            scope.SetVariable("quit", quit);

            while (consoleRunning)
            {
                try
                {
                    //Read command string from user.
                    if (Console.CursorLeft != 0)//Before printing >>> make sure it's on a new line.
                        Console.WriteLine();
                    Console.Write(">>> ");
                    string input = Console.ReadLine();

                    //Execute the command.
                    ObjectHandle objHand = py.ExecuteAndWrap(input, scope);

                    //See if there's a result.
                    object result = objHand.Unwrap();
                    Console.Write(result == null ? string.Empty : result.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Displays the current mouse position to the console.
        /// </summary>
        private static void DisplayCurrentMousePosition()
        {
            Point p = default;
            int printedLength = 0;//The current amount of buffer chars that are used.
            Console.CursorVisible = false;
            while (true)
            {
                Point current = MouseMover.GetMousePosition();
                if (current != p)
                {
                    p = current;
                    string toPrint = p.ToString();
                    int newLength = toPrint.Length;
                    Console.CursorLeft = 0;
                    Console.Write(toPrint);
                    for (int i = newLength; i < printedLength; i++)
                        Console.Write(' ');
                    printedLength = newLength;
                }
            }
        }

    }
}
