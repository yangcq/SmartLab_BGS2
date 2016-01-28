
namespace SmartLab.BGS2.Type
{
    public class InternetRequestResponse
    {
        public InternetServiceInfo Info;
        public InternetError Error;
        public string Body;

        public bool IsSuccess
        {
            get
            {
                if (Error != null)
                {
                    return Error.InfoID == 0 ? true : false;
                }
                return false;
            }
        }
    }
}