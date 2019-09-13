using System;
using System.Drawing;

namespace ConsoleWindowManager
{
    using ConsoleClassLibrary;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Uses virtual terminal Processing to make prrttyyy pictures
    /// </summary>
    class ConsoleBufferManager
    {
        //Constant definitions for types
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;

        private const string ESC = "\x1b";
        private const string CSI = "\x1b[";
        private const string OSC = "\x1b]";
        private const string DEC = "\x1b(";
        private const string BEL = "\x07";

        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x00000004;
        private readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public readonly bool VTModeEnabled;

        public IntPtr Handle { get; private set; }
        public Size ScreenSize
        {
            get { 
                var v = GetInfo().dwSize;
                return new Size(v.X, v.Y);
            }
        }


        private bool EnableVTMode()
        {

            if (!CF.GetConsoleMode(Handle, out uint dwMode))
            {
                return false;
            }
            dwMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            if(!CF.SetConsoleMode(Handle, dwMode))
            {
                return false;
            }

            return true;
        }

        public ConsoleBufferManager()
        {
            Handle = CF.CreateConsoleScreenBuffer(GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, IntPtr.Zero, 1, IntPtr.Zero);

            if(Handle == INVALID_HANDLE_VALUE)
            {
                throw new Exception("ERROR: FAILED TO CREATE CONSOLE BUFFER");
            }

            //Attempt to enable virtual terminal mode. If this fails. Only basic (non-vt) instructions are supported.
            VTModeEnabled = EnableVTMode();
            if (VTModeEnabled)
            {
                ChangeTitle("Boi this is so colo");
            }
        }

        /// <summary>
        /// Displays some text on the screen using virtual terminal sequences
        /// </summary>
        /// <param name="Text">Text to display</param>
        /// <param name="Fore">Foreground Color</param>
        /// <param name="Back">Background Color</param>
        /// <param name="Underline">Draws a line under the text</param>
        /// <param name="Bold">Makes the foreground color bolder</param>
        /// <returns>The number of characters displayed</returns>
        public uint VT_Write(string Text, Color? Fore = null, Color? Back = null, bool Underline = false,bool Bold = false)
        {

            string data = CSI + (Bold ? "1;" : "") + (Underline ? "4" : "24");

            if (Fore.HasValue)
            {
                Color f = Fore.Value;
                data += ";38;2;" + f.R + ";" + f.G + ";" + f.B;
            }
            if (Back.HasValue)
            {
                Color b = Back.Value;
                data += ";48;2;" + b.R + ";" + b.G + ";" + b.B;
            }

            data += "m" + Text;

            CF.WriteConsole(Handle,data,(uint)data.Length,out uint value,IntPtr.Zero);
            return value;
        }

        private bool Swapped = false;
        public void SwapColors()
        {
            string data = CSI + (Swapped?"27m": "7m");
            Swapped = !Swapped;
            CF.WriteConsole(Handle, data, (uint)data.Length, out _, IntPtr.Zero);
        }

        public void ResetAll()
        {
            string data = CSI + "!p";
            CF.WriteConsole(Handle, data, (uint)data.Length, out _, IntPtr.Zero);
        }

        /*----------------------------------------------------------------------------------------------------------------------*/
        /*----------------------------------------------------PALETTE STUFF-----------------------------------------------------*/
        /*----------------------------------------------------------------------------------------------------------------------*/
        /*----------------------------------------------------------------------------------------------------------------------*/
        public enum PaletteOptions
        {
            Black = 0,
            Gray= 8,
            Blue= 1,
            Light_Blue= 9,
            Green= 2,
            Light_Green= 0xA,
            Aqua= 3,
            Light_Aqua= 0xB,
            Red= 4,
            Light_Red= 0xC,
            Purple= 5,
            Light_Purple= 0xD,
            Yellow= 6,
            Light_Yellow= 0xE,
            White= 7,
            Light_White= 0xF
        };
        public void UpdatePalette(PaletteOptions p, Color color)
        {
            KeyValuePair<PaletteOptions, Color>[] ca = { new KeyValuePair<PaletteOptions, Color>(p,color) };
            UpdatePalette(ca);
        }

        public void UpdatePalette(KeyValuePair< PaletteOptions, Color>[] o)
        {
            string b = "";

            foreach(KeyValuePair<PaletteOptions,Color> el in o)
            {
                b += (";4;" + el.Key.ToString() + ";" + el.Value.R + "; " + el.Value.G + "; " + el.Value.B);
            }

            b += ESC;

            CF.WriteConsole(Handle, b, (uint)b.Length, out _, IntPtr.Zero);
        }
        /*----------------------------------------------------------------------------------------------------------------------*/
        /*----------------------------------------------------------------------------------------------------------------------*/
        /*----------------------------------------------------PALETTE STUFF-----------------------------------------------------*/
        /*----------------------------------------------------------------------------------------------------------------------*/
        public enum CharacterSet
        {
            LineDrawing = '0', //Uses jklmnqtuvwx To drawlines
            Default = 'C'
        }
        public void ChangeCharacterSet(CharacterSet c = CharacterSet.Default)
        {
            string data = DEC +((char)c);
            CF.WriteConsole(Handle,data, (uint)data.Length, out _, IntPtr.Zero);
        }

        public void ChangeTitle(string Title)
        {
            string data = OSC + Title + BEL;
            CF.WriteConsole(Handle, data, (uint)data.Length, out _, IntPtr.Zero);
        }

