namespace SmartLab.BGS2.Type
{
    public class InternetError
    {
        /// <summary>
        /// 0 means no error
        /// </summary>
        public int ID;

        public int ProfileID;

        public string InfoText;

        public InternetError()
        {
            this.ProfileID = -1;
            this.ID = -1;
            this.InfoText = string.Empty;
        }
    }
}
