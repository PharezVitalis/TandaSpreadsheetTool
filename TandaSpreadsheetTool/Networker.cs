using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace TandaSpreadsheetTool
{
    class Networker
    {

        bool networkerBusy = false;        

        bool connected = false;

        string status = "No connection attempt has been made";

        HttpClient client;

        public Networker()
        {
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            client = new HttpClient();
        }

        public async void Connect(string username, string password)
        {
            if (connected | networkerBusy)
            {
                return;
            }
            
            networkerBusy = true;

            client.BaseAddress = new Uri("https://my.tanda.co/api/oauth/token/");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("scope","leave unavailability roster"),
                new KeyValuePair<string,string>("grant_type","password")
            });

            
                var responsehttp = await client.PostAsync("", formContent);

            var responseString = await responsehttp.Content.ReadAsByteArrayAsync();
           
            connected = responsehttp.IsSuccessStatusCode;
            networkerBusy = false;
        }

       public string Response
        {
            get
            {
                return status;
            }
        }

        public bool Connected
        {
            get
            {
                return connected;
            }
        }
        
    }
}
