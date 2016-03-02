﻿
namespace SmartLab.BGS2.Status
{
    public enum BGS2SMSErrorCode
    {
        not_included = -1,
        unassigned_unallocated_number = 1,
        operator_determined_barring = 8,
        call_barred = 10,
        short_message_transfer_rejected = 21,
        destination_out_of_service = 27,
        unidentified_subscriber = 28,
        facility_rejected = 29,
        unknownn_subscriber = 30,
        network_out_of_order = 38,
        temporary_failure = 41,
        congestion = 42,
        resources_unavailable_unspecified = 47,
        requested_facility_not_subscribed = 50,
        requested_facility_not_implemented = 69,
        invalid_short_message_transfer_reference_value = 81,
        invalid_message_unspecified = 95,
        invalid_mandatory_information = 96,
        message_type_non_existent_or_not_implemented = 97,
        message_not_compatible_with_short_message_protocol_state = 98,
        information_element_non_existent_or_not_implemented = 99,
        protocol_error_unspecified = 111,
        interworking_unspecified = 127,
        telematic_interworking_not_supported = 128,
        short_message_type_0_not_supported = 129,
        cannot_replace_short_message = 130,
        unspecified_TP_PID_error = 143,
        data_coding_scheme_alphabet_not_supported = 144,
        message_class_not_supported = 145,
        unspecified_TP_DCS_error = 159,
        command_cannot_be_actioned = 160,
        command_unsupported = 161,
        unspecified_TP_command_error = 175,
        TPDU_not_supported = 176,
        SC_busy = 192,
        no_SC_subscription = 193,
        SC_system_failure = 194,
        invalid_SME_address = 195,
        destination_SME_barred = 196,
        SM_rejected_duplicate_SM = 197,
        TP_VPF_not_supported = 198,
        TP_VP_not_supported = 199,
        do_SIM_SMS_storage_full = 208,
        no_SMS_storage_capability_in_SIM = 209,
        error_in_MS = 210,
        memory_capacity_exceeded = 211,
        SIM_application_toolkit_busy = 212,
        SIM_data_download_error = 213,
        unspecified_error_cause = 255,
        ME_failure = 300,
        SMS_service_of_ME_reserved = 301,
        operation_not_allowed = 302,
        operation_not_supported = 303,
        invalid_PDU_mode_parameter = 304,
        invalid_text_mode_parameter = 305,
        SIM_not_inserted = 310,
        SIM_PIN_required = 311,
        PH_SIM_PIN_required = 312,
        SIM_failure = 313,
        SIM_busy = 314,
        SIM_wrong = 315,
        SIM_PUK_required = 316,
        SIM_PIN2_required = 317,
        SIM_PUK2_required = 318,
        memory_failure = 320,
        invalid_memory_index = 321,
        memory_full = 322,
        SMSC_address_unknownn = 330,
        no_network_service = 331,
        network_timeout = 332,
        no_CNMA_acknowledgement_expected = 340,
        unknownn_error = 500,
        user_abort = 512,
        unable_to_store = 513,
        invalid_status = 514,
        invalid_character_in_address_string = 515,
        invalid_length = 516,
        invalid_character_in_PDU = 517,
        invalid_parameter = 518,
        invalid_length_or_character = 519,
        invalid_character_in_text = 520,
        timer_expired = 521,
        operation_temporary_not_allowed = 522,
    }
}