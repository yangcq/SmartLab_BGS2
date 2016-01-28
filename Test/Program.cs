using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartLab.BGS2;
using SmartLab.BGS2.Core;
using SmartLab.BGS2.Status;
using SmartLab.BGS2.Type;

namespace Test
{
    class Program
    {
        static CoreBGS2 m;
        static void Main(string[] args)
        {
            m = new CoreBGS2("COM90");

            m.OnNewSMSReceived += M_OnNewSMSReceived;
            m.OnNetworkRegistrationChanged += M_OnNetworkRegistrationChanged;
            m.OnUSSDReceived += m_OnUSSDReceived;
            m.OnCurrentCallInfo += m_OnCurrentCallList;
            m.OnNetworkTimeZoneIndicator += m_OnNetworkTimeZoneIndicator;
            m.OnServiceAvailabiliryIndicator += m_OnServiceAvailabiliryIndicator;
            m.OnIncomingCall += m_OnIncomingCall;

            m.Start();

            /*
            InternetRequestResponse httpGetRe = m.Internet_HttpRequest_GET("http://sash.herts.ac.uk");
            if (httpGetRe != null)
            {
                System.Diagnostics.Debug.WriteLine(httpGetRe.Body);
            }
             * */

            //while (true)
                //m.SendATCommand("ATI");

            //InternetRequestResponse popRe = m.Internet_POP_List_All("pop.163.com", "yangcq88517", "cq880517");

            //InternetRequestResponse popRe1 = m.Internet_POP_Retrieve("pop.163.com", "yangcq88517", "cq880517", "7");

            /*
            byte[] _dat = new byte[4123];
            for (int i = 0; i < _dat.Length; i++)
                _dat[i] = 0x60;
            string longstring = UTF8Encoding.UTF8.GetString(_dat);

            InternetRequestResponse smtpRe = m.Internet_SMTP("smtp.163.com", "yangcq88517@163.com", "c.yang21@herts.ac.uk", "yangcq88517", "cq880517", "BGS2-W", "Hello and welocome");
            InternetRequestResponse smtpRe2 = m.Internet_SMTP("smtp.163.com", "yangcq88517@163.com", "c.yang21@herts.ac.uk", "yangcq88517", "cq880517", "BGS2-W", longstring);
            InternetRequestResponse smtpRe1 = m.Internet_SMTP("smtp.163.com", "aaaaaaaa@163.com", "c.yang21@herts.ac.uk", "yangcq88517", "cq880517", "BGS2-W", "Hello and welocome");
            */

            /*
            SMS[] smslist = m.SMS_List_Peek(SMSMessageStatus.all_messages);

            SMS asdf = m.SMS_Peek("22");

            InternetConnectionProfile asdfdhhsa = m.Internet_Read_Connection_Profile(0);

            InternetServiceProfile kljdf = m.Internet_Read_Service_Profile(0);
            */

            /*
            byte[] _dat = new byte[4123];
            for (int i = 0; i < _dat.Length; i++)
                _dat[i] = 0x60;
            string longstring = UTF8Encoding.UTF8.GetString(_dat);
            
            InternetRequestResponse httpPOSTRe = m.Internet_HttpRequest_POST("http://posttestserver.com/post.php", longstring);
            if (httpPOSTRe != null)
            {
                System.Diagnostics.Debug.WriteLine(httpPOSTRe.Body);
            }
            */

            System.Diagnostics.Debug.WriteLine(m.Call_Last_Duration);
            System.Diagnostics.Debug.WriteLine(m.Call_Total_Duration);
            /*
            //InternetRequestResponse httpPOSTRe = m.Internet_HttpRequest_POST("http://posttestserver.com/post.php", "Host:\\20posttestserver.com", "CQ");
           

            //System.Diagnostics.Debug.WriteLine("SMS send : " + m.SMS_Send("07440455603", "Hi From BGS2 API"));
            */
            /*
            System.Diagnostics.Debug.WriteLine(m.Internet_Connection_Information(0).Status);
            System.Diagnostics.Debug.WriteLine(m.Internet_Open_Service(1));
            System.Diagnostics.Debug.WriteLine(m.Internet_Connection_Information(0).Status);
            System.Diagnostics.Debug.WriteLine(m.Internet_Service_Information(1).Status);
            System.Diagnostics.Debug.WriteLine(m.Internet_Service_Information(1).Status);
            System.Diagnostics.Debug.WriteLine(m.Internet_Service_Information(1).Status);
            System.Diagnostics.Debug.WriteLine(m.Internet_Service_Information(1).Status);
            System.Diagnostics.Debug.WriteLine("writer : " + m.Internet_Service_Write_Date(1, "111111111111111111111111"));
            System.Diagnostics.Debug.WriteLine(m.Internet_Close_Service(1));
            */

            /*
            while (true) {
               CallList[] l =  m.Call_Current_List();

               foreach (CallList lll in l) 
               {
                   System.Diagnostics.Debug.WriteLine(lll.EntryInPhonebook);
               }
            }

            PhoneBook[] list =  m.Phonebook_List(PhoneBookStorage.mobile_equipment_phonebook, true);
             * */

            /*
            string gafg = "";

            GeneralResponse rrr = m.SendATCommand("AT^SISO=0");
            if (rrr != null)
            {
                if (rrr.IsSuccess)
                {
                    while (true)
                    {
                        rrr = m.SendATCommand("AT^SISR=0,1500");
                        if (rrr == null)
                            break;

                        if (rrr.IsSuccess)
                        {
                            if (rrr.PayLoad.Length >= 1)
                            {
                                string[] fsdaf = rrr.PayLoad[0].Split(',');
                                if (fsdaf.Length >= 2)
                                {
                                    int fads = int.Parse(fsdaf[1]);

                                    if (fads == -2)
                                        break;

                                    if (fads > 0)
                                    { gafg += rrr.PayLoad[1]; }
                                        
                                }
                            }
                        }
                    }
                    rrr = m.SendATCommand("AT^SISC=0");
                }
            }*/
            
            System.Diagnostics.Debug.WriteLine("IMEI:" + m.IMEI);
            System.Diagnostics.Debug.WriteLine("IMSI:" + m.IMSI);
            System.Diagnostics.Debug.WriteLine("Network_Operator_Name:" + m.Network_Operator_Name);
            System.Diagnostics.Debug.WriteLine("Network_Registration_Status:" + m.Network_Registration_Status);
            System.Diagnostics.Debug.WriteLine("Network_Service_Provider_Name:" + m.Network_Service_Provider_Name);
            System.Diagnostics.Debug.WriteLine("Network_Signal_Quality:" + m.Network_Signal_Quality);

            m.SendATCommand("ATI");

            GeneralResponse re = m.SendATCommand("AT^SMGL=ALL");
            if (re != null)
                for (int i = 0; i < re.PayLoad.Length; i += 2)
                    System.Diagnostics.Debug.WriteLine(re.PayLoad[i + 1]);
            //m.SendATCommand("ATD343243245");

            string indata = Console.ReadLine();

            while (indata != "exit")
            {
                re = m.SendATCommand(indata);

                if (re != null)
                {
                    if (re.IsSuccess)
                    //if (re.Result == CommandResult.command_executed_no_errors)
                    {
                        for (int i = 0; i < re.PayLoad.Length; i++)
                            Console.WriteLine(re.PayLoad[i]);
                    }
                    else Console.WriteLine("ERROR");
                }

                indata = Console.ReadLine();
            }
        }

