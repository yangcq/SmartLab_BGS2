
namespace SmartLab.BGS2.Status
{
    public enum SMSOverflowStatus
    {
        space_available = 0,
        SMS_buffer_full = 1,
        buffer_full_new_SMS_waiting_in_SC = 2,
        unknown = 3,
    }
}