using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class InternetConnectionInfo
    {
        public int ProfileID, NumberOfServices;
        public string IPAddress;
        public InternetConnectionStatus Status;

        public InternetConnectionInfo()
        {
            Status = InternetConnectionStatus.unknown;
            ProfileID = NumberOfServices = -1;
            IPAddress = "0.0.0.0";
        }
    }
}
