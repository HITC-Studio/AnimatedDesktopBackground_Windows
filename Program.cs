﻿using System;
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
            Image img = Image.FromFile("Background.gif");            
            FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]); //gets the GUID
            int frameCount = img.GetFrameCount(dimension); //total frames in the animation

            int index = 0;

            // Loop forever
            while (true)
            {
                // Grab this index frame
                img.SelectActiveFrame(dimension, index);
                // Convert to a bitmap
                Bitmap frame = (Bitmap)img.Clone();

                // Save this bitmap
                frame.Save("displayImage.bmp");

                // Set the background
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());

                string tempPath = Path.Combine(Application.StartupPath, "displayImage.bmp");
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                // Grab the time duration of this frame
                PropertyItem item = img.GetPropertyItem(0x5100);
                int delay = (item.Value[0] + item.Value[1] * 256) * 10;

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