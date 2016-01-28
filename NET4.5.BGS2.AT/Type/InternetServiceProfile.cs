using SmartLab.BGS2.Status;

namespace SmartLab.BGS2.Type
{
    /// <summary>
    /// <srvParmTag> Mandatory or optional
    /// Socket service
    /// "srvType" mandatory
    /// "conId" mandatory
    /// "alphabet" optional
    /// "address" mandatory
    /// "tcpMR" optional
    /// "tcpOT" optional
    /// "secOpt" optional
    /// Transparent service
    /// "srvType" mandatory
    /// "conId" mandatory
    /// "alphabet" optional
    /// "address" mandatory
    /// "tcpMR" optional
    /// "tcpOT" optional
    /// "secOpt" optional
    /// FTP service
    /// "srvType" mandatory
    /// "conId" mandatory
    /// "alphabet" optional
    /// "address" mandatory
    /// "tcpMR" optional"tcpOT" optional
    /// HTTP service
    /// "srvType" mandatory
    /// "conId" mandatory
    /// "alphabet" optional
    /// "address" mandatory
    /// "user" optional
    /// "passwd" optional
    /// "hcContent" optional
    /// "hcContLen" optional
    /// "hcUserAgent" optional
    /// "hcMethod" mandatory
    /// "hcProp" optional
    /// "hcRedir" optional
    /// "hcAuth" optional
    /// "tcpMR" optional
    /// "tcpOT" optional
    /// "secOpt" optional
    /// SMTP service
    /// "srvType" mandatory
    /// "conId" mandatory
    /// "alphabet" optional
    /// "address" mandatory
    /// "user" optional
    /// "passwd" optional
    /// "tcpPort" optional
    /// "smFrom" mandatory
    /// "smRcpt" mandatory
    /// "smCC" optional
    /// "smSubj" optional
    /// "smHdr" optional
    /// "smAuth" optional
    /// "tcpMR" optional
    /// "tcpOT" optional
    /// POP3 service
    /// "srvType" mandatory
    /// "conId" mandatory
    /// "alphabet" optional
    /// "address" mandatory
    /// "user" mandatory
    /// "passwd" mandatory
    /// "tcpPort" optional
    /// "pCmd" mandatory
    /// "pNumber" optional
    /// "pLength" optional
    /// "pDelFlag" optional
    /// "tcpMR" optional
    /// "tcpOT" optional
    /// </summary>
    public class InternetServiceProfile
    {
        /// <summary>
        /// Internet service profile identifier.
        /// The "srvProfileId" is used to reference all parameters related to the same service profile. Furthermore,
        /// when using the AT commands AT^SISO, AT^SISR, AT^SISW, AT^SIST, AT^SISH and AT^SISC the 'srvProfileId' is needed to select a specific service profile.
        /// </summary>
        public int profileID;

        /// <summary>
        /// Type of Internet service to be configured with consecutive usage of AT^SISS.
        /// For supported values of "srvParmValue" refer to "srvParmValue-srvType".
        /// </summary>
        public InternetServiceType srvType;

        /// <summary>
        /// User name string.
        /// 1. Socket
        /// Not applicable.
        /// 2. Transparent
        /// Not applicable.
        /// 3. FTP
        /// Not applicable; set within "address" parameter.
        /// 4. HTTP
        /// Length: 2...31
        /// User name for the HTTP authentication mechanism. Currently only HTTP
        /// simple authentication is supported.
        /// 5. SMTP
        /// User name to be used for SMTP authentication (string).
        /// Length: 4...64.
        /// If SMTP authentication is disabled, i.e. "smAuth" flag not set, user name
        /// parameter will be ignored.
        /// 6. POP3
        /// User name identifying a mailbox, i.e. mailbox name (string).
        /// Length: 1...64.
        /// </summary>
        public string user;

        /// <summary>
        /// Password string
        /// 1. Socket
        /// Not applicable.
        /// 2. Transparent
        /// Not applicable.
        /// 3. FTP
        /// Not applicable; set within "address" parameter.
        /// 4. HTTP
        /// Length: 2...31
        /// Password for the HTTP authentication mechanism. Currently HTTP simple
        /// authentication is supported only.
        /// 5. SMTP
        /// Password to be used for SMTP authentication (string).
        /// Length: 4...64.
        /// If SMTP authentication is disabled, i.e. "smAuth" flag not set, password
        /// parameter will be ignored.
        /// 6. POP3
        /// Server/mailbox-specific password (string).
        /// Length: 1...64.
        /// Used to perform authentication with a POP3 server.
        /// </summary>
        public string passwd;

