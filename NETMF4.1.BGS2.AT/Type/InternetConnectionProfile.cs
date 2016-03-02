using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    /// <summary>
    /// <conParmTag> value CSD GPRS0
    /// "conType" mandatory mandatory
    /// "user" optional optional
    /// "passwd" optional optional
    /// "apn" ø optional
    /// "inactTO" optional optional
    /// "calledNum" mandatory ø
    /// "dataType" mandatory ø
    /// "dns1" optional optional
    /// "dns2" optional optional
    /// "alphabet" optional optional
    /// </summary>
    public class InternetConnectionProfile
    {
        /// <summary>
        /// Internet connection profile identifier.
        /// The "conProfileId" identifies all parameters of a connection profile, and,
        /// when a service profile is created with AT^SISS the "conProfileId" needs
        /// to be set as "conId" value of the AT^SISS parameter "srvParmTag".
        /// </summary>
        public int profileID;

        /// <summary>
        /// Type of Internet connection.
        /// For supported values of "conParmValue" refer to "conParmValue-conType".
        /// </summary>
        public InternetConnectionType conType;

        /// <summary>
        /// Selects the character set for input and output of string parameters within a profile.
        /// The selected value is bound to the specific profile. This means that different
        /// profiles may use different alphabets. Unlike other parameters the alphabet can be changed no matter whether the "conParmTag" value "conType" has been set.
        /// </summary>
        public string alphabet;
        
        /// <summary>
        /// User name string: maximum 31 characters (where "" is default).
        /// </summary>
        public string user;
        
        /// <summary>
        /// Password string: maximum 31 characters (where ***** is default).
        /// </summary>
        public string passwd;
        
        /// <summary>
        /// Access point name string value: maximum 99 characters (where "" is default).
        /// </summary>
        public string apn;
        
        /// <summary>
        /// Inactivity timeout value in seconds: 0 ... 216-1, default = 20
        /// Number of seconds the bearer remains open although the service no longer needs the bearer connection.
        /// Do not set the timeout value below 3 sec. This may result in problems when
        /// using the "eodFlag" (set in the last AT^SISW command to terminate an upload data stream).
        /// </summary>
        public string inactTO;

        /// <summary>
        /// Called BCD number.
        /// </summary>
        public string calledNum;

        /// <summary>
        /// Data call type. For supported values of "conParmValue" refer to "conParmValuedataType".
        /// </summary>
        public string dataType;

        /// <summary>
        /// Primary DNS server address (IP address in dotted-four-byte format).
        /// This value determines whether to use the DNS server addresses dynamically
        /// assigned by the network or a specific DNS server address given by the user.
        /// "dns1" = "0.0.0.0" (default) means that the CSD or GPRS connection profile
        /// uses dynamic DNS assignment. Any other address means that the Primary DNS is manually set.
        /// The default value applies automatically if no other address is set. Note that the
        /// AT^SICS read command only returns a manually configured IP address, while the value "0.0.0.0" is not indicated at all,
        /// no matter whether assumed by default or explicitly specified.
        /// </summary>
        public string dns1;

        /// <summary>
        /// Secondary DNS server address (IP address in dotted-four-byte format).
        /// If "dns1" = "0.0.0.0" this setting will be ignored. Otherwise this value can be used to manually configure an alternate server for the DNS1.
        /// If "dns1" is not equal "0.0.0.0" and no "dns2" address is given, then "dns2"="0.0.0.0" will be assumed automatically. The AT^SICS read command
        /// only returns a manually configured IP address, while the value "0.0.0.0" is not indicated at all, no matter whether assumed by default or explicitly specified.
        /// </summary>
        public string dns2;

        /// <summary>
        /// if CSD calledNum and dataType also required
        /// </summary>
        /// <param name="ProfileID"></param>
        /// <param name="conType"></param>
        public InternetConnectionProfile(int ProfileID, InternetConnectionType conType)
        {
            this.profileID = ProfileID;
            this.conType = conType;
            user = passwd = apn = inactTO = calledNum = dataType = dns1 = dns2 = alphabet = "";
        }

        public static string GetInternetConnectionString(InternetConnectionType type)
        {
            switch (type)
            {
                case InternetConnectionType.CSD: return "CSD";
                case InternetConnectionType.GPRS0: return "GPRS0";
                default: return "none";
            }
        }

        public static InternetConnectionType GetInternetConnectionType(string type)
        {
            switch (type)
            {
                case "\"CSD\"": return InternetConnectionType.CSD;
                case "\"GPRS0\"": return InternetConnectionType.GPRS0;
                default: return InternetConnectionType.none;
            }
        }
    }
}
