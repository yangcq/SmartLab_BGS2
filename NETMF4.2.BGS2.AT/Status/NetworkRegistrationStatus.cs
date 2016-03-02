
namespace SmartLab.BGS2.Status
{
    public enum NetworkRegistrationStatus
    {
        not_registered = 0,
        registered_to_home_network = 1,
        not_registered_is_currently_searching = 2,
        registration_denied = 3,
        unknown = 4,
        registered_roaming = 5,
    }
}