        /// <summary>
        /// Internet connection profile to be used, for details refer AT^SICS
        /// </summary>
        public int conId;

        /// <summary>
        /// TCP Port Number
        /// 1. Socket
        /// Not applicable; set within "address" parameter.
        /// 2. Transparent
        /// Not applicable; set within "address" parameter.
        /// 3. FTP
        /// Not applicable; set within "address" parameter.
        /// 4. HTTP
        /// Not applicable; set within "address" parameter.
        /// If parameter is omitted the service connects to HTTP default port 80.
        /// 5. SMTP
        /// SMTP server TCP port number (numeric)
        /// Length: 0...2e16-1
        /// If this parameter is not set, SMTP default port number 25 is used.
        /// 6. POP3
        /// POP3 server TCP port number (numeric)
        /// Length: 0...2e16-1 If this parameter is not set, POP3 default port number 110 is used.
        /// </summary>
        public string tcpPort;

        /// <summary>
        /// String value, depending on the service type either a URL in the case of Socket,
        /// FTP and HTTP or an address in the case of SMTP and POP3:
        /// 1. Socket
        /// - Socket type TCP client URL
        /// "socktcp://'host':'remotePort'
        /// - Socket type TCP server URL
        /// "socktcp://listener:'localPort'"
        /// - Socket type UDP client URL
        /// "sockudp://'host':'remotePort'[;size='value'][;port='localPort']"
        /// Option "size":
        /// 0: PDU size is variable (default).
        /// 1...1460: Fixed PDU size in bytes.
        /// Option "port":
        /// 0: Port number will be assigned from service (default).
        /// 1...2e16-1: defines the local port number for the UDP client.
        /// 2. Transparent service
        /// - Transparent TCP client
        /// "[socktcp://]'host':'remotePort'[;timer='value'][;etx='etxChar'][;keepidle='value'][;keepcnt='value'][;keepintvl='value']"
        /// - Transparent UDP client
        /// "sockudp://'host':'remotePort'[;timer='value'][;etx='etxChar']"
        /// - Transparent TCP Listener
        /// "[socktcp://]:'localPort'[;timer='value'][;etx='etxChar'][;autoconnect='0|1'][;connecttimeout='value'][;keepidle='value'][;keepcnt='value'][;keepintvl='value'][;addrfilter='filter']"
        /// Supported Options:
        /// - "timer": The parameter configures the Nagle algorithm, which is used in
        /// transparent access mode.
        /// range: 20...[100]...500 milliseconds in steps of 20
        /// - "etx": Specifies the character used to change from transparent access
        /// mode to AT command mode.
        /// range: 1...15,17...255 (16 is not allowed because it is used as DLE(0x10)) 
        /// If parameter is not set no escaping is configured, thus requiring either
        /// +++ or DTR ON-OFF transition for changing to AT command mode. If
        /// value is set, the transmitted bytes are parsed for the DLE (0x10) character
        /// followed by the specified <etxChar> value. If both characters are
        /// found the service returns to AT command mode without transmitting
        /// these two bytes. This behavior differs from +++ handling, where +++ is
        /// transmitted over the air.
        /// If you wish to send DLE characters as normal text string within your payload
        /// data the characters shall be doubled (DLE DLE).
        /// - "keepidle": specifies the TCP parameter TCP_KEEPIDLE (see
        /// RFC1122; not for Transparent UDP client)
        /// range: 1...65535 seconds, 0 disabled (default)
        /// - "keepcnt": specifies the TCP parameter TCP_KEEPCNT (see
        /// RFC1122; not for Transparent UDP client); ignored if option "keepidle" is not set
        /// range: 1...[9]...127
        /// - "keepintvl": specifies the TCP parameter TCP_KEEPINTVL (see
        /// RFC1122; not for Transparent UDP client); ignored if option "keepidle" is not set
        /// range: 1...[75]...255 secondsString value, depending on the service type either a URL in the case of Socket,
        /// FTP and HTTP or an address in the case of SMTP and POP3:
        /// 1. Socket
        /// - Socket type TCP client URL
        /// "socktcp://'host':'remotePort'
        /// - Socket type TCP server URL
        /// "socktcp://listener:'localPort'"
        /// - Socket type UDP client URL
        /// "sockudp://'host':'remotePort'[;size='value'][;port='localPort']"
        /// Option "size":
        /// 0: PDU size is variable (default).
        /// 1...1460: Fixed PDU size in bytes.
        /// Option "port"
        /// 0: Port number will be assigned from service (default).
        /// 1...216-1: defines the local port number for the UDP client.
        /// 2. Transparent service
        /// - Transparent TCP client
        /// "[socktcp://]'host':'remotePort'[;timer='value'][;etx='etx-Char'][;keepidle='value'][;keepcnt='value'][;keepintvl='value']"
        /// - Transparent UDP client
        /// "sockudp://'host':'remotePort'[;timer='value'][;etx='etxChar']"
        /// - Transparent TCP Listener
        /// "[socktcp://]:'localPort'[;timer='value'][;etx='etxChar'][;autoconnect='0|1'][;connecttimeout='value'][;keepidle='value'][;keepcnt='value'][;keepintvl='value'][;addrfilter='filter']"
        /// Supported Options:
        /// - "timer": The parameter configures the Nagle algorithm, which is used in
        /// transparent access mode.
        /// range: 20...[100]...500 milliseconds in steps of 20
        /// - "etx": Specifies the character used to change from transparent access mode to AT command mode.
        /// range: 1...15,17...255 (16 is not allowed because it is used as DLE(0x10)) 
        /// If parameter is not set no escaping is configured, thus requiring either
        /// +++ or DTR ON-OFF transition for changing to AT command mode. If
        /// value is set, the transmitted bytes are parsed for the DLE (0x10) character
        /// followed by the specified 'etxChar' value. If both characters are
        /// found the service returns to AT command mode without transmitting
        /// these two bytes. This behavior differs from +++ handling, where +++ is transmitted over the air.
        /// If you wish to send DLE characters as normal text string within your payload
        /// data the characters shall be doubled (DLE DLE).
        /// - "keepidle": specifies the TCP parameter TCP_KEEPIDLE (see RFC1122; not for Transparent UDP client)
        /// range: 1...65535 seconds, 0 disabled (default)
        /// - "keepcnt": specifies the TCP parameter TCP_KEEPCNT (see RFC1122; not for Transparent UDP client); ignored if option "keepidle" is not set
        /// range: 1...[9]...127
        /// - "keepintvl": specifies the TCP parameter TCP_KEEPINTVL (see RFC1122; not for Transparent UDP client); ignored if option "keepidle" is not set
        /// range: 1...[75]...255 seconds
        /// "http://'server':'port'/'path'" or "http://'server':'ort'/'path'" if profile is
        /// configured for secure connection (see value "secOpt" below).
        /// 'server': FQDN or IP-address
        /// 'path': path of file or directory
        /// 'port': If parameter is omitted the service connects to HTTP default port80.
        /// Refer to "IETF-RFC 2616"
        /// 5. SMTP
        /// SMTP server address (string).
        /// Length: 4...256.
        /// 6. POP3
        /// POP3 server address (string).
        /// Length: 4...256.
        /// </summary>
        public string address;

