using System.Drawing;
using System;
namespace UltimateConsoleUI
{
    using ConsoleWindowManager;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using static ConsoleWindowManager.ConsoleBufferManager;

    class Program
    {
        static ConsoleWindowManager c = new ConsoleWindowManager();

        static IEnumerable<string> GetDirectories(string path,string match, SearchOption searchOptions)
        {
            List<string> files = new List<string>();
            foreach(string s in match.Split('|'))
            {
                foreach (string p in Directory.EnumerateFiles(path, s, searchOptions)){
                    files.Add(p);
                }
            }
            return files;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("beganig");
            Thread.Sleep(1000);

            c.NewBuffer("cheese");
            c.SwitchBuffer("cheese");
            c.CurrentBuffer.VT_Write("hello there\n\r",Color.Green,Color.Pink);
            c.CurrentBuffer.Write("this is more text \n\r\n\r");
            c.CurrentBuffer.ResetAll();
            c.CurrentBuffer.Write("this is more text again\n\r\n\r");
            Thread.Sleep(1000);
            c.CurrentBuffer.VT_MoveCursor(0, 0);
            c.CurrentBuffer.Write("\x1b[31mThis text has a red foreground using SGR.31.\r\n");
            c.CurrentBuffer.Write("\x1b[1mThis text has a bright (bold) red foreground using SGR.1 to affect the previous color setting.\r\n");
            c.CurrentBuffer.Write("\x1b[34;46mThis text shows the foreground and background change at the same time.\r\n");

            Thread.Sleep(1000);

            c.CurrentBuffer.VT_Clear();

            Thread.Sleep(1000);

            c.CurrentBuffer.ChangeCharacterSet(CharacterSet.LineDrawing);
            c.CurrentBuffer.VT_WriteAt("jklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwxjklmnqtuvwx", 0,0,Color.Indigo,Color.OliveDrab);

            Thread.Sleep(1000);

            c.CurrentBuffer.VT_FillScreen("jklmnqtuvwx", Color.AntiqueWhite, Color.Navy);
            Thread.Sleep(1000);

            Size windowSize = c.CurrentBuffer.ScreenSize;

            c.CurrentBuffer.ChangeCharacterSet(CharacterSet.Default);
            c.CurrentBuffer.VT_FillArea(" ", new Rectangle(15, 5, 36, 10), null, Color.Green);
            c.CurrentBuffer.VT_FillArea(" ", new Rectangle(0, windowSize.Height-2, windowSize.Width, 3), null, Color.LemonChiffon);
            Thread.Sleep(5000);

            foreach(string fpath in GetDirectories(@"B:\Users\Idris\Pictures", "*.jpg|*.png|*.bmp", SearchOption.AllDirectories)){
                try 
                {
                    Image i = Image.FromFile(fpath);
                    Bitmap b = new Bitmap(i);
                    c.CurrentBuffer.VT_Image_Raster_Image(b);
                    Thread.Sleep(8000);
                }
                catch 
                {   //Probably means it's not an image 
                    c.CurrentBuffer.VT_FillScreen("FAIL", Color.White, Color.Red);
                }
            }
            
            c.SwitchBuffer("main");
            Console.WriteLine("endnig");
            Thread.Sleep(1000);
        }
    }
}
