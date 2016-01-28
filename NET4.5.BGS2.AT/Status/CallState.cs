using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLab.BGS2.Status
{
    public enum CallState
    {
        unknown = -1,
        active = 0,
        held = 1,
        dialing = 2,
        alerting = 3,
        incoming = 4,
        waiting = 5,
    }
}