        /// <summary>
        /// Optional parameter for HTTP method "Post".
        /// Length: 0...127
        /// Can be used to transfer a small amount of data. The content of this string will
        /// only be sent if "hcContLen" = 0. The maximum length of "hcContent" is 127
        /// bytes.
        /// To transmit a larger amount of data "hcContLen" must be set to a non-zero
        /// value. In this case the "hcContent" string will be ignored, and data transmission
        /// from the client to the server is done with AT^SISW.
        /// </summary>
        public string hcContent;

        /// <summary>
        /// Mandatory parameter for HTTP method "Post".
        /// Length: 0...231-1
        /// The content length shall be set in the header of the HTTP "Post" request before
        /// the data part is transferred.
        /// If "hcContLen" = 0 then the data given in the "hcContent" string will be posted.
        /// If "hcContLen" > 0 then the AT^SISW command will be used to send data from
        /// the client to the server. In this case, "hcContLen" specifies the total amount of
        /// data to be sent. The data can be sent in one or several parts. For each part,
        /// the transmission is triggered by the URC "^SISW: x, 1", then the AT^SISW write
        /// command can be executed. After the exact number of bytes are transferred via
        /// the serial interface, the HTTP client will go from service state "Up" to service
        /// state "Closing" (see parameters "srvState" and "srvState" for detail).
        /// Finally, the URC "^SISW: x, 2" indicates that all data have been transferred and
        /// the service can be closed with AT^SISC.
        /// </summary>
        public string hcContLen;

        /// <summary>
        /// The user agent string must be set by the application to identify the mobile. Usually
        /// operation system and software version info is set with this browser identifier.
        /// Length: 0...63
        /// </summary>
        public string hcUsrAgent;

        /// <summary>
        /// HTTP method specification: 0=GET, 1=POST, 2=HEAD.
        /// </summary>
        public InternetServiceMethod hcMethod;

