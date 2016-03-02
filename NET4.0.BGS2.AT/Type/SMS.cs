using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class SMS
    {
        public string MessageID, Sender, Message, Date, Time;
        public SMSMessageStatus Status;

        public SMS()
        {
            this.MessageID = "-1";
            this.Status = SMSMessageStatus.unknown;
        }


        public static string GetSMSStatusString(SMSMessageStatus status)
        {
            switch (status)
            {
                case SMSMessageStatus.received_unread_messages: return "\"REC UNREAD\"";
                case SMSMessageStatus.received_read_messages: return "\"REC READ\"";
                case SMSMessageStatus.stored_unsent_messages: return "\"STO UNSENT\"";
                case SMSMessageStatus.stored_sent_messages: return "\"STO SENT\"";
                case SMSMessageStatus.all_messages: return "\"ALL\"";
                default: return "unknown";
            }
        }

        public static SMSMessageStatus GetSMSStatusType(string status)
        {
            switch (status)
            {
                case "\"REC UNREAD\"": return SMSMessageStatus.received_unread_messages;
                case "\"REC READ\"": return SMSMessageStatus.received_read_messages;
                case "\"STO UNSENT\"": return SMSMessageStatus.stored_unsent_messages;
                case "\"STO SENT\"": return SMSMessageStatus.stored_sent_messages;
                case "\"ALL\"": return SMSMessageStatus.all_messages;
                default: return SMSMessageStatus.unknown;
            }
        }
    }
}