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

namespace PixelClicker
{
    /// <summary>
    /// Handles getting pixels from the screen.
    /// </summary>
    [Serializable]
    class Screenshot : IDisposable
    {
        /// <summary>
        /// The portion of the screen.
        /// </summary>
        public Rectangle Portion { private set; get; }

        /// <summary>
        /// The bitmap data from the portion.
        /// </summary>
        private Bitmap m_bitmap;

        /// <summary>
        /// Grabs the pixels from a portion of the screen.
        /// </summary>
        public Screenshot(Rectangle portion)
        {
            Portion = portion;
            m_bitmap = new Bitmap(Portion.Width, Portion.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(m_bitmap))
                g.CopyFromScreen(Portion.Left, Portion.Top, 0, 0, m_bitmap.Size, CopyPixelOperation.SourceCopy);
        }

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

        public static bool operator ==(Screenshot a, Screenshot b)
        {
            //Double check that portions are the same.
            if (a.Portion != b.Portion)
                return false;

            //Check bitmap pixels.
            Bitmap
                ab = a.m_bitmap,
                bb = b.m_bitmap;

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

        public static bool operator !=(Screenshot a, Screenshot b) => !(a == b);

        public void Dispose()
        {
            m_bitmap.Dispose();
        }
    }
}
