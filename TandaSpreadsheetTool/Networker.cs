using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;

namespace TandaSpreadsheetTool
{
    class Networker
    {

        NetworkStatus status = NetworkStatus.IDLE;

        string mostRecentNetError = "";

        protected string userNameKey = "iv7jlwbcx#hcq&*";

        string username = "";

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
            client.BaseAddress = new Uri("https://my.tanda.co/api/");
        }

        public bool SignIn(string password)
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.wpn") & password != "")
            {
                if (password != "")
                {

                    try
                    {


                        using (FileStream file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "data.wpn", FileMode.Open))
                        {
                            byte[] data = new byte[file.Length];

                            file.Read(data, 0, (int)file.Length);

                            var str = "";

                            for (int i = 0; i < data.Length; i++)
                            {
                                str += (char)data[i];
                            }

                            str = Decrypt(str, password);

                            if (str == "FAILED")
                            {
                                return false;
                            }

                            token = JObject.Parse(str);
                        }

                       
                    }
                    catch
                    {
                        Console.WriteLine("Failed to read file");
                        return false;
                    }
                }


            }
            else
            {
                return false;
            }


            return true;
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


            
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("scope","roster"),
                new KeyValuePair<string,string>("grant_type","password")
            });

            
             
            

            

            try
            {
                httpresponse = await client.PostAsync("oauth/token/", formContent);
                var tokenStr = await httpresponse.Content.ReadAsStringAsync();

                token = JObject.Parse(tokenStr);

                this.username = username;

                SaveUserName(username);
                SaveToken(token, password);
                

            }
            catch (Exception ex)
            {                
                mostRecentNetError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }
            UpdateStatus = NetworkStatus.IDLE;
            
         
            
            
        }

        public void ClearFileData()
        {
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.wpn"))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "data.wpn");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn"))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn");
                }
                catch
                {

                }
                
            }

            token = null;
            username = "";
        }

        public void SaveUserName(string username)
        {            
           byte[] storedBytes=Encoding.UTF8.GetBytes(Encrypt(username, userNameKey));

            try
            {
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn", storedBytes);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to Save User Data");
            }
        }

        public string LoadUsername()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn"))
            {
                return "";
            }

            byte[] fileData =  new byte[1];
            string username ="";

            try
            {
                fileData = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn");
                username = Decrypt(Encoding.UTF8.GetString(fileData), userNameKey);
                
            }
            
            catch(Exception e)
            {

            }

             

            if (username == "FAILED" || username == "")
            {
                // add warning system here
            }
            else
            {
                this.username = username;
            }

            return username;
        }

        private void SaveToken(JObject token, string password)
        {
            string cypher = Encrypt(token.ToString(), password);

            byte[] outBytes = Encoding.UTF8.GetBytes(token.ToString());

          

            try
            {
                File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "data.wpn", outBytes);
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to save network information " + e.Message);
            }
        }

        public static string Encrypt(string data, string key)
        {
            var DES = new TripleDESCryptoServiceProvider();

            DES.Mode = CipherMode.ECB;
            
            DES.Key = GetKey(key);

            DES.Padding = PaddingMode.PKCS7;
            var DESEncrypt = DES.CreateEncryptor();
            var buffer = Encoding.UTF8.GetBytes(data);

            Console.WriteLine("Data input length: " + data.Length);

            try
            {
                string cypherText = Convert.ToBase64String(DESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                DES.Clear();
                DESEncrypt.Dispose();
                Array.Clear(buffer,0,buffer.Length);

                return cypherText;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

                return "FAILED";
            }
            
        }

        private static byte[] GetKey(string password)
        {
            string key = null;
            if (Encoding.UTF8.GetByteCount(password) < 24)
            {
                key = password.PadRight(24, ' ');
            }
            else
            {
                key = password.Substring(0, 24);
            }

            return Encoding.UTF8.GetBytes(key);
        }

        public static string Decrypt(string data, string key)
        {
            var DES = new TripleDESCryptoServiceProvider();
            DES.Mode = CipherMode.ECB;
            DES.Key = GetKey(key);

            

            DES.Padding = PaddingMode.PKCS7;
            var DESDecrypt = DES.CreateDecryptor();

            var buffer = Convert.FromBase64String(data.Replace(" ", "+"));

            
            try
            {
                string message = Encoding.UTF8.GetString(DESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                DES.Clear();
                DESDecrypt.Dispose();
                Array.Clear(buffer, 0, buffer.Length); 

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "FAILED";
            }
           
        }

        public string LastNetErrMsg
        {
            get
            {
                return mostRecentNetError;
            }
        }

        public async void GetRooster(string containingDate)
        {
            status = NetworkStatus.BUSY;
            client.DefaultRequestHeaders.Clear();
           

          

            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));
           

            try
            {
                httpresponse = await client.GetAsync("v2/rosters/on/" + containingDate);
                var payload = await httpresponse.Content.ReadAsStringAsync();

               var roosters = JObject.Parse(payload);
               SaveJSON(roosters);
            }
            catch (Exception ex)
            {
                mostRecentNetError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }

            UpdateStatus = NetworkStatus.IDLE;
        }

        void SaveJSON(JObject jObj)
        {
            try
            {
                using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "roosters.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    serializer.Serialize(file, jObj);
                }
            }

            catch
            {
                Console.WriteLine("Failed to save the roosters");
            }
        }

        private NetworkStatus UpdateStatus
        {
            set
            {
                status = value;
                
                for (int i = 0; i<listeners.Count;i++)
                {
                    listeners[i].NetStatusChanged(status);
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

       public bool Authenticated
        {
            get
            {
                return token != null;
            }
        }

        public string LastUser
        {
            get
            {
                return username;
            }
        }

        bool IsFirstUse
        {
            get
            {
                return !File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.wpn");
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