        /// <summary>
        /// Parameter for several HTTP settings.
        /// Length: 0...127
        /// The general format is 'key': "space" 'value' 
        /// Multiple settings can be given separated by "\0d\0a" sequences within the string, do not put them at the end.
        /// Possible 'key' values are defined at HTTP/1.1 Standard RFC 2616.
        /// </summary>
        public string hcProp;

        /// <summary>
        /// This flag controls the redirection mechanism of the BGS2-W acting as HTTP
        /// client (numeric).
        /// If "hcRedir" = 0: No redirection.
        /// If "hcRedir" = 1 (default): The client automatically sends a new HTTP request
        /// if the server answers with a redirect code (range 30x).
        /// </summary>
        public string hcRedir; 

        /// <summary>
        /// HTTP authentication control flag (numeric):
        /// "hcAuth" = 0 (default): To be used if "passwd" and "user" are not required and not set for HTTP.
        /// "hcAuth" = 1: HTTP client will automatically answer on authentication requests
        /// from the server with the current "passwd" and "user" parameter settings. If
        /// these parameters are not specified the BGS2-W will terminate the HTTP connection
        /// and send an indication to the TA.
        /// </summary>
        public string hcAuth;

        /// <summary>
        /// Email sender address, i.e. "MAIL FROM" address (string).
        /// Length: 6...256
        /// A valid address parameter consists of local part and domain name delimited by
        /// a '@' character, e.g. "john.smith@somedomain.de".
        /// </summary>
        public string smFrom;

        /// <summary>
        /// Recipient address of the email, i.e. "RCPT TO" address (string).
        /// Length: 6...256
        /// If multiple recipient addresses are to be supplied the comma character is used
        /// as delimiter to separate individual address values, e.g. "john.smith@somedomain.de,tom.meier@somedomain.de". Some mail servers do not accept recipient
        /// addresses without brackets <>. It is recommended to use the "RCPT TO" variable with brackets.
        /// </summary>
        public string smRcpt;
        
        /// <summary>
        /// CC recipient address of the email (string).
        /// Length: 6...256
        /// If multiple CC recipient addresses are to be supplied the comma character is used as delimiter to separate individual address values, e.g."john.smith@somedomain.de,tom.meier@somedomain.de".
        /// </summary>
        public string smCC;
        
        /// <summary>
        /// Subject content of the email (string).
        /// Length: 0...256
        /// If no subject is supplied the email will be sent with an empty subject.
        /// </summary>
        public string smSubj;
        
        /// <summary>
        /// This parameter, if set, will be appended at the end of the email header section (string).
        /// Length: 0...256
        /// Hence, it serves as a generic header field parameter which allows the user to
        /// provide any email header field. It is the user's responsibility to provide correct
        /// header fields!
        /// String of max. 256 characters. 
        /// Example for multipart MIME messages: "Content-Type: multipart/mixed".
        /// </summary>
        public string smHdr;
        
        /// <summary>
        /// SMTP authentication control flag (numeric).
        /// If "smAuth" = 0 (default): BGS2-W performs action without SMTP authentication.
        /// If "smAuth" = 1: Authentication procedure with the SMTP server will be performed by means of supported authentication methods, using values of "user" 
        /// and "passwd" parameters. If BGS2-W and SMTP server are not able to negotiate an authentication mechanism supported by both parties, the BGS2-W
        /// continues action without authentication.
        /// BGS2-W supports SMTP authentication.
        /// </summary>
        public string smAuth;

        /// <summary>
        /// POP3 user command to be executed by the POP3 service (numeric).
        /// For supported values of 'srvParmValue' refer to 'srvParmValue-pCmd'.
        /// </summary>
        public POPCommand pCmd;

        /// <summary>
        /// Optional message number argument used by the POP3 commands List ("2"),
        /// Retrieve ("3") and Delete ("4"). For POP3 commands see "srvParmTag" value "pCmd".
        /// Length: 0...231-1
        /// If no specific value is set in the service profile, the value "0" is assumed by default, i.e. "pNumber" is disabled.
        /// </summary>
        public string pNumber;
        
        /// <summary>
        /// Maximum message length (string, optional)
        /// Length: 0...2e31-1
        /// "pLength" can be used to specify the length of the message(s) to be retrieved
        /// from or deleted on the POP3 server. If no specific value is set in the service
        /// profile, the default value "0" is assumed, which means that there is no limit on
        /// the message size. 
        /// A warning will be issued inthe following cases:
        /// If "pNumber" > 0 and a specific message to be retrieved from / deleted on
        /// the server is longer than "pLength".
        /// If "pNumber" = 0 and all messages to be retrieved from / deleted on the
        /// server are longer than "pLength".
        /// No warning will be issued in the following cases:
        /// </summary>
        public string pLength;

