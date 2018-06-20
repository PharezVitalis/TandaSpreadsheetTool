using System;
using System.Collections.Generic;
using System.IO;
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
        HttpResponseMessage httpresponse;

        public Networker()
        {
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            

            httpresponse = new HttpResponseMessage();
            httpresponse.EnsureSuccessStatusCode();

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

            client.DefaultRequestHeaders.Clear();


            client.BaseAddress = new Uri("https://my.tanda.co/api/");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("scope","leave unavailability roster"),
                new KeyValuePair<string,string>("grant_type","password")
            });

            
             
            

            

            try
            {
                httpresponse = await client.PostAsync("oauth/token/", formContent);
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

        public async void GetRooster(string containingDate)
        {
            status = NetworkStatus.BUSY;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "bearer" + token.GetValue("access_token"));
           

            try
            {
                httpresponse = await client.GetAsync("v2/rosters/on/" + containingDate);
                var payload = await httpresponse.Content.ReadAsStringAsync();

                var roosters = JObject.Parse(payload);

            }
            catch (Exception ex)
            {
                mostRecentError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }

            UpdateStatus = NetworkStatus.IDLE;
        }

        void SaveJSON(JObject jObj
        {
            
           
            using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory+"roosters.json")){
                JsonSerializer serializer = new JsonSerializer();

                serializer.Serialize(file, jObj);
            }
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
            httpresponse.Dispose();

        }

        
        
    }
}
