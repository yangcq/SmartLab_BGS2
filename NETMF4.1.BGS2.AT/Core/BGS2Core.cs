using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using SmartLab.BGS2.Status;
using SmartLab.BGS2.Type;
using System.Collections;

namespace SmartLab.BGS2.Core
{
    #region Delegate
    /// <summary>
    /// New SMS received, SMS peek is used so the state of the SMS will not change.
    /// </summary>
    /// <param name="storage">where the sms storaged</param>
    /// <param name="sms">message</param>
    public delegate void NewSMSHandler(SMSStorage storage, SMS sms);

    /// <summary>
    /// AT+CREG serves to monitor the BGS2-W's network registration status. For this purpose the read command or URC presentation mode are available.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="netLac">Two byte location area code in hexadecimal format (e.g. "00C3" equals 195 in decimal).</param>
    /// <param name="netCellId">Two byte cell ID in hexadecimal format.</param>
    public delegate void NetworkRegistrationHandler(NetworkRegistrationStatus status, string netLac, string netCellId);

    public delegate void SMSOverflowHandler(SMSOverflowStatus status);

    public delegate void USSDHandler(USSDStatus status, string message);

    public delegate void CurrentCallHandler(CallInfo info);

    public delegate void StringReceiveHandler(string result);

    public delegate void StatusHandler(bool status);

    public delegate void EnhancedOperatorNameHandler(string operatorName, string serviceProviderName);

    public delegate void NetworkTimeZoneHandler(DateTime time);

    public delegate void IncomingCallHandler(CallInfo call);

    #endregion

    /// <summary>
    /// ATE1 & ATV1 & AT+CMEE=0 are required.
    /// </summary>
    public class BGS2Core
    {
        #region Event
        public event NewSMSHandler OnNewSMSReceived;

        public event NetworkRegistrationHandler OnNetworkRegistrationChanged;

        public event SMSOverflowHandler OnSMSOverflow;

        public event StringReceiveHandler OnSignalStrengthIndicator;

        public event StringReceiveHandler OnSignalBitErrorRateIndicator;

        public event StatusHandler OnServiceAvailabiliryIndicator;

        public event StatusHandler OnSounderActivityIndicator;

        public event StatusHandler OnUnreadReceivedMessageIndicator;

        public event StatusHandler OnCallInProcessIndicator;

        public event StatusHandler OnRoamingIndicator;

        public event StatusHandler OnSMSFullIndicator;

        public event StatusHandler OnAudioActivityIndicator;

        public event StatusHandler OnSimtrayIndicator;

        public event EnhancedOperatorNameHandler OnEnhancedOperatorIndicator;

        public event NetworkTimeZoneHandler OnNetworkTimeZoneIndicator;

        public event IncomingCallHandler OnIncomingCall;

        /// <summary>
        /// AT+CUSD allows to control the handling of Unstructured Supplementary Service Data (USSD) according to 3GPP 
        /// TS 22.090 [29]. Both network and mobile initiated operations are supported. The interaction of this command
        /// with other AT commands based on other supplementary services is described in the related technical specifications.
        /// </summary>
        public event USSDHandler OnUSSDReceived;

        public event CurrentCallHandler OnCurrentCallInfo;
        #endregion

        private SerialPort serialPort;
        private byte[] serialByte = new byte[1];

        /// <summary>
        /// Global status to determine whether funtions can be called.
        /// </summary>
        private bool isRunning = false;

        private BufferedArray buffer;

        private ArrayList data = new ArrayList();

        private GeneralResponse response = new GeneralResponse();

        #region Message Queue
        /// <summary>
        /// Thread control for message control
        /// </summary>
        private AutoResetEvent messageWait = new AutoResetEvent(false);

        /// <summary>
        /// Message queue from event, echo and unwanted response
        /// </summary>
        private Queue messageQueue = new Queue();
        #endregion

        #region Echo Check Related
        /// <summary>
        /// Global status to check whether one at command is echo back
        /// </summary>
        private bool isEchoOK = false;

        /// <summary>
        /// a number increase by 1 each time a command is issued, and checking thread uses this number to determin whether it is responser for this event
        /// </summary>
        private int echoTrackNum = 0;

        /// <summary>
        /// Global status for echo at command.
        /// </summary>
        private string echoString = null;

        /// <summary>
        /// Thread control auto reset: let the calling thread to wait for the payload process
        /// </summary>
        private AutoResetEvent echoCheckWait = new AutoResetEvent(false);

        /// <summary>
        /// Thread control auto reset: let the echo process continue, on demand.
        /// </summary>
        private AutoResetEvent echoCheckControl = new AutoResetEvent(false);
        #endregion

        public BGS2Core(SerialPort serial)
        {
            this.serialPort = serial;
            buffer = new BufferedArray(1024);
        }

        public BGS2Core(string port)
            : this(new SerialPort(port, 230400))
        { }

        public BGS2Core(string port, int baudRate)
            : this(new SerialPort(port, baudRate))
        { }

        public void Start()
        {
            if (isRunning)
                return;

            if (serialPort.IsOpen == false)
                serialPort.Open();

            // error fomat
            Write("ATV1\r");
            Thread.Sleep(100);
            // echo
            Write("ATE1\r");
            Thread.Sleep(100);
            // only OK or ERROR
            Write("AT+CMEE=0\r");
            Thread.Sleep(100);

            while (serialPort.BytesToRead > 0)
                ReadLine();

            isRunning = true;
            new Thread(CommandEchoCheckThread).Start();
            new Thread(MessageProcessThread).Start();
            this.serialPort.DataReceived += serialPort_DataReceived;
            
            SendATCommand("AT^SCKS=1");
            SendATCommand("AT+CREG=2");
            SendATCommand("AT+CMGF=1");
            SendATCommand("AT+CNMI=1,1,2,2");
            SendATCommand("AT^SMGO=1");
            SendATCommand("AT+CGREG=1");
            SendATCommand("AT^SM20=0");
            SendATCommand("AT^SLCC=1");
            SendATCommand("AT+CUSD=1");
            SendATCommand("AT^SIND=BATTCHG,0");
            SendATCommand("AT^SIND=SIGNAL,1");
            SendATCommand("AT^SIND=SERVICE,1");
            SendATCommand("AT^SIND=MESSAGE,1");
            SendATCommand("AT^SIND=CALL,1");
            SendATCommand("AT^SIND=ROAM,1");
            SendATCommand("AT^SIND=SMSFULL,1");
            SendATCommand("AT^SIND=RSSI,1");
            SendATCommand("AT^SIND=AUDIO,1");
            SendATCommand("AT^SIND=EONS,1");
            SendATCommand("AT^SIND=NITZ,1");
            SendATCommand("AT^SIND=SIMTRAY,1");
            SendATCommand("AT+CSCS=\"GSM\"");
            SendATCommand("AT+CMER=2,0,0,2");
            SendATCommand("AT+COPS=0");
            SendATCommand("AT^SCFG=\"TCP/WITHURCS\",\"OFF\"");
        }

        #region Low Level Serial Data process
        private void Write(string command)
        {
            lock (serialPort)
            {
                byte[] _command = UTF8Encoding.UTF8.GetBytes(command);
                serialPort.Write(_command, 0, _command.Length);
            }
        }

        private void Write(byte[] command, int offset, int length)
        {
            lock (serialPort)
            {
                serialPort.Write(command, offset, length);
            }
        }

        private void Write(byte command)
        {
            lock (serialPort)
            {
                serialByte[0] = command;
                serialPort.Write(serialByte, 0, 1);
            }
        }

        /// <summary>
        /// read one line from the serial port which ends with \r\n
        /// </summary>
        /// <returns>string which exclude the terminator \r\n</returns>
        private string ReadLine()
        {
            lock (serialPort)
            {
                buffer.Rewind();

                while (true)
                {
                    serialPort.Read(serialByte, 0, 1);
                    if (serialByte[0] == 0x0D)
                    {
                        serialPort.Read(serialByte, 0, 1);
                        if (serialByte[0] == 0x0A)
                        {
                            if (buffer.GetPosition() == 0)
                                return string.Empty;

                            byte[] tempArray = new byte[buffer.GetPosition()];
                            System.Array.Copy(buffer.GetFrameData(), tempArray, tempArray.Length);

                            return new string(UTF8Encoding.UTF8.GetChars(tempArray));
                            //return new string(UTF8Encoding.UTF8.GetChars(buffer.GetFrameData(), 0, buffer.GetPosition()));
                        }

                        buffer.Rewind();
                        continue;
                    }

                    buffer.SetContent(serialByte[0]);
                }
            }
        }
        #endregion

        /// <summary>
        /// AT Command Echo check
        /// </summary>
        private void CommandEchoCheckThread()
        {
            while (isRunning)
            {
                echoCheckControl.WaitOne();
                buffer.Rewind();

                lock (serialPort)
                {
                    while (true)
                    {
                        serialPort.Read(serialByte, 0, 1);
                        buffer.SetContent(serialByte[0]);
                        int position = buffer.GetPosition();

                        if (buffer.GetFrameData()[position - 1] == 0x0D)
                        {
                            // this must be the 0x0D 0x0A as the begining
                            if (position == 1)
                            {
                                serialPort.Read(serialByte, 0, 1);
                                buffer.Rewind();
                                continue;
                            }

                            byte[] tempArray = new byte[buffer.GetPosition() - 1];
                            System.Array.Copy(buffer.GetFrameData(), tempArray, tempArray.Length);

                            string _echo = new string(UTF8Encoding.UTF8.GetChars(tempArray));
                            //string _echo = new string(UTF8Encoding.UTF8.GetChars(buffer.GetFrameData(), 0, position - 1));

                            if (_echo == null)
                            {
                                buffer.Rewind();
                                continue;
                            }

                            if (_echo.IndexOf(echoString) >= 0)
                            {
                                isEchoOK = true;
                                break;
                            }
                            else
                            {
                                // read one more 0x0D 0x0A
                                serialPort.Read(serialByte, 0, 1);
                                if (serialByte[0] == 0x0A)
                                {
                                    lock (messageQueue)
                                    {
                                        messageQueue.Enqueue(_echo);
                                    }
                                    messageWait.Set();
                                }

                                buffer.Rewind();
                            }
                        }
                    }
                    //while (serialPort.BytesToRead > 0) ;
                }

                echoCheckWait.Set();
            }
        }

