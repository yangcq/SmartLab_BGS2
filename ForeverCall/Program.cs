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
        static CoreBGS2 bgs;
        static AutoResetEvent waitEvent;

        static void Main(string[] args)
        {
            waitEvent = new AutoResetEvent(false);

            bgs = new CoreBGS2("COM3");

            bgs.OnCurrentCallInfo += bgs_OnCurrentCallInfo;

            bgs.Start();

            while (!bgs.Network_Service_Status)
                Thread.Sleep(10000);

            while (true) 
            {
                bgs.Call("07857939269");
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
