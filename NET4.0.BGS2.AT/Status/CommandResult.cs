
namespace SmartLab.BGS2.Status
{
    public enum CommandResult
    {
        not_included = -1,
        command_executed_no_errors = 0,
        link_established = 1,
        ring_detected = 2,
        link_not_established_or_disconnected = 3,
        invalid_command_or_command_line_too_long = 4,
        no_dial_Tone_dialling_impossible_wrong_mode = 6,
        remote_station_busy = 7,
        no_answer = 8,
        link_with_2400_bps_and_radio_link_protocol = 47,
        link_with_4800_bps_and_radio_link_protocol = 48,
        link_with_9600_bps_and_radio_link_protocol = 49,
        link_with_14400_bps_and_radio_link_Pprotocol = 50,
        alerting_at_called_phone = 99,
        mobile_phone_is_dialing = 100,
    }
}