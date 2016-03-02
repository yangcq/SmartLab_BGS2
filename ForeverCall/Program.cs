using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartLab.BGS2;
using SmartLab.BGS2.Core;
using SmartLab.BGS2.Status;
using SmartLab.BGS2.Type;

namespace ForeverCall
{
    class Program
    {
        static BGS2Core bgs;
        static AutoResetEvent waitEvent;

        static void Main(string[] args)
        {
            waitEvent = new AutoResetEvent(false);

            bgs = new BGS2Core("COM3");

            bgs.OnCurrentCallInfo += bgs_OnCurrentCallInfo;

            bgs.Start();

            while (!bgs.Network_Service_Status)
                Thread.Sleep(10000);

            while (true) 
            {
                bgs.Call("07440455603");
                waitEvent.WaitOne(30000);
                bgs.Call_Hang_Up();
            }

        }

        static void bgs_OnCurrentCallInfo(CallInfo list)
        {
            if (list.Status == CallState.alerting)
                waitEvent.Set();
        }
    }
}
