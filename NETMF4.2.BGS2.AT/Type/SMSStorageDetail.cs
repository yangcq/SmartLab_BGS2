using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class SMSStorageDetail
    {
        public SMSStorage Storage;
        public string Total, Used;

        public SMSStorageDetail()
        {
            Storage = SMSStorage.unknown;
            Total = Used = "-1";
        }

        public static SMSStorage GetSMSStorageType(string s)
        {
            switch (s)
            {
                case "\"MT\"": return SMSStorage.memory_plus_SIM;
                case "\"ME\"": return SMSStorage.memory;
                case "\"SM\"": return SMSStorage.SIM_card;
                default: return SMSStorage.unknown;
            }
        }

        public static string GetSMSStorageString(SMSStorage s)
        {
            switch (s)
            {
                case SMSStorage.memory: return "\"ME\"";
                case SMSStorage.memory_plus_SIM: return "\"MT\"";
                case SMSStorage.SIM_card: return "\"SM\"";
                default: return "\"unknown\"";
            }
        }
    }
}