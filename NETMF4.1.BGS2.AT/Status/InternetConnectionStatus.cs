namespace SmartLab.BGS2.Status
{
    public enum InternetConnectionStatus
    {
        down_internet_connection_is_defined_but_not_connected = 0,
        connecting_a_service_has_been_opened_and_internet_connection_is_initated = 1,
        up_internet_connection_is_established_and_usable = 2,
        limited_up_internet_connection_is_established_but_temporarily_no_coverage = 3,
        closing_internet_connection_is_terminating = 4,
        unknown = 5,
    }
}