        public uint Write(string Text)
        {
            CF.WriteConsole(Handle, Text, (uint)Text.Length, out uint value, IntPtr.Zero);
            return value;
        }

        public void VT_WriteAt(string Text,int x, int y, Color? Fore = null, Color? Back = null, bool Underline = false, bool Bold = false)
        {
            VT_MoveCursor(x, y);
            VT_Write(Text,Fore,Back,Underline,Bold);
        }
        public enum ClearSetting
        {
            ClearAll = '2',
            ClearFromCursor = '0',
            ClearBehindCursor = '1'
        };
        public void VT_Clear(ClearSetting c = ClearSetting.ClearAll)
        {
            if(c == ClearSetting.ClearAll)
            {
                VT_MoveCursor(0, 0);
                c = ClearSetting.ClearFromCursor;
            }
            string Text = CSI + (char)c + 'J';
            CF.WriteConsole(Handle, Text, (uint)Text.Length, out _, IntPtr.Zero);
        }
        public void VT_ClearLine(ClearSetting c = ClearSetting.ClearAll)
        {
            string Text = CSI + (char)c + 'K';
            CF.WriteConsole(Handle, Text, (uint)Text.Length, out _, IntPtr.Zero);
        }

        public void VT_MoveCursor(int x,int y)
        {
            string Text = CSI + y + ";" + x + "H";
            CF.WriteConsole(Handle, Text, (uint)Text.Length, out _, IntPtr.Zero);
        }

        public void VT_FillArea(string pattern, Rectangle area, Color? Fore = null, Color? Back = null, bool Underline = false, bool Bold = false)
        {
            StringBuilder Line = new StringBuilder(area.Width*area.Height);
            for(int i = 0; i<= area.Width; i += pattern.Length)
            {
                Line.Append(pattern);
            }
            string l = Line.ToString().Substring(0, area.Width);

            for(int i = area.Top;i<area.Bottom;i++)
            {
                VT_WriteAt(l,area.Left,i,Fore,Back,Underline,Bold);
            }
        }
        public void ClearArea()
        {

        }
        public void VT_FillScreen(string pattern, Color? Fore = null, Color? Back = null)
        {
            CF.CONSOLE_SCREEN_BUFFER_INFO_EX inf = GetInfo();
            int dwSize = inf.dwSize.X * inf.dwSize.Y;
            StringBuilder b = new StringBuilder(dwSize);
            for(int i = 0; i< dwSize; i += pattern.Length)
            {
                b.Append(pattern);
            }

            VT_MoveCursor(0, 0);
            VT_Write(b.ToString().Substring(0,dwSize),Fore,Back);
            VT_MoveCursor(inf.dwCursorPosition.X, inf.dwCursorPosition.Y);
        }
        public CF.CONSOLE_SCREEN_BUFFER_INFO_EX GetInfo()
        {
            CF.CONSOLE_SCREEN_BUFFER_INFO_EX buf = new CF.CONSOLE_SCREEN_BUFFER_INFO_EX();
            buf.cbSize = (uint)Marshal.SizeOf(buf);
            CF.GetConsoleScreenBufferInfoEx(Handle,ref buf);
            return buf;
        }
        /*----------------------------------------------------------------------------------------------------------------------*/
        /*----------------------------------------------------------------------------------------------------------------------*/
        /*----------------------------------------------------Y U DO DIS!!?-----------------------------------------------------*/
        /*----------------------------------------------------------------------------------------------------------------------*/
        public void VT_Image_Raster_Image(Bitmap b)
        {

            CF.CONSOLE_SCREEN_BUFFER_INFO_EX inf = GetInfo();
            int width = inf.dwSize.X + 1, height = inf.dwSize.Y + 1;
            Color[,] grid = new Color[width,height]; //Destination Bitmap
            double alpha = 0.4;

            //For each pixel in the source bitmap, work out which destination pixel it fits into. Then: Either Take an average, or set its value
            for (int charX = 0; charX < b.Width; charX++)
            {
                for (int charY = 0; charY < b.Height; charY++)
                {
                    int destX = Math.Min(Convert.ToInt32(charX * ((double)width  / b.Width )),width-1);
                    int destY = Math.Min(Convert.ToInt32(charY * ((double)height / b.Height)),height-1);

                    if (grid[destX, destY].IsEmpty)
                    {
                        grid[destX, destY] = b.GetPixel(charX, charY);
                    }
                    else
                    {   //Do a running average etc.Or better yet base alpha on distance to actual value
                        
                        grid[destX, destY] = Color.FromArgb(255, Convert.ToInt32((grid[destX, destY].R * alpha) + (b.GetPixel(charX, charY).R * (1 - alpha))),
                                                                 Convert.ToInt32((grid[destX, destY].G * alpha) + (b.GetPixel(charX, charY).G * (1 - alpha))),
                                                                 Convert.ToInt32((grid[destX, destY].B * alpha) + (b.GetPixel(charX, charY).B * (1 - alpha))));
                    }
                }
            }

            StringBuilder d = new StringBuilder((grid.Length * 16) + (height*7) );
            for(int y = 0; y < height; y++)
            {
                d.Append(CSI + y + ";" + 0 + "H");
                for (int x = 0; x < width; x++)
                {
                    d.Append(CSI + ";48;2;" + grid[x, y].R + ";" + grid[x, y].G + ";" + grid[x, y].B + "m ");
                }
                
            }
            Write(d.ToString());
        }
    }
}