        /// <summary>
        /// General AT Command
        /// </summary>
        /// <param name="command">AT Command that starts with "AT", no need the return "\r"</param>
        /// <param name="addtionalData1">SMS data only!</param>
        /// <param name="addtionalData2">Internet write data only!</param>
        /// <returns></returns>
        public GeneralResponse SendATCommand(string command, string addtionalData1 = null, string addtionalData2 = null)
        {
            if (!isRunning)
                return null;

            response.Reset();

            // check if "AT" is included
            if (command.ToUpper().IndexOf("AT") < 0)
                return response;

            // remove the event
            this.serialPort.DataReceived -= serialPort_DataReceived;

            // send the command
            Write(command);
            Write(0x0D);

            // set echo check options
            isEchoOK = false;
            echoTrackNum++;
            echoString = command;

            // release the echo check
            echoCheckControl.Set();
            // block for MAX 10 * 1000 ms for the echo to finish.
            //echoCheckWait.WaitOne();
            echoCheckWait.WaitOne(10000, false);

            if (!isEchoOK)
            {
                this.serialPort.DataReceived += serialPort_DataReceived;
                response.Reset();
                return response;
            }

            if (addtionalData1 != null)
            {
                Write(addtionalData1);
                Write(0x1A);

                while (true)
                {
                    serialPort.Read(serialByte, 0, 1);
                    if (serialByte[0] == 0x1A)
                        break;
                }
            }

            // try read one line from the serial port, because payload always start with \r\n so it is alway be ""
            if (ReadLine().Length != 0)
            {
                this.serialPort.DataReceived += serialPort_DataReceived;
                response.Reset();
                return response;
            }

            data.Clear();

            // setep 1: keep reading without ending check.
            // at least read one line, so this will work with the write data
            do
            {
                data.Add(ReadLine());
            }
            while (serialPort.BytesToRead > 0);

            if (addtionalData2 != null)
                Write(addtionalData2);

            // setep 2: start from botton, check any ending words,
            // if no words detected, check from above and remove any that is not
            bool isfound = false;
            int endindex = 0;
            int searchend = 0;

            do
            {
                for (endindex = data.Count - 1; endindex >= searchend; endindex--)
                {
                    switch (data[endindex].ToString())
                    {
                        case "OK": // 0 command executed, no errors
                        case "CONNECT": //  1 link established
                        case "RING": //  2 ring detected
                        case "NO CARRIER": //  3 link not established or disconnected
                        case "ERROR": //  4 invalid command or command line too long
                        case "NO DIALTONE": //  6 no dial tone, dialling impossible, wrong mode
                        case "BUSY": //  7 remote station busy
                        case "NO ANSWER": //  8 no answer
                        case "CONNECT 2400/RLP": //  47 link with 2400 bps and Radio Link Protocol
                        case "CONNECT 4800/RLP": //  48 link with 4800 bps and Radio Link Protocol
                        case "CONNECT 9600/RLP": //  49 link with 9600 bps and Radio Link Protocol
                        case "CONNECT 14400/RLP": //  50 link with 14400 bps and Radio Link Protocol
                        case "ALERTING": //  alerting at called phone
                        case "DIALING": //  mobile phone
                            isfound = true;
                            break;
                    }

                    if (isfound)
                        break;
                }

                // end string is one of the above
                if (endindex == data.Count - 1)
                    break;

                // no end string is detected, we have to read more
                if (endindex < searchend)
                {
                    searchend = data.Count;
                    data.Add(ReadLine());
                }
                else // end string is somewhere in the middle
                {
                    // remove them from the payload
                    while (endindex < data.Count - 1)
                    {
                        string unwanted = data[endindex + 1].ToString();
                        data.RemoveAt(endindex + 1);
                        if (unwanted.Length == 0)
                            continue;

                        lock (messageQueue)
                        {
                            messageQueue.Enqueue(unwanted);
                        }
                    }
                    break;
                }
            }
            while (true);

            switch (data[data.Count - 1].ToString())
            {
                case "OK": // 0 command executed, no errors
                    // remove a blank line just before the status if there is one
                    data.RemoveAt(data.Count - 1);
                    if (data.Count > 0)
                        data.RemoveAt(data.Count - 1);
                    response.IsSuccess = true;
                    break;
                case "CONNECT": //  1 link established
                case "RING": //  2 ring detected
                case "NO CARRIER": //  3 link not established or disconnected
                case "ERROR": //  4 invalid command or command line too long
                case "NO DIALTONE": //  6 no dial tone, dialling impossible, wrong mode
                case "BUSY": //  7 remote station busy
                case "NO ANSWER": //  8 no answer
                case "CONNECT 2400/RLP": //  47 link with 2400 bps and Radio Link Protocol
                case "CONNECT 4800/RLP": //  48 link with 4800 bps and Radio Link Protocol
                case "CONNECT 9600/RLP": //  49 link with 9600 bps and Radio Link Protocol
                case "CONNECT 14400/RLP": //  50 link with 14400 bps and Radio Link Protocol
                case "ALERTING": //  alerting at called phone
                case "DIALING": //  mobile phone
                    response.IsSuccess = false;
                    data.RemoveAt(data.Count - 1);
                    break;
                default:
                    response.IsSuccess = false;
                    break;
            }
            // Step 3: process

            //while (!(data[data.Count - 1].Equals("OK") || data[data.Count - 1].Equals("ERROR")));


            // Current error code using AT+CMEE=0
            // So the last line of a correct response can only be OK and ERROR

            // check the command result code, the last line
            // there are three types of result when AT+CMEE=1 or AT+CMEE=2
            // 1. Words: OK, ERROR, etc.
            // 2. +CME: 
            // 3. +CMS:
            /*
            if (lastline.Contains("+CME ERROR"))
            {
                string[] _cme = lastline.Split(':');
                if (_cme.Length == 2)
                    response.ErrorCode = (BGS2ErrorCode)int.Parse(_cme[1]);
            }
            else if (lastline.Contains("+CMS ERROR"))
            {
                string[] _cms = lastline.Split(':');
                if (_cms.Length == 2)
                    response.SMSError = (BGS2SMSErrorCode)int.Parse(_cms[1]);
            }
            else
            {
                data.RemoveAt(data.Count - 1);
                switch (lastline)
                {
                    case "OK":
                        response.Result = CommandResult.command_executed_no_errors;
                        if (data.Count > 0)
                            data.RemoveAt(data.Count - 1);
                        break;
                    case "CONNECT":
                        response.Result = CommandResult.link_established;
                        break;
                    case "RING":
                        response.Result = CommandResult.ring_detected;
                        break;
                    case "NO CARRIER":
                        response.Result = CommandResult.link_not_established_or_disconnected;
                        break;
                    case "ERROR":
                        response.Result = CommandResult.invalid_command_or_command_line_too_long;
                        break;
                    case "NO DIALTONE":
                        response.Result = CommandResult.no_dial_Tone_dialling_impossible_wrong_mode;
                        break;
                    case "BUSY":
                        response.Result = CommandResult.remote_station_busy;
                        break;
                    case "NO ANSWER":
                        response.Result = CommandResult.no_answer;
                        break;
                    case "CONNECT 2400/RLP":
                        response.Result = CommandResult.link_with_2400_bps_and_radio_link_protocol;
                        break;
                    case "CONNECT 4800/RLP":
                        response.Result = CommandResult.link_with_4800_bps_and_radio_link_protocol;
                        break;
                    case "CONNECT 9600/RLP":
                        response.Result = CommandResult.link_with_9600_bps_and_radio_link_protocol;
                        break;
                    case "CONNECT 14400/RLP":
                        response.Result = CommandResult.link_with_14400_bps_and_radio_link_Pprotocol;
                        break;
                    case "ALERTING alerting at called phone":
                        response.Result = CommandResult.alerting_at_called_phone;
                        break;
                    case "DIALING":
                        response.Result = CommandResult.mobile_phone_is_dialing;
                        break;
                        // non of these conditions are match, so this is not the right last status line.
                    default:
                        break;
                }
            }
            */

            // add payload to the response
            response.PayLoad = (string[])data.ToArray(typeof(string));

            /*
            If an AT command is finished (with "OK" or "ERROR") the TE shall always wait at least 100 ms before sending
            the next one. This applies to bit rates of 9600 bps or higher (see AT+IPR).
            */
            Thread.Sleep(100);

            // add the event back
            this.serialPort.DataReceived += serialPort_DataReceived;

            return response;
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.BytesToRead > 0)
            {
                string URCstring = ReadLine();

                if (URCstring.Length == 0)
                    continue;

                lock (messageQueue)
                {
                    messageQueue.Enqueue(URCstring);
                }
            }

            messageWait.Set();
        }

