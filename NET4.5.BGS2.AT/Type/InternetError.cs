namespace SmartLab.BGS2.Type
{
    public class InternetError
    {
        public int ProfileID;

        /// <summary>
        /// 0 means no error
        /// </summary>
        public int InfoID;

        public string InfoText;

        public InternetError()
        {
            this.ProfileID = -1;
            this.InfoID = -1;
            this.InfoText = string.Empty;
        }
    }
}
