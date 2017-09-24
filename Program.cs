using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace AnimatedBackground
{    
    static class Program
    {
        // DLL Import
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        // Consts
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread t = new Thread(AnimatedBackground);
            t.Start();            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run();
        }

        private static void AnimatedBackground()
        {
            Thread.CurrentThread.IsBackground = true;            

            int index = 0;
            // Grab this index frame
            Image img = Image.FromFile("Background.gif");
            FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]); //gets the GUID
            int frameCount = img.GetFrameCount(dimension); //total frames in the animation

            // Save each frame
            for (int i = 0; i < frameCount; i++)
            {
                img.SelectActiveFrame(dimension, i);

                // Convert to a bitmap
                Bitmap frame = (Bitmap)img.Clone();
                // Save this bitmap
                frame.Save("displayImage" + i + ".bmp");
                frame.Dispose();
                frame = null;
            }

            // Set the background
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", 1.ToString());
            key.SetValue(@"TileWallpaper", 0.ToString());
            key.Dispose();
            key = null;

            // Loop forever
            while (true)
            {                
                string tempPath = Path.Combine(Application.StartupPath, "displayImage" + index + ".bmp");
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                tempPath = null;

                // Grab the time duration of this frame
                img.SelectActiveFrame(dimension, index);
                PropertyItem item = img.GetPropertyItem(0x5100);
                int delay = (item.Value[0] + item.Value[1] * 256);// *10;
                item = null;

                // Sleep for the frame duration
                Thread.Sleep(delay);

                // Next frame
                index++;
                if (index >= frameCount)
                    index = 0;
            }
        }

    }
}