        private void MessageProcessThread()
        {
            while (isRunning)
            {
                messageWait.WaitOne();

                lock (messageQueue)
                {
                    while (messageQueue.Count > 0)
                    {
                        string message = messageQueue.Dequeue().ToString();

                        if (message == null)
                            continue;

                        //System.Diagnostics.Debug.WriteLine(message);
                        int start = message.IndexOf(": ");
                        if (start > 0)
                        {
                            string[] values = message.Substring(start + 2).Split(',');
                            switch (message.Substring(0, start))
                            {
                                // New SMS
                                case "+CMTI":
                                    if (OnNewSMSReceived != null)
                                    {
                                        if (values.Length == 2)
                                        {
                                            SMS sms = SMS_Peek(values[1]);
                                            sms.MessageID = values[1];
                                            OnNewSMSReceived(SMSStorageDetail.GetSMSStorageType(values[0]), sms);
                                        }
                                    }
                                    break;
                                // Network Registration Status
                                case "+CREG":
                                    if (OnNetworkRegistrationChanged != null)
                                    {
                                        if (values.Length == 1)
                                            OnNetworkRegistrationChanged((NetworkRegistrationStatus)int.Parse(values[0]), "-1", "-1");
                                        else
                                            OnNetworkRegistrationChanged((NetworkRegistrationStatus)int.Parse(values[0]), values[1].Split('"')[1], values[2].Split('"')[1]);
                                    }
                                    break;
                                // GPRS Network Registration Status
                                case "+CGREG":
                                    break;
                                // Inditor Control
                                case "+CIEV":
                                    switch (values[0])
                                    {
                                        case "signal":
                                            if (OnSignalBitErrorRateIndicator != null)
                                                OnSignalBitErrorRateIndicator(values[1]);
                                            break;
                                        case "service":
                                            if (OnServiceAvailabiliryIndicator != null)
                                                OnServiceAvailabiliryIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "sounder":
                                            if (OnSounderActivityIndicator != null)
                                                OnSounderActivityIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "message":
                                            if (OnUnreadReceivedMessageIndicator != null)
                                                OnUnreadReceivedMessageIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "call":
                                            if (OnCallInProcessIndicator != null)
                                                OnCallInProcessIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "roam":
                                            if (OnRoamingIndicator != null)
                                                OnRoamingIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "smsfull":
                                            if (OnSMSFullIndicator != null)
                                                OnSMSFullIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "rssi":
                                            if (OnSignalStrengthIndicator != null)
                                                if (values[1] == "99")
                                                    OnSignalStrengthIndicator("0");
                                                else OnSignalStrengthIndicator(values[1]);
                                            break;
                                        case "audio":
                                            if (OnAudioActivityIndicator != null)
                                                OnAudioActivityIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "simtray":
                                            if (OnSimtrayIndicator != null)
                                                OnSimtrayIndicator(values[1] == "0" ? false : true);
                                            break;
                                        case "eons":
                                            if (OnEnhancedOperatorIndicator != null)
                                                OnEnhancedOperatorIndicator(values[2].Split('"')[1], values[3].Split('"')[1]);
                                            break;
                                        case "nitz":
                                            if (OnNetworkTimeZoneIndicator != null)
                                            {
                                                try
                                                {
                                                    if (values.Length > 3)
                                                    {
                                                        string[] date = values[1].Split('"')[1].Split('/');
                                                        string[] t = values[2].Split('"')[0].Split(':');
                                                        DateTime time = new DateTime(int.Parse("20" + date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(t[0]), int.Parse(t[1]), int.Parse(t[2]));
                                                        time = time.AddHours(double.Parse(values[3]) / 4);
                                                        OnNetworkTimeZoneIndicator(time);
                                                    }
                                                }
                                                catch { }
                                            }
                                            break;
                                    }
                                    break;
                                // USSD
                                case "+CUSD":
                                    if (OnUSSDReceived != null)
                                    {
                                        string m = string.Empty;
                                        int s = message.IndexOf('"');
                                        if (s != -1)
                                            m = message.Substring(s, message.LastIndexOf('"') - s);
                                        OnUSSDReceived((USSDStatus)int.Parse(values[0]), m);
                                    }
                                    break;
                                // SMS full
                                case "^SMGO":
                                    if (OnSMSOverflow != null)
                                        OnSMSOverflow((SMSOverflowStatus)int.Parse(values[0]));
                                    break;
                                // Extrended Call List
                                case "^SLCC":
                                    if (OnCurrentCallInfo != null)
                                    {
                                        if (values.Length > 1)
                                        {
                                            CallInfo list = new CallInfo();
                                            list.CallIndex = int.Parse(values[0]);
                                            list.Dir = (CallDir)int.Parse(values[1]);
                                            list.Status = (CallState)int.Parse(values[2]);
                                            list.CallMode = (CallMode)int.Parse(values[3]);
                                            list.IsMultipartyConferenceCall = values[4] == "0" ? false : true;
                                            if (values.Length >= 8)
                                            {
                                                list.Number = values[6].Split('"')[1];
                                                list.NumberType = (CallNumberType)int.Parse(values[7]);
                                            }
                                            if (values.Length == 9)
                                                list.EntryInPhonebook = values[8].Split('"')[1];
                                            OnCurrentCallInfo(list);
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (message)
                            {
                                case "CONNECT": //  1 link established
                                    break;
                                case "RING": //  2 ring detected
                                    if (OnIncomingCall != null)
                                    {
                                        CallInfo[] list = Call_Current_List();

                                        if (list != null)
                                        {
                                            foreach (CallInfo call in list)
                                                if (call.Status == CallState.incoming)
                                                {
                                                    OnIncomingCall(call);
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                case "NO CARRIER": //  3 link not established or disconnected
                                    break;
                                case "ERROR": //  4 invalid command or command line too long
                                    break;
                                case "NO DIALTONE": //  6 no dial tone, dialling impossible, wrong mode
                                    break;
                                case "BUSY": //  7 remote station busy
                                    break;
                                case "NO ANSWER": //  8 no answer
                                    break;
                                case "CONNECT 2400/RLP": //  47 link with 2400 bps and Radio Link Protocol
                                    break;
                                case "CONNECT 4800/RLP": //  48 link with 4800 bps and Radio Link Protocol
                                    break;
                                case "CONNECT 9600/RLP": //  49 link with 9600 bps and Radio Link Protocol
                                    break;
                                case "CONNECT 14400/RLP": //  50 link with 14400 bps and Radio Link Protocol
                                    break;
                                case "ALERTING": //  alerting at called phone
                                    break;
                                case "DIALING": //  mobile phone
                                    break;
                            }
                        }
                    }
                }
            }
        }

        #region ID Related
        public string IMEI
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CGSN");

                if (re.IsSuccess && re.PayLoad.Length == 1)
                    return re.PayLoad[0];
                else return string.Empty;
            }
        }

        public string IMSI
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CIMI");

                if (re.IsSuccess && re.PayLoad.Length == 1)
                    return re.PayLoad[0];
                else return string.Empty;
            }
        }
        #endregion

        #region Network Related
        public NetworkRegistrationStatus Network_Registration_Status
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CREG?");

                if (re.IsSuccess)
                {
                    if (re.PayLoad.Length == 1)
                    {
                        string[] values = re.PayLoad[0].Split(',');

                        if (values.Length >= 4)
                            return (NetworkRegistrationStatus)(int.Parse(values[1]));
                    }
                }

                return NetworkRegistrationStatus.unknown;
            }
        }

        public string Network_Operator_Name
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+COPS?");

                if (re.IsSuccess)
                {
                    if (re.PayLoad.Length == 1)
                    {
                        string[] values = re.PayLoad[0].Split('"');

                        if (values.Length == 3)
                            return values[1];
                    }
                }

                return string.Empty;
            }
        }

        public string Network_Service_Provider_Name
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^SIND=eons,2");

                if (re.IsSuccess)
                {
                    if (re.PayLoad.Length == 1)
                    {
                        string[] values = re.PayLoad[0].Split('"');

                        if (values.Length == 5)
                            return values[3];
                    }
                }

                return string.Empty;
            }
        }

        public bool Network_Service_Status
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^SIND=service,2"); if (re.IsSuccess)
                {
                    if (re.PayLoad.Length == 1)
                    {
                        string[] values = re.PayLoad[0].Split(',');

                        if (values.Length == 3)
                            return values[2] == "1" ? true : false;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// dBm unit, if unknown or not detectable also return -113 dBm
        /// </summary>
        public int Network_Signal_Quality
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CSQ");

                if (re.IsSuccess)
                {
                    if (re.PayLoad.Length == 1)
                    {
                        string[] values = re.PayLoad[0].Split(new char[2] { ' ', ',' });

                        if (values.Length == 3)
                        {
                            int t = int.Parse(values[1]);
                            if (t == 0 || t == 99)
                                return -113;
                            else return t * 2 - 113;
                        }
                    }
                }

                return 0;
            }
        }
        #endregion

        #region Call Related
        public void Call(string number)
        {
            SendATCommand("ATD" + number + ";");
        }

        public void Call(PhoneBook Contact)
        {
            Call(Contact.Number);
        }

        public void Call(PhoneBookStorage Storage, string LocationID)
        {
            SendATCommand("ATD>" + PhoneBook.GetPhoneBookStorageString(Storage) + LocationID + ";");
        }

        public void Call_From_Active_Momory(string LocationIDorContactName)
        {
            SendATCommand("ATD>" + LocationIDorContactName + ";");
        }

        public bool Call_Hang_Up()
        {
            return SendATCommand("ATH").IsSuccess;
        }

        public bool Call_Accpet()
        {
            return SendATCommand("ATA").IsSuccess;
        }

        /// <summary>
        /// The execute command lists all current calls. If the command is successful, but no calls are available, no information response is sent to TE.
        /// </summary>
        /// <returns></returns>
        public CallInfo[] Call_Current_List()
        {
            GeneralResponse re = SendATCommand("AT+CLCC");
            if (re.IsSuccess)
            {
                CallInfo[] list = new CallInfo[re.PayLoad.Length];
                for (int i = 0; i < re.PayLoad.Length; i++)
                {
                    string[] temp = re.PayLoad[i].Split(',');
                    list[i] = new CallInfo();

                    if (temp[0].IndexOf("+CLCC") < 0)
                        continue;

                    list[i].CallIndex = int.Parse(temp[0].Split(' ')[1]);
                    list[i].Dir = (CallDir)int.Parse(temp[1]);
                    list[i].Status = (CallState)int.Parse(temp[2]);
                    list[i].CallMode = (CallMode)int.Parse(temp[3]);
                    list[i].IsMultipartyConferenceCall = temp[4] == "0" ? false : true;
                    if (temp.Length >= 7)
                    {
                        list[i].Number = temp[5].Split('"')[1];
                        list[i].NumberType = (CallNumberType)int.Parse(temp[6]);
                    }
                    if (temp.Length == 8)
                        list[i].EntryInPhonebook = temp[7].Split('"')[1];
                }
                return list;
            }

            return null;
        }

        public string Call_Last_Duration
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^SLCD");
                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    string[] values = re.PayLoad[0].Split(' ');
                    if (values[0].IndexOf("^SLCD") >= 0)
                        return values[1];
                }

                return string.Empty;
            }
        }

        public string Call_Total_Duration
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^STCD");
                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    string[] values = re.PayLoad[0].Split(' ');
                    if (values[0].IndexOf("^STCD") >= 0)
                        return values[1];
                }

                return string.Empty;
            }
        }
        #endregion

        #region Phonebook Related

        /// <summary>
        /// The AT^SPBC write command searches the current phonebook for the index number of the first (lowest) entry 
        /// that matches the character specified with "schar". The AT^SPBC test command returns the list of phonebooks which can be searched through with AT^SPBC.
        /// CAUTION: Please note that AT^SPBC is assigned the same index as AT^SPBG or AT^SPBS which is not identical
        /// with the physical location numbers used in the various phonebooks. Therefore, do not use the index numbers retrieved with AT^SPBC to dial out or modify phonebook entries.
        /// 
        /// AT+CPBR serves to read one or more entries from the phonebook selected with AT command AT+CPBS.
        /// The AT+CPBR test command returns the location range supported by the current phonebook storage, the maximum
        /// length of "number" field and the maximum length of "text" field.
        /// Note: Length information may not be available while SIM storage is selected. If storage does not offer format information, the format list contains empty parenthesizes.
        /// The AT+CPBR write command determines the phonebook entry to be displayed with "location1" or a location range from "location1" to "location2".
        /// Hence, if no "location2" is given only the entry at "location1" will be displayed. If no entries are found at the selected location "OK" will be returned.
        /// </summary>
        /// <param name="StartLocationID"></param>
        /// <param name="EndLocationID"></param>
        /// <param name="Alphabetically"></param>
        /// <returns></returns>
        public PhoneBook[] Phonebook_List(string StartLocationID, string EndLocationID, bool Alphabetically)
        {
            GeneralResponse re = null;
            if (Alphabetically)
                re = SendATCommand("AT^SPBG=" + StartLocationID + "," + EndLocationID + ",1");
            else
                re = SendATCommand("AT+CPBR=" + StartLocationID + "," + EndLocationID);
            if (re.IsSuccess)
            {
                PhoneBook[] entries = new PhoneBook[re.PayLoad.Length];
                for (int i = 0; i < re.PayLoad.Length; i++)
                {
                    entries[i] = new PhoneBook();
                    string[] values = re.PayLoad[i].Split(',');

                    if (values[0].IndexOf("^SPBG") >=0 )
                    {
                        entries[i].Number = values[1].Split('"')[1];
                        entries[i].NumberType = (CallNumberType)int.Parse(values[2]);
                        entries[i].Name = values[3].Split('"')[1];
                        entries[i].LocationID = values[4];
                    }
                    else if (values[0].IndexOf("+CPBR") >= 0)
                    {
                        entries[i].Number = values[1].Split('"')[1];
                        entries[i].NumberType = (CallNumberType)int.Parse(values[2]);
                        entries[i].Name = values[3].Split('"')[1];
                        entries[i].LocationID = values[0].Split(' ')[1];
                    }
                }

                return entries;
            }

            return null;
        }

        public PhoneBook[] Phonebook_List(bool Alphabetically)
        {
            string EndLocationID = this.Phonebook_Current_Storage_Detail.Used.ToString();
            return Phonebook_List("1", EndLocationID, Alphabetically);
        }

        public PhoneBook[] Phonebook_List(PhoneBookStorage Storage, bool Alphabetically)
        {
            this.Phonebook_Current_Storage = Storage;
            string EndLocationID = this.Phonebook_Current_Storage_Detail.Used.ToString();
            return Phonebook_List("1", EndLocationID, Alphabetically);
        }

        public PhoneBook[] Phonebook_List(PhoneBookStorage Storage, string StartLocationID, string EndLocationID, bool Alphabetically)
        {
            this.Phonebook_Current_Storage = Storage;
            return Phonebook_List(StartLocationID, EndLocationID, Alphabetically);
        }

        public PhoneBook[] Phonebook_Missed_Calls
        {
            get
            {
                return this.Phonebook_List(PhoneBookStorage.missed, false);
            }
        }

        public PhoneBook[] Phonebook_Received_Calls
        {
            get
            {
                return this.Phonebook_List(PhoneBookStorage.received_call_list, false);
            }
        }

        public PhoneBook[] Phonebook_Last_Dialed_Calls
        {
            get
            {
                return this.Phonebook_List(PhoneBookStorage.last_number_dialed_phonebook, false);
            }
        }

        /// <summary>
        /// AT+CPBS selects the active phonebook storage, i.e. the phonebook storage that all subsequent phonebook commands will be operating on.
        /// The read command returns the currently selected "storage", the number of "used" entries and the "total" number of entries available for this storage.
        /// The test command returns all supported "storage"s as compound value.
        /// </summary>
        public PhoneBookStorage Phonebook_Current_Storage
        {
            get
            {
                PhoneBookStorageDetail detail = Phonebook_Current_Storage_Detail;
                if (detail != null)
                    return detail.Storage;

                return PhoneBookStorage.unkonow;
            }

            set
            {
                SendATCommand("AT+CPBS=" + PhoneBook.GetPhoneBookStorageString(value));
            }
        }

        /// <summary>
        /// AT+CPBS selects the active phonebook storage, i.e. the phonebook storage that all subsequent phonebook commands will be operating on.
        /// The read command returns the currently selected "storage", the number of "used" entries and the "total" number of entries available for this storage.
        /// The test command returns all supported "storage"s as compound value.
        /// </summary>
        public PhoneBookStorageDetail Phonebook_Current_Storage_Detail
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CPBS?");
                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    PhoneBookStorageDetail detail = new PhoneBookStorageDetail();
                    string[] values = re.PayLoad[0].Split(new char[] { ',', ' ' });
                    if (values.Length == 4 && values[0].IndexOf("+CPBS") >= 0)
                    {
                        detail.Storage = PhoneBook.GetPhoneBookStorageType(values[1]);
                        detail.Used = int.Parse(values[2]);
                        detail.Total = int.Parse(values[3]);
                        return detail;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The AT+CPBW write command can be used to create, edit and delete a phonebook entry at a "location" of the active storage selected with AT+CPBS.
        /// If "storage"="FD" (SIM fixed dialing numbers) is selected, PIN2 authentication has to be performed prior to any write access.
        /// The AT+CPBW test command returns the location range supported by the current storage, the maximum length
        /// of the "number" field, the range of supported "type" values and the maximum length of the "text" field.
        /// Note: The length may not be available while SIM storage is selected. If storage does not offer format information, the format list contains empty parenthesizes.
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool Phonebook_Write(string Number, string Name)
        {
            return SendATCommand("AT+CPBW=,\"" + Number + "\",,\"" + Name + "\"").IsSuccess;
        }

        /// <summary>
        /// The AT+CPBW write command can be used to create, edit and delete a phonebook entry at a "location" of the active storage selected with AT+CPBS.
        /// If "storage"="FD" (SIM fixed dialing numbers) is selected, PIN2 authentication has to be performed prior to any write access.
        /// The AT+CPBW test command returns the location range supported by the current storage, the maximum length
        /// of the "number" field, the range of supported "type" values and the maximum length of the "text" field.
        /// Note: The length may not be available while SIM storage is selected. If storage does not offer format information, the format list contains empty parenthesizes.
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="type"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool Phonebook_Write(string Number, CallNumberType type, string Name)
        {
            return SendATCommand("AT+CPBW=,\"" + Number + "\"," + (int)type + ",\"" + Name + "\"").IsSuccess;
        }

        /// <summary>
        /// The AT+CPBW write command can be used to create, edit and delete a phonebook entry at a "location" of the active storage selected with AT+CPBS.
        /// If "storage"="FD" (SIM fixed dialing numbers) is selected, PIN2 authentication has to be performed prior to any write access.
        /// The AT+CPBW test command returns the location range supported by the current storage, the maximum length
        /// of the "number" field, the range of supported "type" values and the maximum length of the "text" field.
        /// Note: The length may not be available while SIM storage is selected. If storage does not offer format information, the format list contains empty parenthesizes.
        /// </summary>
        /// <param name="LocationID"></param>
        /// <param name="Number"></param>
        /// <param name="type"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool Phonebook_Write(string LocationID, string Number, CallNumberType type, string Name)
        {
            return SendATCommand("AT+CPBW=" + LocationID + ",\"" + Number + "\"," + (int)type + ",\"" + Name + "\"").IsSuccess;
        }

        /// <summary>
        /// The AT+CPBW write command can be used to create, edit and delete a phonebook entry at a "location" of the active storage selected with AT+CPBS.
        /// If "storage"="FD" (SIM fixed dialing numbers) is selected, PIN2 authentication has to be performed prior to any write access.
        /// The AT+CPBW test command returns the location range supported by the current storage, the maximum length
        /// of the "number" field, the range of supported "type" values and the maximum length of the "text" field.
        /// Note: The length may not be available while SIM storage is selected. If storage does not offer format information, the format list contains empty parenthesizes.
        /// </summary>
        /// <param name="LocationID"></param>
        /// <returns></returns>
        public bool Phonebook_Delete(string LocationID)
        {
            return SendATCommand("AT+CPBW=" + LocationID).IsSuccess;
        }

        /// <summary>
        /// AT^SPBD can be used to purge the selected phonebook "storage" manually, i.e. all entries stored in the
        /// selected phonebook storage will be deleted. CAUTION! The operation cannot be stopped nor reversed! 
        /// The AT^SPBD test command returns the list of phonebooks which can be deleted with AT^SPBD.
        /// An automatic purge of the phonebooks is performed when the SIM card is removed and replaced with a different
        /// SIM card. This affects the ME based part of the "LD" storage, and storages "MC" and "RC". Storage "ME" is not affected.
        /// </summary>
        /// <param name="Storage"></param>
        /// <returns></returns>
        public bool Phonebook_Purge(PhoneBookStorage Storage)
        {
            return SendATCommand("AT^SPBD=" + PhoneBook.GetPhoneBookStorageString(Storage)).IsSuccess;
        }

        /// <summary>
        /// AT^SDLD deletes all numbers stored in the "LD" memory.
        /// </summary>
        /// <returns></returns>
        public bool Phonebook_Delete_Last_Number_Redial_Memory()
        {
            return SendATCommand("AT^SDLD").IsSuccess;
        }
        #endregion

        #region SMS Related Function

        /// <summary>
        /// Write command updates the SMSC address, through which mobile originated SMs are transmitted. In text mode,
        /// setting is used by send and write commands. In PDU mode, setting is used by the same commands, but only
        /// when the length of the SMSC address coded into the "pdu" parameter equals zero.
        /// </summary>
        public string SMS_Service_Center_Address
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CSCA?");
                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    string[] values = re.PayLoad[0].Split('"');
                    if (values.Length == 3)
                    {
                        if (values[0].IndexOf("+CSCA") >= 0)
                            return values[1];
                    }
                }

                return string.Empty;
            }

            set
            {
                SendATCommand("AT+CSCA=" + value);
            }
        }

        /// <summary>
        /// The short message storage "MT" (see AT+CPMS) is a logical storage. It consists of two physical storages "ME" and "SM". This command allows to select the sequence of addressing this storage.
        /// </summary>
        public SMSStorageSequence SMS_Storage_Sequence
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^SSMSS?");
                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    string[] values = re.PayLoad[0].Split(' ');
                    if (values.Length == 2 && values[1].IndexOf("^SSMSS") >= 0)
                        return (SMSStorageSequence)int.Parse(values[1]);
                }

                return SMSStorageSequence.unable_to_retrieve;
            }

            set
            {
                SendATCommand("AT^SSMSS=" + (int)value);
            }
        }

        public SMSPerferredStorage SMS_Preferred_Storage
        {
            get
            {
                SMSPerferredStorage storage = new SMSPerferredStorage();
                GeneralResponse re = SendATCommand("AT+CPMS?");
                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    string[] values = re.PayLoad[0].Split(new char[2] { ',', ' ' });
                    if (values.Length == 10 && values[0].IndexOf("+CPMS") >= 0)
                    {
                        storage.Listing_Reading_Deleting.Storage = SMSStorageDetail.GetSMSStorageType(values[1]);
                        storage.Listing_Reading_Deleting.Used = values[2];
                        storage.Listing_Reading_Deleting.Total = values[3];
                        storage.Writing_Sending.Storage = SMSStorageDetail.GetSMSStorageType(values[4]);
                        storage.Writing_Sending.Used = values[5];
                        storage.Writing_Sending.Total = values[6];
                        storage.Received.Storage = SMSStorageDetail.GetSMSStorageType(values[7]);
                        storage.Received.Used = values[8];
                        storage.Received.Total = values[9];
                        return storage;
                    }
                }

                return null;
            }

            set
            {
                SendATCommand("AT+CPMS=" + SMSStorageDetail.GetSMSStorageString(value.Listing_Reading_Deleting.Storage) + "," + SMSStorageDetail.GetSMSStorageString(value.Writing_Sending.Storage) + "," + SMSStorageDetail.GetSMSStorageString(value.Received.Storage));
            }
        }

        public SMSStorageDetail[] SMS_List_Memory_Storage
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^SLMS");
                if (re.IsSuccess && re.PayLoad.Length > 0)
                {
                    SMSStorageDetail[] details = new SMSStorageDetail[re.PayLoad.Length];
                    for (int i = 0; i < re.PayLoad.Length; i++)
                    {
                        details[i] = new SMSStorageDetail();
                        string[] values = re.PayLoad[i].Split(new char[2] { ',', ' ' });

                        if (values[0].IndexOf("^SLMS") < 0)
                            continue;

                        details[i].Storage = SMSStorageDetail.GetSMSStorageType(values[1]);
                        details[i].Total = values[2];
                        details[i].Used = values[3];
                    }

                    return details;
                }

                return null;
            }
        }

        private SMS[] SMS_Process(GeneralResponse re, string command)
        {
            if (re == null)
                return null;

            if (re.IsSuccess)
            {
                SMS[] list = new SMS[re.PayLoad.Length >> 1];

                for (int i = 0; i < re.PayLoad.Length; i += 2)
                {
                    int index = i >> 1;
                    list[index] = new SMS();
                    string[] values = re.PayLoad[i].Split(',');

                    if (values[0].IndexOf(command) < 0)
                        continue;

                    list[index].MessageID = values[0].Split(' ')[1];
                    list[index].Status = SMS.GetSMSStatusType(values[1]);
                    list[index].Sender = values[2].Split('"')[1];

                    if (values.Length >= 6)
                    {
                        list[index].Date = values[4].Split('"')[1];
                        list[index].Time = values[5].Split('"')[0];
                    }

                    list[index].Message = re.PayLoad[i + 1];
                }

                return list;
            }

            return null;
        }

        private SMS SMS_Process(GeneralResponse re, string MessageID, string command)
        {
            if (re == null)
                return null;

            if (re.IsSuccess && re.PayLoad.Length == 2)
            {
                string[] values = re.PayLoad[0].Split(',');
                if (values[0].IndexOf(command) >= 0)
                {
                    SMS message = new SMS();
                    message.Status = SMS.GetSMSStatusType(values[0].Split(' ')[1]);
                    message.Sender = values[1].Split('"')[1];

                    if (values.Length >= 5)
                    {
                        message.Date = values[3].Split('"')[1];
                        message.Time = values[4].Split('"')[0];
                    }

                    message.Message = re.PayLoad[1];

                    return message;
                }
            }

            return null;
        }

        /// <summary>
        /// The write command returns messages with status value <stat> from message storage <mem1> to the TE. If the
        /// status of the message is 'received unread', the status in the storage changes to 'received read'.
        /// The execute command is the same as the write command with the given default for <stat>.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public SMS[] SMS_List_Read(SMSMessageStatus status)
        {
            switch (status)
            {
                case SMSMessageStatus.received_read_messages: return SMS_Process(SendATCommand("AT+CMGL=" + SMS.GetSMSStatusString(status)), "+CMGL");
                case SMSMessageStatus.received_unread_messages: return SMS_Process(SendATCommand("AT+CMGL=" + SMS.GetSMSStatusString(status)), "+CMGL");
                case SMSMessageStatus.stored_sent_messages: return SMS_Process(SendATCommand("AT+CMGL=" + SMS.GetSMSStatusString(status)), "+CMGL");
                case SMSMessageStatus.stored_unsent_messages: return SMS_Process(SendATCommand("AT+CMGL=" + SMS.GetSMSStatusString(status)), "+CMGL");
                case SMSMessageStatus.all_messages: return SMS_Process(SendATCommand("AT+CMGL=" + SMS.GetSMSStatusString(status)), "+CMGL");
                default: return null;
            }
        }

        /// <summary>
        /// The write command returns messages with status value <stat> from message storage <mem1> to the TE. If the
        /// status of the message is 'received unread', the status in the storage changes to 'received read'.
        /// The execute command is the same as the write command with the given default for <stat>.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public SMS[] SMS_List_Peek(SMSMessageStatus status)
        {
            switch (status)
            {
                case SMSMessageStatus.received_read_messages: return SMS_Process(SendATCommand("AT^SMGL=" + SMS.GetSMSStatusString(status)), "^SMGL");
                case SMSMessageStatus.received_unread_messages: return SMS_Process(SendATCommand("AT^SMGL=" + SMS.GetSMSStatusString(status)), "^SMGL");
                case SMSMessageStatus.stored_sent_messages: return SMS_Process(SendATCommand("AT^SMGL=" + SMS.GetSMSStatusString(status)), "^SMGL");
                case SMSMessageStatus.stored_unsent_messages: return SMS_Process(SendATCommand("AT^SMGL=" + SMS.GetSMSStatusString(status)), "^SMGL");
                case SMSMessageStatus.all_messages: return SMS_Process(SendATCommand("AT^SMGL=" + SMS.GetSMSStatusString(status)), "^SMGL");
                default: return null;
            }
        }

        public bool SMS_Delete(string MessageID)
        {
            return SendATCommand("AT+CMGD=" + MessageID).IsSuccess;
        }

        public void SMS_Delete_All()
        {
            foreach (SMS sms in SMS_List_Peek(SMSMessageStatus.all_messages))
                SMS_Delete(sms.MessageID);
        }

        public SMS SMS_Read(string MessageID)
        {
            return SMS_Process(SendATCommand("AT+CMGR=" + MessageID), MessageID, "CMGR");
        }

        public SMS SMS_Peek(string MessageID)
        {
            return SMS_Process(SendATCommand("AT^SMGR=" + MessageID), MessageID, "SMGR");
        }

        /// <summary>
        /// AT+CMGS write command transmits a short message to network (SMS-SUBMIT).
        /// After invoking the write command wait for the prompt ">" and then start to write the message. To send the message
        /// simply enter "CTRL-Z". After the prompt a timer will be started to guard the input period.
        /// To abort sending use "ESC". Abortion is acknowledged with "OK", though the message will not be sent.
        /// The message reference "mr" is returned by the ME on successful message delivery. The value can be used to
        /// identify the message in a delivery status report provided as an unsolicited result code.
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="MessageBody"></param>
        /// <returns></returns>
        public bool SMS_Send(string Number, string MessageBody)
        {
            return SendATCommand("AT+CMGS=\"" + Number + "\"", MessageBody).IsSuccess;
        }

        public bool SMS_Send_From_Memory(string MessageID)
        {
            return SendATCommand("AT+CMSS=" + MessageID).IsSuccess;
        }

        public bool SMS_Send_From_Memory(string MessageID, string Number)
        {
            return SendATCommand("AT+CMSS=" + MessageID + "," + "\"" + Number + "\"").IsSuccess;
        }

        public int SMS_Write_To_Memory(string Number, string MessageBody)
        {
            GeneralResponse re = SendATCommand("AT+CMGW=\"" + Number + "\"", MessageBody);

            if (re.IsSuccess && re.PayLoad.Length == 1)
            {
                string[] values = re.PayLoad[0].Split(' ');
                if (values[0].IndexOf("+CMGW") >= 0)
                    return int.Parse(values[1]);
            }

            return -1;
        }

        public int SMS_Write_To_Memory(string MessageBody)
        {
            return SMS_Write_To_Memory("", MessageBody);
        }

        public SMSOverflowStatus SMS_Buffer_Overflow
        {
            get
            {
                GeneralResponse re = SendATCommand("AT^SMGO?");

                if (re.IsSuccess && re.PayLoad.Length == 1)
                {
                    string[] values = re.PayLoad[0].Split(',');

                    if (values[0].IndexOf("^SMGO") >= 0)
                        return (SMSOverflowStatus)int.Parse(values[1].Split(',')[1]);
                }

                return SMSOverflowStatus.unknown;
            }
        }
        #endregion

        #region Internet Related

        #region Setup Connection Profile

        public void Internet_Setup_Connection_Profile_GPRS(string APN, string username, string password)
        {
            Internet_Setup_Connection_Profile(0, InternetConnectionType.GPRS0, APN, username, password);
        }

        public void Internet_Setup_Connection_Profile_GPRS(int ProfileID, string APN, string username, string password)
        {
            Internet_Setup_Connection_Profile(ProfileID, InternetConnectionType.GPRS0, APN, username, password);
        }

        public void Internet_Setup_Connection_Profile(InternetConnectionType type, string APN, string username, string password)
        {
            Internet_Setup_Connection_Profile(0, type, APN, username, password);
        }

        public void Internet_Setup_Connection_Profile(int ProfileID, InternetConnectionType type, string APN, string username, string password)
        {
            InternetConnectionProfile p = new InternetConnectionProfile(ProfileID, type);
            p.user = username;
            p.passwd = password;
            p.apn = APN;
            Internet_Setup_Connection_Profile(p);
        }

        public void Internet_Setup_Connection_Profile(InternetConnectionProfile profile)
        {
            SendATCommand("AT^SICS=" + profile.profileID + ",conType," + InternetConnectionProfile.GetInternetConnectionString(profile.conType));
            SendATCommand("AT^SICS=" + profile.profileID + ",apn," + profile.apn);
            if (profile.user.Length > 0)
                SendATCommand("AT^SICS=" + profile.profileID + ",user," + profile.user);
            if (profile.passwd.Length > 0)
                SendATCommand("AT^SICS=" + profile.profileID + ",passwd," + profile.passwd);
            if (profile.inactTO.Length > 0)
                SendATCommand("AT^SICS=" + profile.profileID + ",inactTO," + profile.inactTO);
            if (profile.dns1.Length > 0)
                SendATCommand("AT^SICS=" + profile.profileID + ",dns1," + profile.dns1);
            if (profile.dns2.Length > 0)
                SendATCommand("AT^SICS=" + profile.profileID + ",dns2," + profile.dns2);
            if (profile.alphabet.Length > 0)
                SendATCommand("AT^SICS=" + profile.profileID + ",alphabet," + profile.alphabet);
        }

        public bool Internet_Clear_Connection_Profile(int ProfileID)
        {
            return SendATCommand("AT^SICS=" + ProfileID + ",conType,none").IsSuccess;
        }

        public InternetConnectionProfile Internet_Read_Connection_Profile(int ProfileID)
        {
            GeneralResponse re = SendATCommand("AT^SICS?");

            if (re.IsSuccess && re.PayLoad.Length > 0)
            {
                InternetConnectionProfile p = new InternetConnectionProfile(ProfileID, InternetConnectionType.none);
                for (int i = 0; i < re.PayLoad.Length; i++)
                {
                    string[] values = re.PayLoad[i].Split(',');
                    if (values[0].IndexOf("^SICS") < 0)
                        continue;

                    if (values[0].Split(' ')[1].Equals(ProfileID.ToString()))
                    {
                        switch (values[1])
                        {
                            case "\"alphabet\"": p.alphabet = values[2].Split('"')[1]; break;
                            case "\"apn\"": p.apn = values[2].Split('"')[1]; break;
                            case "\"calledNum\"": p.calledNum = values[2].Split('"')[1]; break;
                            case "\"conType\"": p.conType = InternetConnectionProfile.GetInternetConnectionType(values[2]); break;
                            case "\"dataType\"": p.dataType = values[2].Split('"')[1]; break;
                            case "\"dns1\"": p.dns1 = values[2].Split('"')[1]; break;
                            case "\"dns2\"": p.dns2 = values[2].Split('"')[1]; break;
                            case "\"inactTO\"": p.inactTO = values[2].Split('"')[1]; break;
                            case "\"passwd\"": p.passwd = values[2].Split('"')[1]; break;
                            case "\"user\"": p.user = values[2].Split('"')[1]; break;
                        }
                    }
                }

                return p;
            }

            return null;
        }

        #endregion

        #region Setup Service Profile

        public void Internet_Setup_Service_Profile(InternetServiceProfile profile)
        {
            SendATCommand("AT^SISS=" + profile.profileID + ",srvType," + InternetServiceProfile.GetInternetServiceString(profile.srvType));
            SendATCommand("AT^SISS=" + profile.profileID + ",hcMethod," + (int)profile.hcMethod);
            SendATCommand("AT^SISS=" + profile.profileID + ",conID," + profile.conId);
            SendATCommand("AT^SISS=" + profile.profileID + ",address," + profile.address);
            if (profile.alphabet != Alphabet.unknown)
                SendATCommand("AT^SISS=" + profile.profileID + ",alphabet," + (int)profile.alphabet);
            if (profile.hcAuth.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",hcAuth," + profile.hcAuth);
            if (profile.hcContent.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",hcContent," + profile.hcContent);
            if (profile.hcContLen.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",hcContLen," + profile.hcContLen);
            if (profile.hcProp.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",hcProp," + profile.hcProp);
            if (profile.hcRedir.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",hcRedir," + profile.hcRedir);
            if (profile.hcUsrAgent.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",hcUsrAgent," + profile.hcUsrAgent);
            if (profile.passwd.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",passwd," + profile.passwd);
            if (profile.pCmd != POPCommand.unknown)
                SendATCommand("AT^SISS=" + profile.profileID + ",pCmd," + (int)profile.pCmd);
            if (profile.pDelFlag != POPDeleteFlage.unknown)
                SendATCommand("AT^SISS=" + profile.profileID + ",pDelFlag," + (int)profile.pDelFlag);
            if (profile.pLength.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",pLength," + profile.pLength);
            if (profile.pNumber.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",pNumber," + profile.pNumber);
            if (profile.smAuth.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",smAuth," + profile.smAuth);
            if (profile.smCC.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",smCC," + profile.smCC);
            if (profile.smFrom.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",smFrom," + profile.smFrom);
            if (profile.smHdr.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",smHdr," + profile.smHdr);
            if (profile.smRcpt.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",smRcpt," + profile.smRcpt);
            if (profile.smSubj.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",smSubj," + profile.smSubj);
            if (profile.tcpMR.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",tcpMR," + profile.tcpMR);
            if (profile.tcpOT.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",tcpOT," + profile.tcpOT);
            if (profile.tcpPort.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",tcpPort," + profile.tcpPort);
            if (profile.user.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",user," + profile.user);
            if (profile.secOpt.Length > 0)
                SendATCommand("AT^SISS=" + profile.profileID + ",secOpt," + profile.secOpt);
        }

        public bool Internet_Clear_Service_Profile(int ProfileID)
        {
            return SendATCommand("AT^SISS=" + ProfileID + ",srvType,none").IsSuccess;
        }

        public InternetServiceProfile Internet_Read_Service_Profile(int ProfileID)
        {
            GeneralResponse re = SendATCommand("AT^SISS?");

            if (re.IsSuccess && re.PayLoad.Length > 0)
            {
                InternetServiceProfile p = new InternetServiceProfile(ProfileID, InternetServiceType.none, -1);
                for (int i = 0; i < re.PayLoad.Length; i++)
                {
                    string[] values = re.PayLoad[i].Split(',');
                    if (values[0].IndexOf("^SISS") < 0)
                        continue;

                    if (values[0].Split(' ')[1].Equals(ProfileID.ToString()))
                    {
                        switch (values[1])
                        {
                            case "\"address\"": p.address = values[2].Split('"')[1]; break;
                            case "\"alphabet\"": p.alphabet = (Alphabet)int.Parse(values[2].Split('"')[1]); break;
                            case "\"conId\"": p.conId = int.Parse(values[2].Split('"')[1]); break;
                            case "\"hcAuth\"": p.hcAuth = values[2].Split('"')[1]; break;
                            case "\"hcContent\"": p.hcContent = values[2].Split('"')[1]; break;
                            case "\"hcContLen\"": p.hcContLen = values[2].Split('"')[1]; break;
                            case "\"hcMethod\"": p.hcMethod = (InternetServiceMethod)int.Parse(values[2].Split('"')[1]); break;
                            case "\"hcProp\"": p.hcProp = values[2].Split('"')[1]; break;
                            case "\"hcRedir\"": p.hcRedir = values[2].Split('"')[1]; break;
                            case "\"hcUsrAgent\"": p.hcUsrAgent = values[2].Split('"')[1]; break;
                            case "\"passwd\"": p.passwd = values[2].Split('"')[1]; break;
                            case "\"pCmd\"": p.pCmd = (POPCommand)int.Parse(values[2].Split('"')[1]); break;
                            case "\"pDelFlag\"": p.pDelFlag = (POPDeleteFlage)int.Parse(values[2].Split('"')[1]); break;
                            case "\"pLength\"": p.pLength = values[2].Split('"')[1]; break;
                            case "\"pNumber\"": p.pNumber = values[2].Split('"')[1]; break;
                            case "\"smAuth\"": p.smAuth = values[2].Split('"')[1]; break;
                            case "\"smCC\"": p.smCC = values[2].Split('"')[1]; break;
                            case "\"smFrom\"": p.smFrom = values[2].Split('"')[1]; break;
                            case "\"smHdr\"": p.smHdr = values[2].Split('"')[1]; break;
                            case "\"smRcpt\"": p.smRcpt = values[2].Split('"')[1]; break;
                            case "\"smSubj\"": p.smSubj = values[2].Split('"')[1]; break;
                            case "\"srvType\"": p.srvType = InternetServiceProfile.GetInternetServiceType(values[2].Split('"')[1]); break;
                            case "\"tcpMR\"": p.tcpMR = values[2].Split('"')[1]; break;
                            case "\"tcpOT\"": p.tcpOT = values[2].Split('"')[1]; break;
                            case "\"tcpPort\"": p.tcpPort = values[2].Split('"')[1]; break;
                            case "\"user\"": p.user = values[2].Split('"')[1]; break;
                            case "\"secOpt\"": p.secOpt = values[2].Split('"')[1]; break;
                        }
                    }
                }

                return p;
            }

            return null;
        }

        #endregion

        #region Service Status

        public InternetConnectionInfo[] Internet_Connection_Information()
        {
            GeneralResponse re = SendATCommand("AT^SICI?");
            if (re.IsSuccess)
            {
                InternetConnectionInfo[] info = new InternetConnectionInfo[re.PayLoad.Length];
                for (int i = 0; i < re.PayLoad.Length; i++)
                {
                    string[] values = re.PayLoad[i].Split(',');
                    info[i] = new InternetConnectionInfo();

                    if (values[0].IndexOf("^SICI") < 0)
                        continue;

                    info[i].ProfileID = int.Parse(values[0].Split(' ')[1]);
                    info[i].Status = (InternetConnectionStatus)int.Parse(values[1]);
                    info[i].NumberOfServices = int.Parse(values[2]);
                    info[i].IPAddress = values[3].Split('"')[1];
                }
                return info;
            }

            return null;
        }

        public InternetConnectionInfo Internet_Connection_Information(int ProfileID)
        {
            GeneralResponse re = SendATCommand("AT^SICI=" + ProfileID);
            if (re.IsSuccess)
            {
                string[] values = re.PayLoad[0].Split(',');

                if (values[0].IndexOf("^SICI") >= 0)
                {
                    InternetConnectionInfo info = new InternetConnectionInfo();
                    info.ProfileID = ProfileID;
                    info.Status = (InternetConnectionStatus)int.Parse(values[1]);
                    info.NumberOfServices = int.Parse(values[2]);
                    info.IPAddress = values[3].Split('"')[1];
                    return info;
                }
            }

            return null;
        }

        public InternetServiceInfo[] Internet_Service_Information()
        {
            GeneralResponse re = SendATCommand("AT^SISI?");
            if (re.IsSuccess)
            {
                InternetServiceInfo[] info = new InternetServiceInfo[re.PayLoad.Length];
                for (int i = 0; i < re.PayLoad.Length; i++)
                {
                    string[] values = re.PayLoad[i].Split(',');
                    info[i] = new InternetServiceInfo();

                    if (values[0].IndexOf("^SISI") >= 0)
                    {
                        info[i].ProfileID = int.Parse(values[0].Split(' ')[1]);
                        info[i].Status = (InternetServiceStatus)int.Parse(values[1]);
                        info[i].RX_Count = int.Parse(values[2]);
                        info[i].TX_Count = int.Parse(values[3]);
                        info[i].Acknowledged_Data = int.Parse(values[4]);
                        info[i].Not_Acknowledged_Data = int.Parse(values[5]);
                    }
                }
                return info;
            }

            return null;
        }

        public InternetServiceInfo Internet_Service_Information(InternetServiceProfile Profile)
        {
            return Internet_Service_Information(Profile.profileID);
        }

        public InternetServiceInfo Internet_Service_Information(int ProfileID)
        {
            GeneralResponse re = SendATCommand("AT^SISI=" + ProfileID);
            if (re.IsSuccess)
            {
                string[] values = re.PayLoad[0].Split(',');
                if (values[0].IndexOf("^SISI") >= 0)
                {
                    InternetServiceInfo info = new InternetServiceInfo();
                    info.ProfileID = ProfileID;
                    info.Status = (InternetServiceStatus)int.Parse(values[1]);
                    info.RX_Count = int.Parse(values[2]);
                    info.TX_Count = int.Parse(values[3]);
                    info.Acknowledged_Data = int.Parse(values[4]);
                    return info;
                }
            }

            return null;
        }

        public InternetError Internet_Service_Error(InternetServiceProfile Profile)
        {
            return Internet_Service_Error(Profile.profileID);
        }

        public InternetError Internet_Service_Error(int ProfileID)
        {
            GeneralResponse re = SendATCommand("AT^SISE=" + ProfileID);
            if (re.IsSuccess)
            {
                string[] values = re.PayLoad[0].Split(',');
                if (values[0].IndexOf("^SISE") >= 0)
                {
                    InternetError error = new InternetError();
                    error.ProfileID = ProfileID;
                    error.ID = int.Parse(values[1]);
                    if (values.Length == 3)
                        error.InfoText = values[2];
                    return error;
                }
            }

            return null;
        }

        #endregion

        #region Service Open & Close

        public bool Internet_Open_Service(int serviceProfileId)
        {
            return SendATCommand("AT^SISO=" + serviceProfileId).IsSuccess;
        }

        public bool Internet_Open_Service(InternetServiceProfile Profile)
        {
            return Internet_Open_Service(Profile.profileID);
        }

        public bool Internet_Close_Service(int serviceProfileId)
        {
            return SendATCommand("AT^SISC=" + serviceProfileId).IsSuccess;
        }

        public void Internet_Close_Service(InternetServiceProfile Profile)
        {
            Internet_Close_Service(Profile.profileID);
        }

        #endregion

        #region Service Read & Write

        public int Internet_Service_Peek_Date(InternetServiceProfile Profile)
        {
            GeneralResponse re = SendATCommand("AT^SISR=" + Profile.profileID + ",0");
            if (re.IsSuccess && re.PayLoad.Length == 1)
                return int.Parse(re.PayLoad[0].Split(',')[1]);

            return -1;
        }

        public InternetReadResponse Internet_Service_Read_Date(InternetServiceProfile Profile)
        {
            return Internet_Service_Read_Date(Profile.profileID);
        }

        public InternetReadResponse Internet_Service_Read_Date(int ProfileID)
        {
            string sb = string.Empty;
            InternetReadResponse dataResponse = new InternetReadResponse();

            GeneralResponse re = SendATCommand("AT^SISR=" + ProfileID + ",1500");
            if (re.IsSuccess & re.PayLoad.Length > 0)
            {
                string[] values = re.PayLoad[0].Split(',');
                if (values[0].IndexOf("^SISR") >= 0)
                {
                    int length = int.Parse(values[1]);
                    if (length > 0 && re.PayLoad.Length > 1)
                    {
                        dataResponse.Status = InternetReadStatus.data_is_available;
                        for (int i = 1; i < re.PayLoad.Length; i++)
                        {
                            sb += re.PayLoad[i];
                            if (i < re.PayLoad.Length - 1)
                                sb += "\r\n";
                        }
                        dataResponse.Data = sb.ToString();
                    }
                    else
                        dataResponse.Status = (InternetReadStatus)length;
                }
            }
            else dataResponse.Status = InternetReadStatus.data_transfer_has_been_finished;

            return dataResponse;
        }
        
        public int Internet_Service_Write_Date(int ProfileID, string Data)
        {
            GeneralResponse re = SendATCommand("AT^SISW=" + ProfileID + "," + Data.Length, null, Data);
            if (re.IsSuccess && re.PayLoad.Length > 0)
            {
                string[] values = re.PayLoad[0].Split(',');
                if (values[0].IndexOf("^SISW") >= 0)
                    return int.Parse(values[1]);
            }

            return 0;
        }

        public int Internet_Service_Write_Date(InternetServiceProfile Profile, string Data)
        {
            return Internet_Service_Write_Date(Profile.profileID, Data);
        }

        #endregion

        private InternetRequestResponse Internet_Data_Request(int ServiceID, int ConnectionID, string data = null, bool isSMTP = false)
        {
            InternetRequestResponse response = new InternetRequestResponse();
            bool needBreak = false;

            if (Internet_Open_Service(ServiceID))
            {
                /*
                // service no error, check if the conntion is up, then start receive data
                InternetConnectionInfo info = Internet_Connection_Information(ConnectionID);
                if (info == null || info.Status != InternetConnectionStatus.up_internet_connection_is_established_and_usable)
                {
                    response.Info = Internet_Service_Information(ServiceID);
                    Internet_Close_Service(ServiceID);
                    return response;
                }
                */
                bool isOK = false;
                needBreak = false;

                do
                {
                    response.Info = Internet_Service_Information(ServiceID);
                    response.Error = Internet_Service_Error(ServiceID);

                    switch (response.Info.Status)
                    {
                        case InternetServiceStatus.allocated:
                            break;
                        case InternetServiceStatus.closing:
                        case InternetServiceStatus.down:
                        case InternetServiceStatus.unkonwn:
                            isOK = false;
                            needBreak = true;
                            break;
                        case InternetServiceStatus.up:
                            isOK = true;
                            needBreak = true;
                            break;
                        case InternetServiceStatus.connecting:
                            // smtp only
                            if (isSMTP)
                            {
                                isOK = true;
                                needBreak = true;
                            }
                            break;
                    }

                    if (needBreak)
                        break;

                    if (response.Error.ID != 0)
                        break;
                }
                while (true);

                if (!isOK)
                {
                    Internet_Close_Service(ServiceID);
                    return response;
                }

                // write
                if (data != null)
                {
                    isOK = true;
                    int bytesToWrite = data.Length;
                    int startIndex = 0;
                    needBreak = false;
                    while (true)
                    {
                        // max write size is 1500 and get the actual size that server accpet
                        int size = Internet_Service_Write_Date(ServiceID, data.Substring(startIndex, data.Length - startIndex > 1500 ? 1500 : data.Length - startIndex));

                        // calculate the new start and remain size
                        bytesToWrite -= size;
                        startIndex += size;

                        response.Error = Internet_Service_Error(ServiceID);
                        response.Info = Internet_Service_Information(ServiceID);

                        if (bytesToWrite <= 0)
                            break;

                        if (response.Error.ID != 0)
                        {
                            isOK = false;
                            break;
                        }

                        switch (response.Info.Status)
                        {
                            case InternetServiceStatus.allocated:
                            case InternetServiceStatus.closing:
                            case InternetServiceStatus.down:
                            case InternetServiceStatus.unkonwn:
                                isOK = false;
                                needBreak = true;
                                break;
                            case InternetServiceStatus.up:
                                // no action is reatuired
                                break;
                            case InternetServiceStatus.connecting:
                                // smtp is ok, but other is not ok
                                if (!isSMTP)
                                {
                                    isOK = false;
                                    needBreak = true;
                                }
                                break;
                        }

                        if (needBreak)
                            break;
                    }

                    if (!isOK)
                    {
                        Internet_Close_Service(ServiceID);
                        return response;
                    }

                    if (isSMTP)
                        SendATCommand("AT^SISW=" + ServiceID + ",0,1");
                }

                // read
                int numberDown = 0;
                string sb = string.Empty;
                needBreak = false;
                int loopCount = 0;
                while (true)
                {
                    response.Error = Internet_Service_Error(ServiceID);
                    response.Info = Internet_Service_Information(ServiceID);

                    if (response.Error.ID != 0)
                        break;

                    switch (response.Info.Status)
                    {
                        case InternetServiceStatus.down:
                            if (isSMTP)
                                numberDown = 4;
                            else
                                numberDown++;
                            break;
                        case InternetServiceStatus.closing:
                        case InternetServiceStatus.allocated:
                        case InternetServiceStatus.unkonwn:
                        case InternetServiceStatus.connecting:
                            break;
                        case InternetServiceStatus.up:
                            loopCount++;
                            break;
                    }

                    if (numberDown > 3)
                        break;

                    if (isSMTP)
                        continue;
                    
                    InternetReadResponse inData = Internet_Service_Read_Date(ServiceID);
                    if (inData.Status == InternetReadStatus.data_is_available)
                        sb += inData.Data;
                    else if (inData.Status == InternetReadStatus.data_transfer_has_been_finished)
                        break;
                    
                    if (loopCount > 30)
                        break;
                }

                response.Body = sb.ToString();

                Internet_Close_Service(ServiceID);
                return response;
            }

            Internet_Close_Service(ServiceID);
            return null;
        }

        #region Http Get Request

        public InternetRequestResponse Internet_HttpRequest_GET(int ServiceID, int ConnectionID)
        {
            return Internet_Data_Request(ServiceID, ConnectionID, null);
        }

        public InternetRequestResponse Internet_HttpRequest_GET(InternetServiceProfile Profile)
        {
            return Internet_HttpRequest_GET(Profile.profileID, Profile.conId);
        }

        /// <summary>
        /// this will over write server profile 0
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_HttpRequest_GET(string URL)
        {
            InternetServiceProfile service = new InternetServiceProfile(0, InternetServiceType.Http, 0);
            service.address = URL;
            service.hcContLen = "0";
            service.hcMethod = InternetServiceMethod.GET;
            Internet_Setup_Service_Profile(service);
            return Internet_HttpRequest_GET(service);
        }
        #endregion

        #region Http Post Request

        public InternetRequestResponse Internet_HttpRequest_POST(int ServiceID, int ConnectionID, string Data)
        {
            return Internet_Data_Request(ServiceID, ConnectionID, Data);
        }

        public InternetRequestResponse Internet_HttpRequest_POST(InternetServiceProfile Profile, string Data)
        {
            return Internet_HttpRequest_POST(Profile.profileID, Profile.conId, Data);
        }

        /// <summary>
        /// this will over write server profile 1
        /// </summary>
        /// <param name="RequestURL"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_HttpRequest_POST(string RequestURL, string Data)
        {
            return Internet_HttpRequest_POST(RequestURL, null, Data);
        }

        /// <summary>
        /// this will over write server profile 1
        /// </summary>
        /// <param name="RequestURL"></param>
        /// <param name="Headers"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_HttpRequest_POST(string RequestURL, HttpHeaders Headers, string Data)
        {
            InternetServiceProfile service = new InternetServiceProfile(1, InternetServiceType.Http, 0);
            service.hcMethod = InternetServiceMethod.POST;
            service.address = RequestURL;
            service.hcContLen = Data.Length.ToString();
            if (Headers != null)
                service.hcProp = Headers.ToString();
            Internet_Setup_Service_Profile(service);
            return Internet_HttpRequest_POST(service.profileID, service.conId, Data);
        }

        /// <summary>
        /// this will over write server profile 0
        /// </summary>
        /// <param name="RequestURL"></param>
        /// <param name="Host"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_HttpRequest_SOAP(string RequestURL, string XML)
        {
            InternetServiceProfile service = new InternetServiceProfile(1, InternetServiceType.Http, 0);
            service.hcMethod = InternetServiceMethod.POST;
            service.address = RequestURL;
            service.hcContLen = XML.Length.ToString();
            //service.hcProp = "Host:\\20" + Host + "\\0d\\0aContent-Type:\\20application/soap+xml\\3b\\20charset=utf-8\\0d\\0aContent-Length:\\20" + XML.Length.ToString();
            service.hcProp = "Content-Type:\\20application/soap+xml\\3b\\20charset=utf-8";
            Internet_Setup_Service_Profile(service);
            return Internet_HttpRequest_POST(service.profileID, service.conId, XML);
        }
        #endregion

        #region SMTP

        public InternetRequestResponse Internet_SMTP(int ServiceID, int ConnectionID, string Message)
        {
            return Internet_Data_Request(ServiceID, ConnectionID, Message, true);
            /*
            InternetRequestResponse response = new InternetRequestResponse();

            if (Internet_Open_Service(ServiceID))
            {
                // service no error, check if the conntion is up, then start receive data
                InternetConnectionInfo info = Internet_Connection_Information(ConnectionID);
                if (info == null || info.Status != InternetConnectionStatus.up_internet_connection_is_established_and_usable)
                {
                    response.Info = Internet_Service_Information(ServiceID);
                    Internet_Close_Service(ServiceID);
                    return response;
                }

                bool isOK = false;

                do
                {
                    response.Info = Internet_Service_Information(ServiceID);
                    response.Error = Internet_Service_Error(ServiceID);

                    if (response.Info.Status == InternetServiceStatus.connecting)
                    {
                        isOK = true;
                        break;
                    }
                    else if (response.Info.Status == InternetServiceStatus.down)
                        break;

                    if (response.Error.InfoID != 0)
                        break;
                }
                while (true);

                if (!isOK)
                    return response;

                // write
                if (data != null)
                {
                    isOK = true;
                    int bytesToWrite = Message.Length;
                    int startIndex = 0;
                    while (true)
                    {
                        // max write size is 1500 and get the actual size that server accpet
                        int size = Internet_Service_Write_Date(ServiceID, Message.Substring(startIndex, Message.Length - startIndex > 1500 ? 1500 : Message.Length - startIndex));

                        // calculate the new start and remain size
                        bytesToWrite -= size;
                        startIndex += size;

                        if (bytesToWrite <= 0)
                            break;

                        response.Error = Internet_Service_Error(ServiceID);
                        response.Info = Internet_Service_Information(ServiceID);

                        if (!(response.Info.Status == InternetServiceStatus.up || response.Info.Status == InternetServiceStatus.connecting))
                        {
                            isOK = false;
                            break;
                        }

                        if (response.Error.InfoID != 0)
                        {
                            isOK = false;
                            break;
                        }
                    }

                    SendATCommand("AT^SISW=" + ServiceID + ",0,1");

                    if (!isOK)
                        return response;
                }

                // wait for status to change to down
                while (true)
                {
                    response.Error = Internet_Service_Error(ServiceID);
                    response.Info = Internet_Service_Information(ServiceID);

                    if (response.Info.Status == InternetServiceStatus.down)
                        break;

                    if (response.Error.InfoID != 0)
                        break;
                }

                Internet_Close_Service(ServiceID);
                return response;
            }

            Internet_Close_Service(ServiceID);
            return null;
            */
        }

        public InternetRequestResponse Internet_SMTP(InternetServiceProfile Profile, string Message)
        {
            return Internet_SMTP(Profile.profileID, Profile.conId, Message);
        }

        /// <summary>
        /// this will overwrite server profile 2
        /// </summary>
        /// <param name="SMTPserver"></param>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Subject"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_SMTP(string SMTPserver, string From, string To, string Username, string Password, string Subject, string Message)
        {
            InternetServiceProfile profile = new InternetServiceProfile(2, InternetServiceType.Smtp, 0);
            profile.address = SMTPserver;
            profile.user = Username;
            profile.passwd = Password;
            profile.smFrom = From;
            profile.smRcpt = To;
            profile.smSubj = Subject;
            profile.smAuth = "1";
            profile.alphabet = Alphabet.international_reference_alphabet;
            profile.tcpPort = "25";
            Internet_Setup_Service_Profile(profile);
            return Internet_SMTP(profile, Message);
        }

        #endregion

        #region POP

        public InternetRequestResponse Internet_POP(int ServiceID, int ConnectionID)
        {
            return Internet_Data_Request(ServiceID, ConnectionID, null);
        }

        public InternetRequestResponse Internet_POP(InternetServiceProfile Profile)
        {
            return Internet_POP(Profile.profileID, Profile.conId);
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP(string POPserver, string Username, string Password, POPCommand Command, string MessageID, POPDeleteFlage Flage)
        {
            InternetServiceProfile profile = new InternetServiceProfile(3, InternetServiceType.Pop3, 0);
            profile.address = POPserver;
            profile.user = Username;
            profile.passwd = Password;
            profile.pCmd = Command;
            profile.pNumber = MessageID;
            profile.pDelFlag = Flage;
            profile.smAuth = "1";
            profile.alphabet = Alphabet.international_reference_alphabet;
            profile.tcpPort = "110";
            Internet_Setup_Service_Profile(profile);
            return Internet_POP(profile);
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP_List(string POPserver, string Username, string Password, string MessageID)
        {
            return Internet_POP(POPserver, Username, Password, POPCommand.list_command, MessageID, POPDeleteFlage.unknown);
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP_List_All(string POPserver, string Username, string Password)
        {
            return Internet_POP_List(POPserver, Username, Password, "0");
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP_Delete(string POPserver, string Username, string Password, string MessageID)
        {
            return Internet_POP(POPserver, Username, Password, POPCommand.delete_command, MessageID, POPDeleteFlage.unknown);
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP_Delete_All(string POPserver, string Username, string Password)
        {
            return Internet_POP_Delete(POPserver, Username, Password, "0");
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP_Retrieve(string POPserver, string Username, string Password, string MessageID)
        {
            return Internet_POP(POPserver, Username, Password, POPCommand.retrieve_commad, MessageID, POPDeleteFlage.unknown);
        }

        /// <summary>
        /// this will over write server profile 3
        /// </summary>
        /// <param name="POPserver"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Command"></param>
        /// <param name="MessageID"></param>
        /// <param name="Flage"></param>
        /// <returns></returns>
        public InternetRequestResponse Internet_POP_Retrieve_All(string POPserver, string Username, string Password)
        {
            return Internet_POP_Retrieve(POPserver, Username, Password, "0");
        }

        #endregion

        #endregion

        #region GPRS Command
        public bool GPRS_Attach_or_Detach
        {
            get
            {
                GeneralResponse re = SendATCommand("AT+CGATT?");

                if (re.IsSuccess && re.PayLoad.Length > 0)
                {
                    string[] values = re.PayLoad[0].Split(' ');

                    if (values.Length == 2)
                        return values[1] == "1" ? true : false;
                }

                return false;
            }
            set
            {
                SendATCommand("AT+CGATT=" + (value ? "1" : "0"));
            }
        }
        #endregion
    }
}