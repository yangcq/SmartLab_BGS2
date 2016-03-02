
namespace SmartLab.BGS2.Type
{
    public class SMSPerferredStorage
    {
        public SMSStorageDetail Listing_Reading_Deleting;
        public SMSStorageDetail Writing_Sending;
        public SMSStorageDetail Received;

        public SMSPerferredStorage()
        {
            Listing_Reading_Deleting = new SMSStorageDetail();
            Writing_Sending = new SMSStorageDetail();
            Received = new SMSStorageDetail();
        }
    }
}