        /// <summary>
        /// Flag to be used with the POP3 user command Retrieve ("3"). Specifies whether
        /// or not to delete retrieved emails on the server (optional).
        /// For supported values of 'srvParmValue' refer to 'srvParmValuepDelFlag'.
        /// </summary>
        public POPDeleteFlage pDelFlag;

        /// <summary>
        /// Parameter can be used to overwrite the global AT^SCFG parameter "Tcp/MaxRetransmissions" 'tcpMr' for a specific Internet Service connection profile.
        /// If the parameter is not specified the value specified with AT^SCFG will be used.
        /// Supported values "srvParmValue" for this parameter are the same as described for 'tcpMr'.
        /// Setting is not relevant for Internet Service "Socket" with type "UDP".
        /// </summary>
        public string tcpMR;
        
        /// <summary>
        /// Parameter can be used to overwrite the global AT^SCFG parameter "Tcp/OverallTimeout"
        /// 'tcpOt' for a specific Internet Service connection profile. If the
        /// parameter is not specified the value specified with AT^SCFG will be used.
        /// Supported values 'srvParmValue' for this parameter are the same as described for 'tcpOt'.
        /// Setting is not relevant for Internet Service "Socket" with type "UDP".
        /// </summary>
        public string tcpOT;

        /// <summary>
        /// Parameter for secure connection (TLS) settings for following services: TCP
        /// Socket client, Transparent TCP client, HTTP client. Detailed guidelines for
        /// managing the required certificates can be found in [11]. See also AT commands
        /// AT^SIND, AT^SBNW and AT^SBNR.
        /// secOpt = "" (default) - do not use secure connection (TLS)
        /// secOpt = "-1" - use secure connection (TLS) without check certificates
        /// secOpt = "0...10" - use secure connection (TLS) with client or/and server certificate
        /// (client certificate is stored in NVRAM at index 0, server certificates are
        /// stored in NVRAM at certificate indexes from 1 to 10), e.g. "0,1,5,9"
        /// </summary>
        public string secOpt;
        
        /// <summary>
        /// Parameter not supported
        /// </summary>
        public Alphabet alphabet;

        public InternetServiceProfile(int ServiceProfileID, InternetServiceType type, int connectionProfileID)
        {
            this.pDelFlag = POPDeleteFlage.unknown;
            this.alphabet = Alphabet.unknown;
            this.pCmd = POPCommand.unknown;
            this.profileID = ServiceProfileID;
            this.srvType = type;
            this.conId = connectionProfileID;
            user = passwd = tcpPort = address = hcContent = hcContLen = hcUsrAgent = hcProp = hcRedir = hcAuth = smFrom = smRcpt = smCC = smSubj = smHdr = smAuth = pNumber = pLength = tcpMR = tcpOT = secOpt = "";
        }

        public InternetServiceProfile(int ServiceProfileID, InternetServiceType type, InternetConnectionProfile profile)
        {
            this.pDelFlag = POPDeleteFlage.unknown;
            this.alphabet = Alphabet.unknown;
            this.pCmd = POPCommand.unknown;
            this.profileID = ServiceProfileID;
            this.srvType = type;
            this.conId = profile.profileID;
            user = passwd = tcpPort = address = hcContent = hcContLen = hcUsrAgent = hcProp = hcRedir = hcAuth = smFrom = smRcpt = smCC = smSubj = smHdr = smAuth = pNumber = pLength = tcpMR = tcpOT = secOpt = "";
        }

        public static string GetInternetServiceString(InternetServiceType Type)
        {
            switch (Type)
            {
                case InternetServiceType.Ftp: return "Ftp";
                case InternetServiceType.Http: return "Http";
                case InternetServiceType.Pop3: return "Pop3";
                case InternetServiceType.Smtp: return "Smtp";
                case InternetServiceType.Socket: return "Socket";
                case InternetServiceType.Transparent: return "Transparent";
                default: return "none";
            }
        }

        public static InternetServiceType GetInternetServiceType(string type)
        {
            switch (type)
            {
                case "\"Ftp\"": return InternetServiceType.Ftp;
                case "\"Http\"": return InternetServiceType.Http;
                case "\"Pop3\"": return InternetServiceType.Pop3;
                case "\"Smtp\"": return InternetServiceType.Smtp;
                case "\"Socket\"": return InternetServiceType.Socket;
                case "\"Transparent\"": return InternetServiceType.Transparent;
                default: return InternetServiceType.none;
            }
        }
    }
}
