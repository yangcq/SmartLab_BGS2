
namespace SmartLab.BGS2.Status
{
    public enum InternetReadStatus
    {
        data_transfer_has_been_finished = -2,
        querying_available_bytes_is_not_supported = -1,
        no_further_data_is_available_at_the_moment = 0,
        data_is_available = 1,
    }
}
