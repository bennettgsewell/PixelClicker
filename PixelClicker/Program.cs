using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        /// <summary>
        /// The tasks the program can perform.
        /// </summary>
        enum Task
        {
            None,
            DisplayMousePos,
            PythonConsole,
            Screenshot,
            Script
        }

        static void Main(string[] args)
        {
            //The task the user is going to execute.
            Task task = Task.None;

            //Selected region of the screen.
            Rectangle region = default;

            //Output file location.
            string file = null;

            //Path to script file.
            string script = null;

            //Parse command line arguments.
            OptionSet os = new OptionSet()
            {
                { "m|mouse", "Displays the current mouse posiiton on the screen.", v => task = Task.DisplayMousePos },
                { "c|console", "Opens a Python console to execute commands.", v => task = Task.PythonConsole },
                { "s|screenshot=", "Takes a screenshot and saves it to a .ss file. s=x,y,width,height", v =>
                    {
                        task = Task.Screenshot;
                        string[] parts = v.Split(',');
                        if(parts.Length != 4)
                            throw new ArgumentException("-s incorrectly formatted, expected \"-s=x,y,width,height\"");
                        region = new Rectangle(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                },
                { "f|file=", "The output file path", v =>
                    {
                        if(Uri.IsWellFormedUriString(v, UriKind.RelativeOrAbsolute))
                            file = v;
                        else
                            throw new ArgumentException("-file is incorrectly formatted.");
                    }
                },
                { "p|python|script=", "Executes a Python script.", (v) =>
                    {
                        task = Task.Script;
                        if(Uri.IsWellFormedUriString(v, UriKind.RelativeOrAbsolute))
                            script = v;
                        else
                            throw new ArgumentException("Script file path is incorrectly formatted.");
                    }
                },
            };

            try
            {
                os.Parse(args);
            }
            catch (ArgumentException ar)
            {
                Console.Error.WriteLine(ar.Message);
            }

            switch (task)
            {
                case Task.DisplayMousePos:
                    DisplayCurrentMousePosition();
                    break;
                case Task.PythonConsole:
                    PythonConsole();
                    break;
                case Task.Script:
                    PythonScript(script);
                    break;

                case Task.Screenshot:
                    if (file == null)
                    {
                        Console.Error.WriteLine("-file=path_to_file must be set to take a screenshot.");
                        return;
                    }
                    using (Screenshot ss = new Screenshot(region))
                        ss.SaveToFile(file);
                    break;
            }
        }

        /// <summary>
        /// Creates a Python scode with functions.
        /// </summary>
        /// <returns></returns>
        private static ScriptScope CreatePythonScope(ScriptEngine py)
        {
            ScriptScope scope = py.CreateScope();

            //Take Screenshot command.
            Func<int, int, int, int, Screenshot> screenshot = new Func<int, int, int, int, Screenshot>((x, y, width, height) =>
            {
                return new Screenshot(new Rectangle(x, y, width, height));
            });
            scope.SetVariable("screen", screenshot);
            scope.SetVariable("screenshot", screenshot);
            scope.SetVariable("ss", screenshot);

            //Save screenshot command.
            Action<Screenshot, string> saveScreenshot = new Action<Screenshot, string>((ss, file) => ss.SaveToFile(file));
            scope.SetVariable("saveScreen", saveScreenshot);
            scope.SetVariable("saveScreenshot", saveScreenshot);
            scope.SetVariable("saveSS", saveScreenshot);

            //Open Screenshot command.
            Func<string, Screenshot> loadScreenshot = new Func<string, Screenshot>((path) =>
            {
                try
                {
                    return Screenshot.OpenFile(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return null;
                }
            });
            scope.SetVariable("openScreen", loadScreenshot);
            scope.SetVariable("openScreenshot", loadScreenshot);
            scope.SetVariable("openSS", loadScreenshot);

            //Close Screenshot, Disposes of the screenshot.
            Action<Screenshot> disposeSS = new Action<Screenshot>((ss) => ss?.Dispose());
            scope.SetVariable("disposeSS", disposeSS);
            scope.SetVariable("closeSS", disposeSS);
            scope.SetVariable("closeScreen", disposeSS);
            scope.SetVariable("closeScreenshot", disposeSS);

            //Compare screenshot to region of screen.
            scope.SetVariable("compareSS", new Func<Screenshot, bool>((ss) =>
            {
                Rectangle region = ss.Portion;
                using(Screenshot screen = new Screenshot(region))
                {
                    return ss == screen;
                }
            }));

            //Click the screen.
            scope.SetVariable("click", new Action<int, int>(Mouse.LeftClick));
            scope.SetVariable("rightClick", new Action<int, int>(Mouse.RightClick));

            //Causes looping beep
            scope.SetVariable("alarm", new Action(() =>
            {
                while (true)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    Thread.Sleep(1000);
                }
            }));

            return scope;
        }

        /// <summary>
        /// Executes a Python script.
        /// </summary>
        private static void PythonScript(string file)
        {
            ScriptEngine py = Python.CreateEngine();
            ScriptScope scope = CreatePythonScope(py);
            py.ExecuteFile(file, scope);
        }

        /// <summary>
        /// Displays a Python console to the user
        /// </summary>
        private static void PythonConsole()
        {
            ScriptEngine py = Python.CreateEngine();
            ScriptScope scope = CreatePythonScope(py);

            bool consoleRunning = true;

            //Add quit command.
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
