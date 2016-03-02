namespace SmartLab.BGS2.Type
{
    public class GeneralResponse
    {
        /* Apply to AT+CMEE=1 or AT+CMEE=2
         * public CommandResult Result;
         * public BGS2ErrorCode ErrorCode;
         * public BGS2SMSErrorCode SMSError;
         */
        public string[] PayLoad;

        public bool IsSuccess;

        public GeneralResponse()
        {
            IsSuccess = false;

            /*
            Result = CommandResult.not_included;
            ErrorCode = BGS2ErrorCode.not_included;
            SMSError = BGS2SMSErrorCode.not_included;
            */
        }

        public void Reset() 
        {
            this.IsSuccess = false;
            this.PayLoad = null;
        }
    }
}