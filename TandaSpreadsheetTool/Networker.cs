using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TandaSpreadsheetTool
{
    class Networker
    {

        NetworkStatus status = NetworkStatus.IDLE;

        string mostRecentError = "";

        protected string userNameKey = "iv7jlwbcx#hcq&*";

        string username = "";
        bool authenticated = false;

        JObject token;

        JObject roster;
        JArray teams;
        JArray staff;

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

        public bool LoadToken(string password)
        {
            if (File.Exists(RosterManager.Path + "data.wpn") & password != "")
            {
                if (password != "")
                {

                    try
                    {


                        using (FileStream file = new FileStream(RosterManager.Path + "data.wpn", FileMode.Open))
                        {
                            byte[] data = new byte[file.Length];

                            file.Read(data, 0, (int)file.Length);

                            var str = Encoding.UTF8.GetString(data);
                          
                            str = Decrypt(str, password);
                            password = "";

                            if (str == "FAILED")
                            {
                                return false;
                            }

                            token = JObject.Parse(str);
                        }

                       
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to Load Token");
                        mostRecentError = e.Message;
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
                    mostRecentError = ex.Message;
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn"))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "ud.wpn");
                }
                catch (Exception ex)
                {
                    mostRecentError = ex.Message;
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
                File.WriteAllBytes(RosterManager.Path + "ud.wpn", storedBytes);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to Save User Data");
                mostRecentError = ex.Message;
            }
        }

        public string LoadUsername()
        {
            if (!File.Exists(RosterManager.Path  + "ud.wpn"))
            {
                return "";
            }

            byte[] fileData =  new byte[1];
            string username ="";

            try
            {
                fileData = File.ReadAllBytes(RosterManager.Path + "ud.wpn");
                username = Decrypt(Encoding.UTF8.GetString(fileData), userNameKey);
                
            }
            
            catch(Exception e)
            {
                mostRecentError = e.Message;
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

            byte[] outBytes = Encoding.UTF8.GetBytes(cypher);

          

            try
            {
                File.WriteAllBytes(RosterManager.Path+ "data.wpn", outBytes);
            }
            catch(Exception e)
            {
                mostRecentError = e.Message;

                Console.WriteLine("Failed to save network information " + e.Message);
            }
        }

        public  string Encrypt(string data, string key)
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
                mostRecentError = ex.Message;
              
                return "FAILED";
            }
            
        }

        private  byte[] GetKey(string password)
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

        public  string Decrypt(string data, string key)
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
                mostRecentError = ex.Message;
                Console.WriteLine(ex.Message);
                return "FAILED";
            }
           
        }

       

#region NetworkRequests

        public async Task<bool> GetToken(string username, string password)
        {

            if (token != null | status != NetworkStatus.IDLE)
            {
                return false;
            }

            UpdateStatus = NetworkStatus.BUSY;

            client.DefaultRequestHeaders.Clear();



            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
                new KeyValuePair<string, string>("scope","roster user department"),
                new KeyValuePair<string,string>("grant_type","password")
            });







            try
            {
                Console.WriteLine(client.BaseAddress + "oauth/token/");
                httpresponse = await client.PostAsync("oauth/token/", formContent);
                var tokenStr = await httpresponse.Content.ReadAsStringAsync();



                token = JObject.Parse(tokenStr);

                this.username = username;

                SaveUserName(username);
                SaveToken(token, password);


            }
            catch (Exception ex)
            {
                mostRecentError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
                return false;
            }
            UpdateStatus = NetworkStatus.IDLE;


            return true;

        }

        public async Task<JObject> GetRooster(DateTime containingDate)
        {
            

            UpdateStatus= NetworkStatus.BUSY;
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));
            var payload = "";

            try
            {
                httpresponse = await client.GetAsync("v2/rosters/on/" + containingDate.ToString("yyyy-MM-dd"));
                
                 payload = await httpresponse.Content.ReadAsStringAsync();

                roster = JObject.Parse(payload);
               
            }
            catch (Exception ex)
            {
               
                mostRecentError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }

            UpdateStatus = NetworkStatus.IDLE;

            return roster;
        }

        public async Task<JArray> GetDepartments()
        {
            UpdateStatus = NetworkStatus.BUSY;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));
            try
            {
                httpresponse = await client.GetAsync("v2/departments");
                var payload = await httpresponse.Content.ReadAsStringAsync();
            
                teams = JArray.Parse(payload);
            }
            catch (Exception ex)
            {
              
               
                mostRecentError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }

            
            UpdateStatus = NetworkStatus.IDLE;

            return teams;
        }
    
        public async Task<JArray> GetStaff()
        {
            UpdateStatus = NetworkStatus.BUSY;
            client.DefaultRequestHeaders.Clear();


            client.DefaultRequestHeaders.Add("Authorization", "bearer " + token.GetValue("access_token"));


            try
            {
                
                    httpresponse = await client.GetAsync("v2/users");
                    var payload = await httpresponse.Content.ReadAsStringAsync();
                    staff = JArray.Parse(payload);

            }
            catch (Exception ex)
            {
            
                mostRecentError = ex.Message;
                UpdateStatus = NetworkStatus.ERROR;
            }

            
            UpdateStatus = NetworkStatus.IDLE;

            return staff;
        }
        #endregion


#region Acessors



        public JObject Roster
        {
            get
            {
                return roster;
            }
        }
      
        public JArray Teams
        {
            get
            {
                return teams;
            }
        }

        public JArray Staff
        {
            get
            {
                return staff;
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

        public string LastNetErrMsg
        {
            get
            {
                return mostRecentError;
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
        #endregion
        ~Networker()
        {
            client.CancelPendingRequests();
            client.Dispose();
            httpresponse.Dispose();

        }

        
        
    }
}
