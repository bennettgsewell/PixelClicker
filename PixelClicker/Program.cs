using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                { "m|mouse", "Displays the current mouse posiiton on the screen.", v => DisplayCurrentMousePosition() }
            };
            os.Parse(args);
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
