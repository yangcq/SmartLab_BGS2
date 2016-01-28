using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLab.BGS2.Status
{
    public enum CallMode
    {
        voice = 0,
        data = 1,
        fax = 2,
        voice_followed_by_data_voice_mode = 3,
        alternating_voice_data_voice_mode = 4,
        alternating_voice_fax_voice_mode = 5,
        voice_followed_by_data_data_mode = 6,
        alternating_voice_data_data_mode = 7,
        alternating_voice_fax_fax_mode = 8,
        unknownn = 9,
    }
}
