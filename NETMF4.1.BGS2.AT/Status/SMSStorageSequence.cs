
namespace SmartLab.BGS2.Status
{
    public enum SMSStorageSequence
    {
        memory_firat_then_SIM_card = 0,
        SIM_card_first_then_memory = 1,
        storage_sequence_changed = 2,
        unbale_to_change_sequence = 3,
        unable_to_retrieve = 4,
    }
}