using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWindowManager
{
    using ConsoleClassLibrary;
    class ConsoleWindowManager
    {

        public ConsoleBufferManager CurrentBuffer
        {
            get { return Windows[CW]; }
        }

        Dictionary<string, ConsoleBufferManager> Windows = new Dictionary<string, ConsoleBufferManager>();
        string CW; //The current active window
        public ConsoleWindowManager(string currentWindow = "main")
        {
            CW = currentWindow;
            Windows.Add(currentWindow, new ConsoleBufferManager());
        }


        public void NewWindow()
        {
        }

        public void SwitchWindow()
        {
            
        }
        public void NewBuffer(string bufId, bool switchTo = true)
        {
            Windows.Add(bufId, new ConsoleBufferManager());
            if (switchTo)
            {
                SwitchBuffer(bufId);
            }
            CW = bufId;
        }
        public void SwitchBuffer(string bufId)
        {
            CF.SetConsoleActiveScreenBuffer(Windows[bufId].Handle);
            CW = bufId;
        }

        public IEnumerable<string> ListBuffers()
        {
            IEnumerable<string> list = Windows.Keys;
            return list;
        }

        public void CopyFrom(string Buffer1, string Buffer2)
        {

        }


    }
}
