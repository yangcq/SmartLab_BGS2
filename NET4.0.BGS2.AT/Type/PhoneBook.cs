using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class PhoneBook
    {
        public string LocationID, Number, Name;
        public CallNumberType NumberType;

        public PhoneBook()
        {
            this.LocationID = "-1";
            this.Number = this.Name = "";
            this.NumberType = CallNumberType.unknown;
        }

        public static string GetPhoneBookStorageString(PhoneBookStorage storage)
        {
            switch (storage)
            {
                case PhoneBookStorage.CPHS_voice_mailbox_phonebook: return "VM";
                case PhoneBookStorage.fixed_dialing_phonebook: return "FD";
                case PhoneBookStorage.last_number_dialed_phonebook: return "LD";
                case PhoneBookStorage.missed: return "MC";
                case PhoneBookStorage.mobile_equipment_phonebook: return "ME";
                case PhoneBookStorage.MSISDN_list: return "ON";
                case PhoneBookStorage.received_call_list: return "RC";
                case PhoneBookStorage.SIM_phonebook: return "SM";
                default: return "UNKNOW";
            }
        }

        /// <summary>
        /// Convert string to PhoneBook Type Class
        /// </summary>
        /// <param name="storageString">this has to be enclosed in "", for example "SM"</param>
        /// <returns></returns>
        public static PhoneBookStorage GetPhoneBookStorageType(string storageString)
        {
            switch (storageString)
            {
                case "\"FD\"": return PhoneBookStorage.fixed_dialing_phonebook;
                case "\"SM\"": return PhoneBookStorage.SIM_phonebook;
                case "\"ON\"": return PhoneBookStorage.MSISDN_list;
                case "\"ME\"": return PhoneBookStorage.mobile_equipment_phonebook;
                case "\"LD\"": return PhoneBookStorage.last_number_dialed_phonebook;
                case "\"MC\"": return PhoneBookStorage.missed;
                case "\"RC\"": return PhoneBookStorage.received_call_list;
                case "\"VM\"": return PhoneBookStorage.CPHS_voice_mailbox_phonebook;
                default: return PhoneBookStorage.unkonow;
            }
        }
    }
}