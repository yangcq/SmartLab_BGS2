namespace SmartLab.BGS2.Status
{
    public enum InternetServiceStatus
    {
        /// <summary>
        /// not inclued in the documentation
        /// </summary>
        unkonwn = 0,

        /// <summary>
        /// Service profile resources are allocated, i.e. at least the service type has been
        /// set (parameter 'srvParmTag', value "srvType" of AT^SISS). The service is
        /// not opened, but ready for configuration.
        /// </summary>
        allocated = 2,

        /// <summary>
        /// State after opening a service with AT^SISO where the connection is being established.
        /// If connection setup is successful the service proceeds to the state "4" (Up) and one of the URCs "^SISW" and "^SISR" may follow.
        /// If connection setup is not successful, the "^SIS" URC may appear and the service enters 'srvState' 6 (Down).
        /// In the case of FTP, 'srvState'=3 means that the command channel is being established.
        /// If the service profile is configured as Socket listener, then the listener always stays at 'srvState'=3 and 'socketState'=3 (LISTENER),
        /// while the 'srvState' and 'socketState' of the dynamically assigned service profile may change. See examples in Section 10.6.1.
        /// </summary>
        connecting = 3,

        /// <summary>
        /// The service performs its purpose. The data transfer process is the major function at this state.
        /// FTP: Data channel is up.
        /// SMTP: The SMTP service will not enter 'srvState'=4 until the host has written the first data packet with AT^SISW.
        /// Transparent TCP Listener service: the service is Listening to client connects.
        /// </summary>
        up = 4,

        /// <summary>
        /// Internet Service is closing the network connection.
        /// FTP: Command channel is released.
        /// </summary>
        closing = 5,

        /// <summary>
        /// This state is entered if
        /// - the service has successfully finished its session (see note on Socket),
        /// - the remote peer has reset the connection or
        /// - the IP connection has been closed because of an error (see note below on service or network errors).
        /// </summary>
        down = 6,

        /// <summary>
        /// A client tries to connect to the Transparent TCP Listener service.
        /// </summary>
        alert = 7,

        /// <summary>
        /// A client is connected with the Transparent TCP Listener service.
        /// </summary>
        connected = 8,

        /// <summary>
        /// The client has disconnected from the Transparent TCP Listener service but there are unread data. To go back into Up/Listening state read the pending data using AT^SIST or discard them by using AT^SISH.
        /// </summary>
        released = 9,
    }
}
