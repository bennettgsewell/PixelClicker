﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelClicker
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Grab portions of the screen, remove mouse pointer.
            //TODO: Serialize portions of the screen as files.
            //TODO: Compare that porions of the screen match serialized files.

            //TODO: Function to move mouse.
            //TODO: Function click screen.

            //Add Python scripting functionality.

            //Create script to test the BOYUM addon.

            //1250*470
        }

        /// <summary>
        /// Displays the current mouse position to the console.
        /// </summary>
        private static void DisplayCurrentMousePosition()
        {
            Point p = default;
            while (true)
            {
                Point current = MouseMover.GetMousePosition();
                if (current != p)
                {
                    p = current;
                    Console.WriteLine(p.ToString());
                    Console.Clear();
                    Console.Write(p);
                }
            }
        }
    }
}
