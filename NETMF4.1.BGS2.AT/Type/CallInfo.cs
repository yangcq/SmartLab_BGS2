using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class CallInfo
    {
        public int CallIndex;
        public CallDir Dir;
        public CallState Status;
        public CallMode CallMode;
        public bool IsMultipartyConferenceCall;
        public string Number, EntryInPhonebook;
        public CallNumberType NumberType;

        public CallInfo()
        {
            this.CallIndex = -1;
            this.Dir = CallDir.unknown;
            this.Status = CallState.unknown;
            this.CallMode = CallMode.unknownn;
            this.IsMultipartyConferenceCall = false;
            this.Number = this.EntryInPhonebook = "-1";
            this.NumberType = CallNumberType.unknown;
        }
    }
}