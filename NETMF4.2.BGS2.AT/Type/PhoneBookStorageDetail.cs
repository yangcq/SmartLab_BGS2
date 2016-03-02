using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    public class PhoneBookStorageDetail
    {
        public int Total, Used;

        /// <summary>
        /// Current Stroage Location
        /// </summary>
        public PhoneBookStorage Storage;

        public PhoneBookStorageDetail()
        {
            this.Total = this.Used = -1;
            this.Storage = PhoneBookStorage.unkonow;
        }
    }
}