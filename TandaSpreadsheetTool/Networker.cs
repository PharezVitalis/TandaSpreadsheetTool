using System;
using System.Collections.Generic;
using System.Security;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TandaSpreadsheetTool
{
    class Networker
    {

        NetworkStatus status = NetworkStatus.IDLE;

        string mostRecentError = "";

        

        JObject token;

        List<INetworkListener> listeners;

        HttpClient client;

        public Networker()
        {
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            token = new SecureString();
            

            listeners = new List<INetworkListener>();

            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 30);

        }

        public void Subscribe(INetworkListener listener)
        {
            listeners.Add(listener);
        }

        public void Unsubscribe (INetworkListener listener)
        {
            listeners.Remove(listener);
        }

        public async void GetToken(string username, string password)
        {
           
            if (token != null | status != NetworkStatus.IDLE)
            {
                return;
            }

            UpdateStatus = NetworkStatus.BUSY;

           
            

            client.BaseAddress = new Uri("https://my.tanda.co/api/oauth/token/");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("scope","leave unavailability roster"),
                new KeyValuePair<string,string>("grant_type","password")
            });

            
            HttpResponseMessage httpresponse = new HttpResponseMessage();
            

            httpresponse.EnsureSuccessStatusCode();

            try
            {
                httpresponse = await client.PostAsync("", formContent);
                var tokenStr = await httpresponse.Content.ReadAsStringAsync();

                token = JObject.Parse(tokenStr);

                



            }
            catch (Exception ex)
            {                
                mostRecentError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }
            UpdateStatus = NetworkStatus.IDLE;
            
         
            
            
        }

        public string LastErrMsg
        {
            get
            {
                return mostRecentError;
            }
        }

        public void PostRequest(string dateFrom, string dateTo)
        {

        }

        private NetworkStatus UpdateStatus
        {
            set
            {
                status = value;
                
                foreach (INetworkListener listener in listeners)
                {
                    listener.NetStatusChanged(status);
                }
            }
        }       

        public NetworkStatus Status
        {
            get
            {
                return status;
            }
        }             

        ~Networker()
        {
            client.CancelPendingRequests();
            client.Dispose();

        }

        
        
    }
}
