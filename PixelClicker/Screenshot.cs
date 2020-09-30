using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelClicker
{
    /// <summary>
    /// Handles getting pixels from the screen.
    /// </summary>
    [Serializable]
    public sealed class Screenshot : IDisposable
    {
        /// <summary>
        /// The portion of the screen.
        /// </summary>
        public Rectangle Portion { private set; get; }

        /// <summary>
        /// The bitmap data from the portion.
        /// </summary>
        public Bitmap TheBitMap { private set; get; }

        /// <summary>
        /// Grabs the pixels from a portion of the screen.
        /// </summary>
        public Screenshot(Rectangle portion)
        {
            Portion = portion;
            TheBitMap = new Bitmap(Portion.Width, Portion.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(TheBitMap))
                g.CopyFromScreen(Portion.Left, Portion.Top, 0, 0, TheBitMap.Size, CopyPixelOperation.SourceCopy);
        }

        /// <summary>
        /// Returns all of the screens connected to this machine.
        /// </summary>
        public static Screen[] Screens => Screen.AllScreens;

        /// <summary>
        /// Serializes the screenshot to a stream.
        /// </summary>
        public void Serialize(Stream output)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(output, this);
        }

        /// <summary>
        /// Serializes the screenshot to a file.
        /// </summary>
        public void SaveToFile(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create))
                Serialize(fs);
        }

        public void SavePNG(string path)
        {
            TheBitMap.Save(path, ImageFormat.Png);
        }

        /// <summary>
        /// Deserializes the screenshot from a stream.
        /// </summary>
        public static Screenshot Deserialize(Stream input)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (Screenshot)bf.Deserialize(input);
        }

        /// <summary>
        /// Deserializes a screenshot from a file.
        /// </summary>
        public static Screenshot OpenFile(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Open))
                return Deserialize(fs);
        }

        /// <summary>
        /// Compares two screenshots pixel by pixel to make sure they match.
        /// </summary>
        public static bool operator ==(Screenshot a, Screenshot b)
        {
            //Double check that portions are the same.
            if (a.Portion != b.Portion)
                return false;

            //Check bitmap pixels.
            Bitmap
                ab = a.TheBitMap,
                bb = b.TheBitMap;

            if (ab.Size != bb.Size || ab.Size != a.Portion.Size)
                return false;

            int width = ab.Width;
            int height = ab.Height;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Color aColor = ab.GetPixel(x, y);
                    Color bColor = bb.GetPixel(x, y);

                    if (aColor.R != bColor.R ||
                        aColor.G != bColor.G ||
                        aColor.B != bColor.B)
                        return false;
                }
            return true;
        }

        /// <summary>
        /// Compares two screenshots pixel by pixel to make sure they don't match.
        /// </summary>
        public static bool operator !=(Screenshot a, Screenshot b) => !(a == b);

        public void Dispose()
        {
            TheBitMap.Dispose();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode() => Portion.GetHashCode() + TheBitMap.GetHashCode();
    }
}
