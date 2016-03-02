namespace SmartLab.BGS2.Type
{
    public class InternetError
    {
        /// <summary>
        /// 0 means no error
        /// </summary>
        public int ID;

        public int ProfileID;

        public string Text;

        public InternetError()
        {
            this.ProfileID = -1;
            this.ID = -1;
            this.Text = string.Empty;
        }
    }
}