        static void m_OnIncomingCall(CallInfo call)
        {
            Console.WriteLine("In Coming Call");
            Console.WriteLine(call.CallIndex);
            Console.WriteLine(call.CallMode);
            Console.WriteLine(call.Dir);
            Console.WriteLine(call.EntryInPhonebook);
            Console.WriteLine(call.IsMultipartyConferenceCall);
            Console.WriteLine(call.Number);
            Console.WriteLine(call.NumberType);
            Console.WriteLine(call.Status);
        }

        static void m_OnServiceAvailabiliryIndicator(bool status)
        {
            Console.WriteLine("Service : " + status);
        }

        static void m_OnNetworkTimeZoneIndicator(DateTime time)
        {
            Console.WriteLine("Time : " + time);
        }

        static void m_OnCurrentCallList(CallInfo list)
        {
            Console.WriteLine(list.CallIndex);
            Console.WriteLine(list.CallMode);
            Console.WriteLine(list.Dir);
            Console.WriteLine(list.EntryInPhonebook);
            Console.WriteLine(list.IsMultipartyConferenceCall);
            Console.WriteLine(list.Number);
            Console.WriteLine(list.NumberType);
            Console.WriteLine(list.Status);
        }

        static void m_OnUSSDReceived(USSDStatus status, string message)
        {
            Console.WriteLine("USSD : " + status);
            Console.WriteLine(message);
        }

        private static void M_OnNetworkRegistrationChanged(NetworkRegistrationStatus status, string netLac, string netCellId)
        {
            Console.WriteLine(status);
            Console.WriteLine(netLac + " " + netCellId);
        }

        private static void M_OnNewSMSReceived(SMSStorage storage, SMS sms)
        {
            Console.WriteLine("New Message : " + storage);
            Console.WriteLine(sms.Sender + " : " + sms.Message);
            m.SMS_Delete(sms.MessageID);
        }
    }
}