using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class InternetServiceInfo
    {
        public InternetServiceStatus Status;
        public int ProfileID, RX_Count, TX_Count, Acknowledged_Data, Not_Acknowledged_Data;

        public InternetServiceInfo()
        {
            this.Acknowledged_Data = this.Not_Acknowledged_Data = this.RX_Count = this.TX_Count = this.ProfileID = -1;
            this.Status = InternetServiceStatus.unkonwn;
        }
    }
}
