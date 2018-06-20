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

       

        HttpClient client;

        public Networker()
        {
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 15);
        }

        public async void Connect(string username, string password)
        {
            Console.WriteLine(connected );
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

            string responseString = "";
            HttpResponseMessage httpresponse = new HttpResponseMessage();

            try
            {
                httpresponse = await client.PostAsync("", formContent);

                responseString = await httpresponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
                
           
            connected = httpresponse.IsSuccessStatusCode;

            networkerBusy = false;
        }

        public void PostRequest()
        {

        }

      
        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        ~Networker()
        {
            client.Dispose();
        }

        
        
    }
}
